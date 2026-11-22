import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { AppSettings } from '../../app.settings';
import { CombinationsStateService } from '../../services/combinations-state.service';

/** Screen 1: the user enters n. */
@Component({
  selector: 'app-start-screen',
  imports: [FormsModule],
  templateUrl: './start-screen.html',
})
export class StartScreen {
  protected readonly state = inject(CombinationsStateService);
  protected readonly minN = AppSettings.minN;
  protected readonly maxN = AppSettings.maxN;

  protected readonly value = signal<number | null>(null);
  protected readonly validationError = signal<string | null>(null);

  protected submit(): void {
    const n = this.value();
    if (n === null || !Number.isInteger(n) || n < this.minN || n > this.maxN) {
      this.validationError.set(`יש להזין מספר שלם בין ${this.minN} ל-${this.maxN}`);
      return;
    }
    this.validationError.set(null);
    this.state.start(n);
  }
}
