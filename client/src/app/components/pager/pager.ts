import { Component, computed, input, output, signal } from '@angular/core';

import { AppSettings } from '../../app.settings';

/**
 * Pagination buttons only: first, previous, page numbers + jump to page, next, last.
 * Works with bigint because with n = 20 there can be ~10^17 pages.
 * It does not load anything – it only tells the parent which page was chosen.
 */
@Component({
  selector: 'app-pager',
  templateUrl: './pager.html',
})
export class Pager {
  readonly currentPage = input.required<bigint>();
  readonly totalPages = input.required<bigint>();
  readonly disabled = input(false);
  readonly pageChange = output<bigint>();

  protected readonly jumpText = signal('');
  protected readonly jumpError = signal<string | null>(null);

  protected readonly isFirst = computed(() => this.currentPage() <= 1n);
  protected readonly isLast = computed(() => this.currentPage() >= this.totalPages());

  /** A small window of page numbers around the current page, e.g. 3 4 [5] 6 7. */
  protected readonly pageNumbers = computed(() => {
    const total = this.totalPages();
    const count = BigInt(AppSettings.visiblePageNumbers);
    let from = this.currentPage() - count / 2n;
    if (from < 1n) from = 1n;
    let to = from + count - 1n;
    if (to > total) {
      to = total;
      from = to - count + 1n > 1n ? to - count + 1n : 1n;
    }
    const pages: bigint[] = [];
    for (let page = from; page <= to; page++) pages.push(page);
    return pages;
  });

  protected goTo(page: bigint): void {
    if (page >= 1n && page <= this.totalPages() && page !== this.currentPage()) {
      this.pageChange.emit(page);
    }
  }

  protected first(): void { this.goTo(1n); }
  protected previous(): void { this.goTo(this.currentPage() - 1n); }
  protected next(): void { this.goTo(this.currentPage() + 1n); }
  protected last(): void { this.goTo(this.totalPages()); }

  protected jump(): void {
    const text = this.jumpText().trim();
    const page = /^\d+$/.test(text) ? BigInt(text) : 0n;
    if (page < 1n || page > this.totalPages()) {
      this.jumpError.set(`יש להזין מספר דף בין 1 ל-${this.totalPages()}`);
      return;
    }
    this.jumpError.set(null);
    this.jumpText.set('');
    this.goTo(page);
  }
}
