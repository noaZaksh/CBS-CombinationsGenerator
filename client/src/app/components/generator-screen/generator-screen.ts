import { Component, inject } from '@angular/core';

import { CombinationsStateService } from '../../services/combinations-state.service';
import { AllView } from '../all-view/all-view';
import { SingleView } from '../single-view/single-view';
import { TotalCount } from '../total-count/total-count';

/** Everything after n was entered: the total line + the single view or the all view. */
@Component({
  selector: 'app-generator-screen',
  imports: [TotalCount, SingleView, AllView],
  templateUrl: './generator-screen.html',
})
export class GeneratorScreen {
  protected readonly state = inject(CombinationsStateService);
}
