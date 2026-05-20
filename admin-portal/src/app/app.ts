import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { LanguageService } from './core/services/language.service';
import { AuthService } from './core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, TranslateModule, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  constructor(
    public lang: LanguageService,
    public auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Language service initializes itself
  }

  toggleLanguage(): void {
    this.lang.toggleLanguage();
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
