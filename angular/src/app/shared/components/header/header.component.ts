import { Component, inject, ElementRef, HostListener } from '@angular/core';
import { RouterModule } from '@angular/router';
import { StateService } from '../../../core/services/state.service';
import { FileImportService } from '../../../core/services/file-import.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterModule, TranslatePipe],
  templateUrl: './header.component.html'
})
export class HeaderComponent {
  state = inject(StateService);
  fileImport = inject(FileImportService);
  private elementRef = inject(ElementRef);

  showNotifDropdown = false;
  showUserMenu = false;

  toggleNotifDropdown(event: Event): void {
    event.stopPropagation();
    this.showNotifDropdown = !this.showNotifDropdown;
    if (this.showNotifDropdown) {
      this.showUserMenu = false;
    }
  }

  toggleUserMenu(event: Event): void {
    event.stopPropagation();
    this.showUserMenu = !this.showUserMenu;
    if (this.showUserMenu) {
      this.showNotifDropdown = false;
    }
  }

  closeAllDropdowns(): void {
    this.showNotifDropdown = false;
    this.showUserMenu = false;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (target && !this.elementRef.nativeElement.contains(target)) {
      this.closeAllDropdowns();
    }
  }
}

