import { Component, signal } from '@angular/core';
import { Candidate } from '../../../core/models/erp-models';
import { MOCK_CANDIDATES } from '../../../core/mock/mock-data';

type StageType = 'Applied' | 'Screening' | 'Interview' | 'Technical' | 'Offer' | 'Hired';

@Component({
  selector: 'app-recruitment-kanban',
  standalone: true,
  templateUrl: './recruitment-kanban.component.html'
})
export class RecruitmentKanbanComponent {
  candidates = signal<Candidate[]>(MOCK_CANDIDATES);
  selectedCandidate = signal<Candidate | null>(null);

  stages: StageType[] = ['Applied', 'Screening', 'Interview', 'Technical', 'Offer', 'Hired'];

  getCandidatesByStage(stage: StageType) {
    return this.candidates().filter(c => c.stage === stage);
  }

  moveStage(cand: Candidate, delta: number) {
    const idx = this.stages.indexOf(cand.stage);
    const newIdx = idx + delta;
    if (newIdx >= 0 && newIdx < this.stages.length) {
      const nextStage = this.stages[newIdx];
      this.candidates.update(list => list.map(c => c.id === cand.id ? { ...c, stage: nextStage } : c));
    }
  }

  addCandidate() {
    const newCand: Candidate = {
      id: `cand-${Date.now()}`,
      name: 'Sherif Amer',
      email: 'sherif@example.com',
      phone: '+20 101 222 3344',
      appliedPosition: 'Fullstack Engineer',
      experienceYears: 4,
      stage: 'Applied',
      rating: 4.2,
      skills: ['Angular', 'C#', 'PostgreSQL'],
      appliedDate: new Date().toISOString().split('T')[0]
    };
    this.candidates.update(list => [newCand, ...list]);
  }
}
