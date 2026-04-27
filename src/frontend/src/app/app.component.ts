import { Component } from '@angular/core';
import { AppShellComponent } from './layout/app-shell.component';
import { RuntimeSessionService } from './core/services/runtime-session.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [AppShellComponent],
  template: '<app-shell />'
})
export class AppComponent {
  constructor(_: RuntimeSessionService) {}
}
