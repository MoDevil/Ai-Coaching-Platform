import { Component } from '@angular/core';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'AI Coach OS';

  constructor(public authService: AuthService) {}

  onLogout(): void {
    this.authService.logout();
  }
}
