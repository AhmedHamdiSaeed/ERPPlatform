import { Component, inject } from '@angular/core';
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
  showNotifDropdown = false;
  showUserMenu = false;
}
