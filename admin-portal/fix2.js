const fs = require('fs');
const path = require('path');

const files = [
  'admin-login/admin-login.component.html', 
  'admin-dashboard/admin-dashboard.component.html', 
  'admin-owners/admin-owners.component.html', 
  'admin-owner-details/admin-owner-details.component.html', 
  'admin-projects/admin-projects.component.html', 
  'admin-project-details/admin-project-details.component.html'
];

files.forEach(f => { 
  const p = path.join('src/app/pages/admin', f); 
  let c = fs.readFileSync(p, 'utf8'); 
  c = c.split("[dir]=\"(languageService.currentLanguage$ | async) === 'ar' ? 'rtl' : 'ltr'\"").join(""); 
  fs.writeFileSync(p, c); 
});
console.log('Fixed');
