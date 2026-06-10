const fs = require('fs');
const path = require('path');

const components = [
  'admin-login',
  'admin-dashboard',
  'admin-owners',
  'admin-owner-details',
  'admin-projects',
  'admin-project-details'
];

const basePath = path.join(__dirname, 'src/app/pages/admin');

for (const comp of components) {
  const dir = path.join(basePath, comp);
  const oldTs = path.join(dir, `${comp}.ts`);
  const oldHtml = path.join(dir, `${comp}.html`);
  const newTs = path.join(dir, `${comp}.component.ts`);
  const newHtml = path.join(dir, `${comp}.component.html`);

  // Rename
  if (fs.existsSync(oldTs)) fs.renameSync(oldTs, newTs);
  if (fs.existsSync(oldHtml)) fs.renameSync(oldHtml, newHtml);

  // Fix TS
  if (fs.existsSync(newTs)) {
    let tsContent = fs.readFileSync(newTs, 'utf8');
    tsContent = tsContent.replace(`templateUrl: './${comp}.html'`, `templateUrl: './${comp}.component.html'`);
    if (!tsContent.includes('TranslateModule')) {
      tsContent = tsContent.replace(/import \{ CommonModule \} from '@angular\/common';/g, "import { CommonModule } from '@angular/common';\nimport { TranslateModule, TranslateService } from '@ngx-translate/core';");
      tsContent = tsContent.replace(/imports: \[(.*?)\],/g, "imports: [$1, TranslateModule],");
    }
    
    // Fix TranslateService injection if needed
    if (tsContent.includes('languageService.instant(key)')) {
      tsContent = tsContent.replace('languageService.instant(key)', "translateService.instant(key)");
      tsContent = tsContent.replace('public languageService: LanguageService', "public languageService: LanguageService,\n    private translateService: TranslateService");
    }

    fs.writeFileSync(newTs, tsContent);
  }

  // Fix HTML
  if (fs.existsSync(newHtml)) {
    let htmlContent = fs.readFileSync(newHtml, 'utf8');
    htmlContent = htmlContent.replace(/\[dir\]="\([^"]+"\)/g, ""); // Remove [dir]="..."
    htmlContent = htmlContent.replace(/\[dir\]="'ltr'"/g, 'dir="ltr"');
    fs.writeFileSync(newHtml, htmlContent);
  }
}

console.log('Fixed components');
