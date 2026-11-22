import { Component, inject } from '@angular/core';

import { GeneratorScreen } from './components/generator-screen/generator-screen';
import { StartScreen } from './components/start-screen/start-screen';
import { CombinationsStateService } from './services/combinations-state.service';

/** Root: shows the start screen or the generator screen. */
@Component({
  selector: 'app-root',
  imports: [StartScreen, GeneratorScreen],
  templateUrl: './app.html',
})
export class App {
  protected readonly state = inject(CombinationsStateService);
}
