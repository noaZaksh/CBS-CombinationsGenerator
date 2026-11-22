import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';

import { AppSettings } from '../app.settings';
import { CombinationItem, PageResponse, Screen } from '../models/combinations.models';
import { CombinationsApiService } from './combinations-api.service';

/**
 * Holds what the screen shows (with Angular signals) and runs the user actions.
 * Components read the signals and call the methods – they never call the API directly.
 * No combinations are calculated here: everything comes from the server.
 */
@Injectable({ providedIn: 'root' })
export class CombinationsStateService {
  private readonly api = inject(CombinationsApiService);

  private sessionId: string | null = null;

  // ---- What the screen shows ----
  readonly screen = signal<Screen>('start');
  readonly n = signal<number | null>(null);
  readonly totalCountText = signal<string>('');
  readonly current = signal<CombinationItem | null>(null);
  readonly message = signal<string | null>(null);
  readonly page = signal<PageResponse | null>(null);
  readonly error = signal<string | null>(null);
  readonly loading = signal(false);

  /** Current page and page count as bigint, because with n = 20 they pass Number.MAX_SAFE_INTEGER. */
  readonly currentPage = computed(() => BigInt(this.page()?.pageNumber ?? '0'));
  readonly totalPages = computed(() => BigInt(this.page()?.totalPages ?? '0'));

  // ---- Actions ----

  start(n: number): void {
    this.run(this.api.start(n), (result) => {
      this.sessionId = result.sessionId;
      this.n.set(result.n);
      this.totalCountText.set(result.totalCountText);
      this.current.set(null);
      this.message.set(null);
      this.page.set(null);
      this.screen.set('single');
    });
  }

  reset(): void {
    this.sessionId = null;
    this.n.set(null);
    this.totalCountText.set('');
    this.current.set(null);
    this.message.set(null);
    this.page.set(null);
    this.error.set(null);
    this.screen.set('start');
  }

  next(): void {
    this.run(this.api.getNext(this.requireSession()), (result) => {
      if (result.hasCombination) {
        this.current.set(result.item);
        this.message.set(null);
      } else {
        this.message.set(result.message);
      }
    });
  }

  showAll(): void {
    this.loadPage(1n, () => this.screen.set('all'));
  }

  goToPage(pageNumber: bigint): void {
    this.loadPage(pageNumber);
  }

  back(): void {
    this.run(this.api.back(this.requireSession()), (result) => {
      this.current.set(result.hasCombination ? result.item : null);
      this.message.set(null);
      this.page.set(null);
      this.screen.set('single');
    });
  }

  // ---- Helpers ----

  private loadPage(pageNumber: bigint, afterLoad?: () => void): void {
    this.run(this.api.getAll(this.requireSession(), AppSettings.pageSize, pageNumber), (result) => {
      this.page.set(result);
      afterLoad?.();
    });
  }

  private requireSession(): string {
    if (!this.sessionId) {
      throw new Error('No session – call start() first.');
    }
    return this.sessionId;
  }

  /** Runs one server call: turns on "loading", handles errors in one place. */
  private run<T>(request: Observable<T>, onSuccess: (result: T) => void): void {
    this.loading.set(true);
    this.error.set(null);

    request.subscribe({
      next: (result) => {
        onSuccess(result);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err.status === 404) {
          this.reset();
          this.error.set('פג תוקף ההפעלה. יש להזין את n מחדש.');
        } else if (err.status === 0) {
          this.error.set('לא ניתן להתחבר לשרת. ודאו שהשרת רץ.');
        } else {
          this.error.set(err.error?.detail ?? 'אירעה שגיאה. נסו שוב.');
        }
      },
    });
  }
}
