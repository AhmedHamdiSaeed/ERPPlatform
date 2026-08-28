import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { StateService } from '../../../core/services/state.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: './user-profile.component.html'
})
export class UserProfileComponent {
  state = inject(StateService);
  authService = inject(AuthService);
  toast = inject(ToastService);

  user = computed(() => this.state.currentUser());

  editMode = signal(false);
  editName = signal(this.user().name);
  editEmail = signal(this.user().email);

  toggleEdit() {
    this.editName.set(this.user().name);
    this.editEmail.set(this.user().email);
    this.editMode.set(!this.editMode());
  }

  saveProfile() {
    this.state.currentUser.update(u => ({
      ...u,
      name: this.editName(),
      email: this.editEmail()
    }));
    this.editMode.set(false);
    this.toast.success('User profile updated successfully.');
  }

  logout() {
    this.authService.logout();
  }
}
