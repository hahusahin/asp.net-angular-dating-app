import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  private http = inject(HttpClient);
  members = signal<Member[]>([]);

  baseUrl = environment.apiUrl;

  getMembers() {
    return this.http.get<Member[]>(this.baseUrl + 'users').subscribe({
      next: (members) => this.members.set(members),
    });
  }

  getMember(username: string) {
    return this.http.get<Member>(`${this.baseUrl}users/${username}`);
  }

  updateMember(member: Member) {
    return this.http
      .put(this.baseUrl + 'users', member)
      .pipe(tap(() => this.members.set([]))); // side effect: If update occurs, then empty cache of members array
  }
}
