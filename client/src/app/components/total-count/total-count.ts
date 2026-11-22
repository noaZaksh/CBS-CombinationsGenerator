import { Component, input } from '@angular/core';

/** "Total ways to order n numbers in a row is: X,XXX,XXX". The text comes ready from the server. */
@Component({
  selector: 'app-total-count',
  template: `<p class="total">סה"כ האפשרויות לסדר {{ n() }} מספרים בשורה הוא: <strong><bdi dir="ltr">{{ totalText() }}</bdi></strong></p>`,
})
export class TotalCount {
  readonly n = input.required<number>();
  readonly totalText = input.required<string>();
}
