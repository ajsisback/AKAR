import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly LANG_KEY = 'akar_lang';

  constructor(private translate: TranslateService) {
    this.translate.addLangs(['ar', 'en']);
    this.translate.setDefaultLang('ar');

    const savedLang = localStorage.getItem(this.LANG_KEY) || 'ar';
    this.setLanguage(savedLang);
  }

  setLanguage(lang: string): void {
    this.translate.use(lang);
    localStorage.setItem(this.LANG_KEY, lang);

    const dir = lang === 'ar' ? 'rtl' : 'ltr';
    const htmlEl = document.documentElement;
    htmlEl.setAttribute('dir', dir);
    htmlEl.setAttribute('lang', lang);
    document.body.style.direction = dir;
    document.body.style.textAlign = lang === 'ar' ? 'right' : 'left';
  }

  getCurrentLang(): string {
    return this.translate.currentLang || 'ar';
  }

  toggleLanguage(): void {
    const next = this.getCurrentLang() === 'ar' ? 'en' : 'ar';
    this.setLanguage(next);
  }

  isArabic(): boolean {
    return this.getCurrentLang() === 'ar';
  }
}
