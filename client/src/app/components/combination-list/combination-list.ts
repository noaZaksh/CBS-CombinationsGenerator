import { Component, input } from '@angular/core';

import { CombinationItem } from '../../models/combinations.models';

/** A plain list of combinations with their serial numbers. */
@Component({
  selector: 'app-combination-list',
  templateUrl: './combination-list.html',
})
export class CombinationList {
  readonly items = input.required<CombinationItem[]>();

  protected format(combination: number[]): string {
    return `[${combination.join(',')}]`;
  }
}
