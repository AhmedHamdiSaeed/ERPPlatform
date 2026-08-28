import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EnterpriseApiService, Project } from '../../core/services/api/enterprise-api.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './projects.component.html'
})
export class ProjectsComponent {
  private enterpriseApi = inject(EnterpriseApiService);
  private toast = inject(ToastService);

  projects = signal<Project[]>([]);
  showModal = signal(false);

  newProj: Partial<Project> = {
    name: '', clientName: '', budget: 50000, progressPercentage: 0, status: 'In Progress'
  };

  constructor() {
    this.loadData();
  }

  async loadData() {
    this.projects.set(await this.enterpriseApi.getProjects());
  }

  openAddModal() {
    this.newProj = {
      code: `PRJ-2026-00${this.projects().length + 1}`,
      name: '', clientName: '', budget: 100000, spentAmount: 0, progressPercentage: 0, status: 'In Progress',
      deadline: new Date(Date.now() + 90 * 86400000).toISOString().split('T')[0]
    };
    this.showModal.set(true);
  }

  async saveProject() {
    if (!this.newProj.name) {
      this.toast.warning('Please enter a project name.');
      return;
    }
    try {
      await this.enterpriseApi.createProject(this.newProj);
      this.toast.success('Project created and persisted to database.');
      this.showModal.set(false);
      await this.loadData();
    } catch (e) {
      this.toast.error('Failed to save project to database.');
    }
  }
}
