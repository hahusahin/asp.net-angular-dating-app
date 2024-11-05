import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { Photo } from '../_models/photo';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { of } from 'rxjs';
import { AccountService } from './account.service';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  private accountService = inject(AccountService);
  private http = inject(HttpClient);
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);
  memberCache = new Map();
  userParams = signal<UserParams>(
    new UserParams(this.accountService.currentUser())
  );

  baseUrl = environment.apiUrl;

  getMembers() {
    // Create a key string from the filters that are applied by the user and look at the map for this key if exists
    const response = this.memberCache.get(
      Object.values(this.userParams()).join('-')
    );
    if (response) return setPaginatedResponse(response, this.paginatedResult); // use the existing response from cache
    // continue to make new http request
    let params = setPaginationHeaders(
      this.userParams().pageNumber,
      this.userParams().pageSize
    );
    params = params.append('minAge', this.userParams().minAge);
    params = params.append('maxAge', this.userParams().maxAge);
    params = params.append('gender', this.userParams().gender);
    params = params.append('orderBy', this.userParams().orderBy);

    return this.http
      .get<Member[]>(this.baseUrl + 'users', { observe: 'response', params })
      .subscribe({
        next: (response) => {
          setPaginatedResponse(response, this.paginatedResult);
          this.memberCache.set(
            Object.values(this.userParams()).join('-'),
            response
          ); // cache the response to be further used
        },
      });
  }

  resetUserParams() {
    this.userParams.set(new UserParams(this.accountService.currentUser()));
  }

  getMember(username: string) {
    // Look at the cache first
    const member: Member = [...this.memberCache.values()]
      .reduce((arr, elem) => arr.concat(elem.body), [])
      .find((m: Member) => m.userName === username);
    if (member) return of(member);
    // If not exists in cache
    return this.http.get<Member>(`${this.baseUrl}users/${username}`);
  }

  updateMember(member: Member) {
    return this.http
      .put(this.baseUrl + 'users', member)
      .pipe
      // side effect: If update occurs, then empty cache of members array
      // tap(() => this.members.set([]))
      ();
  }

  setMainPhoto(photo: Photo) {
    return this.http
      .put(this.baseUrl + 'users/set-main-photo/' + photo.id, {})
      .pipe
      // tap(() => this.members.set([]))
      ();
  }

  deletePhoto(photoId: number) {
    return this.http
      .delete(this.baseUrl + 'users/delete-photo/' + photoId, {})
      .pipe
      // tap(() => this.members.set([]))
      ();
  }
}
