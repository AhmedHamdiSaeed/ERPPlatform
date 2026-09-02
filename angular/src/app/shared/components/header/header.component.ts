import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { StateService } from '../../../core/services/state.service';
import { FileImportService } from '../../../core/services/file-import.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterModule, LocalizationPipe],
  templateUrl: './header.component.html'
})
export class HeaderComponent {
  state = inject(StateService);
  fileImport = inject(FileImportService);
  showNotifDropdown = false;
  showUserMenu = false;
}
