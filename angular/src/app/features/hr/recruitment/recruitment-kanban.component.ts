import { Component, inject, signal } from '@angular/core';
import { LocalizationPipe } from '@abp/ng.core';
import { Candidate } from '../../../core/models/erp-models';
import { RecruitmentApiService } from '../../../core/services/api/recruitment-api.service';
import { ToastService } from '../../../core/services/toast.service';

type StageType = 'Applied' | 'Screening' | 'Interview' | 'Technical' | 'Offer' | 'Hired';

@Component({
  selector: 'app-recruitment-kanban',
  standalone: true,
  imports: [LocalizationPipe],
  templateUrl: './recruitment-kanban.component.html'
})
export class RecruitmentKanbanComponent {
  private recruitApi = inject(RecruitmentApiService);
  private toast = inject(ToastService);

  candidates = signal<Candidate[]>([]);
  selectedCandidate = signal<Candidate | null>(null);
  loading = signal(false);

  stages: StageType[] = ['Applied', 'Screening', 'Interview', 'Technical', 'Offer', 'Hired'];

  constructor() {
    this.load();
  }

  async load() {
    this.loading.set(true);
    try {
      this.candidates.set(await this.recruitApi.getCandidates());
    } catch (e) {
      console.error('Failed to load candidates', e);
      this.toast.error('Could not load candidates from the server.');
    } finally {
      this.loading.set(false);
    }
  }

  getCandidatesByStage(stage: StageType) {
    return this.candidates().filter(c => c.stage === stage);
  }

  async moveStage(cand: Candidate, delta: number) {
    const idx = this.stages.indexOf(cand.stage);
    const newIdx = idx + delta;
    if (newIdx < 0 || newIdx >= this.stages.length) return;

    const nextStage = this.stages[newIdx];
    try {
      await this.recruitApi.updateStage(cand.id, nextStage);
      this.candidates.update(list => list.map(c => c.id === cand.id ? { ...c, stage: nextStage } : c));
      this.toast.success(`${cand.name} moved to ${nextStage}.`);
    } catch (e) {
      console.error('Failed to update candidate stage', e);
      this.toast.error('Could not update the candidate stage.');
    }
  }

  async addCandidate() {
    const payload = {
      name: 'Sherif Amer',
      email: 'sherif@example.com',
      phone: '+20 101 222 3344',
      appliedPosition: 'Fullstack Engineer',
      experienceYears: 4,
      stage: 'Applied' as const,
      rating: 4.2,
      skills: ['Angular', 'C#', 'PostgreSQL'],
      appliedDate: new Date().toISOString().split('T')[0]
    };
    try {
      const created = await this.recruitApi.createCandidate(payload);
      this.candidates.update(list => [created, ...list]);
      this.toast.success('Candidate added.');
    } catch (e) {
      console.error('Failed to create candidate', e);
      this.toast.error('Could not add the candidate.');
    }
  }
}
