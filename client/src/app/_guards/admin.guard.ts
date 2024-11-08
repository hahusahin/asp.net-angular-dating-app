import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../_services/account.service';

export const adminGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const router = inject(Router);

  if (
    accountService.roles().includes('Admin') ||
    accountService.roles().includes('Moderator')
  ) {
    return true;
  } else {
    router.navigateByUrl('/');
    return false;
  }
};
