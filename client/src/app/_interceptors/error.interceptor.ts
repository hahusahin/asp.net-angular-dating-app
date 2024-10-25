import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toastr = inject(ToastrService);

  return next(req).pipe(
    catchError((err) => {
      if (err) {
        switch (err?.status) {
          case 400:
            const errorsArr = err?.error?.errors;
            if (errorsArr) {
              const modelStateErrors = [];
              for (const key in errorsArr) {
                if (errorsArr[key]) {
                  modelStateErrors.push(errorsArr[key]);
                }
              }
              throw modelStateErrors.flat();
            } else {
              toastr.error(err?.error, err?.status);
            }
            break;
          case 401:
            toastr.error('Unauthorized', err?.status);
            break;
          case 404:
            router.navigateByUrl('/not-found');
            break;
          case 500:
            const navExtras: NavigationExtras = {
              state: { error: err?.error },
            };
            router.navigateByUrl('/server-error', navExtras);
            break;
          default:
            toastr.error('Something went wrong');
            break;
        }
      }
      throw err;
    })
  );
};
