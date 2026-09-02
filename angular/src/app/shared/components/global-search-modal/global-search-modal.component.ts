import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { StateService } from '../../../core/services/state.service';
import { GlobalSearchService, SearchResultGroup } from '../../../core/services/global-search.service';

@Component({
  selector: 'app-global-search-modal',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './global-search-modal.component.html'
})
export class GlobalSearchModalComponent {
  state = inject(StateService);
  searchService = inject(GlobalSearchService);
  router = inject(Router);

  query = '';
  results = signal<SearchResultGroup[]>([]);

  async onSearchChange(val: string) {
    this.results.set(await this.searchService.search(val));
  }

  navigateTo(link: string) {
    this.close();
    this.router.navigateByUrl(link);
  }

  close() {
    this.state.toggleGlobalSearch(false);
  }
}
