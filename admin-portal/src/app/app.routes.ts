import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent) },
  { path: 'dashboard', loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent), canActivate: [authGuard] },
  { path: 'projects', loadComponent: () => import('./pages/projects/projects.component').then(m => m.ProjectsComponent), canActivate: [authGuard] },
  { path: 'projects/new', loadComponent: () => import('./pages/create-project/create-project.component').then(m => m.CreateProjectComponent), canActivate: [authGuard] },
  { path: 'projects/:id', loadComponent: () => import('./pages/project-details/project-details.component').then(m => m.ProjectDetailsComponent), canActivate: [authGuard] },
  { path: '**', redirectTo: 'dashboard' }
];
