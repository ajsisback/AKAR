import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { AdminAuthService } from '../services/admin-auth.service';

export const adminGuard: CanActivateFn = () => {
  const adminAuthService = inject(AdminAuthService);
  const router = inject(Router);

  if (adminAuthService.isAuthenticated()) {
    return true;
  }

  router.navigate(['/admin/login']);
  return false;
};
