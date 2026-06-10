import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent) },
  { path: 'dashboard', loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent), canActivate: [authGuard] },
  { path: 'projects', loadComponent: () => import('./pages/projects/projects.component').then(m => m.ProjectsComponent), canActivate: [authGuard] },
  { path: 'projects/new', loadComponent: () => import('./pages/create-project/create-project.component').then(m => m.CreateProjectComponent), canActivate: [authGuard] },
  { path: 'projects/:id', loadComponent: () => import('./pages/project-details/project-details.component').then(m => m.ProjectDetailsComponent), canActivate: [authGuard] },

  { path: 'admin', redirectTo: 'admin/dashboard', pathMatch: 'full' },
  { path: 'admin/login', loadComponent: () => import('./pages/admin/admin-login/admin-login.component').then(m => m.AdminLoginComponent) },
  { path: 'admin/dashboard', loadComponent: () => import('./pages/admin/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent), canActivate: [adminGuard] },
  { path: 'admin/owners', loadComponent: () => import('./pages/admin/admin-owners/admin-owners.component').then(m => m.AdminOwnersComponent), canActivate: [adminGuard] },
  { path: 'admin/owners/:id', loadComponent: () => import('./pages/admin/admin-owner-details/admin-owner-details.component').then(m => m.AdminOwnerDetailsComponent), canActivate: [adminGuard] },
  { path: 'admin/projects', loadComponent: () => import('./pages/admin/admin-projects/admin-projects.component').then(m => m.AdminProjectsComponent), canActivate: [adminGuard] },
  { path: 'admin/projects/:id', loadComponent: () => import('./pages/admin/admin-project-details/admin-project-details.component').then(m => m.AdminProjectDetailsComponent), canActivate: [adminGuard] },

  { path: '**', redirectTo: 'dashboard' }
];
