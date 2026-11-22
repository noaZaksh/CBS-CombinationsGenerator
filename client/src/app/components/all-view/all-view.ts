import { Component, inject } from '@angular/core';

import { CombinationsStateService } from '../../services/combinations-state.service';
import { CombinationList } from '../combination-list/combination-list';
import { Pager } from '../pager/pager';

/** Screen 3: one page of combinations, the pager and the Back button. */
@Component({
  selector: 'app-all-view',
  imports: [CombinationList, Pager],
  templateUrl: './all-view.html',
})
export class AllView {
  protected readonly state = inject(CombinationsStateService);
}
