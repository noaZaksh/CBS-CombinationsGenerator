import { Component, inject } from '@angular/core';

import { CombinationsStateService } from '../../services/combinations-state.service';

/** Screen 2: the Reset / Next / All combinations buttons and the current combination. */
@Component({
  selector: 'app-single-view',
  templateUrl: './single-view.html',
})
export class SingleView {
  protected readonly state = inject(CombinationsStateService);

  protected format(combination: number[]): string {
    return `[${combination.join(',')}]`;
  }
}
