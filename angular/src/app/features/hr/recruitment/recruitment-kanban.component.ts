import { Component, signal } from '@angular/core';
import { Candidate } from '../../../core/models/erp-models';
import { MOCK_CANDIDATES } from '../../../core/mock/mock-data';

type StageType = 'Applied' | 'Screening' | 'Interview' | 'Technical' | 'Offer' | 'Hired';

@Component({
  selector: 'app-recruitment-kanban',
  standalone: true,
  template: `
    <div class="space-y-6 animate-fade-in pb-8">
      
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="text-xl font-extrabold text-[var(--text-main)] tracking-tight">Recruitment & Candidate Pipeline</h1>
          <p class="text-xs text-[var(--text-muted)] mt-0.5">Kanban hiring board for tracking job applications, screening, technical interviews, and offers.</p>
        </div>

        <button (click)="addCandidate()" class="btn-primary text-xs cursor-pointer">
          <i class="pi pi-user-plus"></i> Add Candidate
        </button>
      </div>

      <!-- Kanban Pipeline Columns -->
      <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-4 items-start overflow-x-auto pb-4">
        
        @for (stage of stages; track stage) {
          <div class="bg-slate-100/70 dark:bg-slate-800/60 border border-slate-200 dark:border-slate-700/60 rounded-2xl p-3 space-y-3 min-w-[240px]">
            
            <!-- Column Header -->
            <div class="flex items-center justify-between pb-2 border-b border-slate-200 dark:border-slate-700">
              <span class="text-xs font-black uppercase tracking-wider text-slate-700 dark:text-slate-300">{{ stage }}</span>
              <span class="px-2 py-0.5 text-[10px] font-bold bg-white dark:bg-slate-700 rounded-full text-blue-600 shadow-2xs">
                {{ getCandidatesByStage(stage).length }}
              </span>
            </div>

            <!-- Candidate Cards in Stage -->
            <div class="space-y-3">
              @for (cand of getCandidatesByStage(stage); track cand.id) {
                <div 
                  (click)="selectedCandidate.set(cand)"
                  class="card-panel !p-3 bg-[var(--bg-card)] hover:border-blue-500 cursor-pointer shadow-xs hover:shadow-md transition-all space-y-2 group">
                  
                  <div class="flex items-start justify-between">
                    <div>
                      <h4 class="font-bold text-xs text-[var(--text-main)] group-hover:text-blue-600 transition-colors">{{ cand.name }}</h4>
                      <span class="text-[10px] text-blue-600 font-semibold block mt-0.5">{{ cand.appliedPosition }}</span>
                    </div>
                    <div class="flex items-center gap-0.5 text-amber-500 font-bold text-[10px]">
                      <i class="pi pi-star-fill text-[9px]"></i>
                      <span>{{ cand.rating }}</span>
                    </div>
                  </div>

                  <div class="flex flex-wrap gap-1">
                    @for (s of cand.skills.slice(0, 3); track s) {
                      <span class="px-1.5 py-0.5 text-[9px] bg-slate-100 dark:bg-slate-700 text-slate-600 dark:text-slate-300 rounded font-medium">
                        {{ s }}
                      </span>
                    }
                  </div>

                  <div class="pt-2 border-t border-slate-100 dark:border-slate-700/50 flex items-center justify-between text-[10px] text-slate-400">
                    <span>{{ cand.experienceYears }} yrs exp</span>
                    <div class="flex gap-1">
                      <button (click)="$event.stopPropagation(); moveStage(cand, -1)" class="p-1 hover:text-blue-600" title="Move Back"><i class="pi pi-arrow-left"></i></button>
                      <button (click)="$event.stopPropagation(); moveStage(cand, 1)" class="p-1 hover:text-blue-600" title="Move Forward"><i class="pi pi-arrow-right"></i></button>
                    </div>
                  </div>
                </div>
              }

              @if (getCandidatesByStage(stage).length === 0) {
                <div class="p-4 text-center text-[10px] text-slate-400 border border-dashed border-slate-300 dark:border-slate-700 rounded-xl">
                  No candidates in {{ stage }}
                </div>
              }
            </div>

          </div>
        }

      </div>

      <!-- Candidate Detail Drawer Modal -->
      @if (selectedCandidate()) {
        <div class="fixed inset-0 z-50 flex items-center justify-end bg-slate-900/60 backdrop-blur-xs animate-fade-in">
          <div class="w-full max-w-md h-full bg-[var(--bg-card)] border-l border-[var(--border-color)] p-6 shadow-2xl overflow-y-auto space-y-6">
            <div class="flex items-center justify-between border-b border-[var(--border-color)] pb-4">
              <h3 class="font-extrabold text-base text-[var(--text-main)]">Candidate Profile</h3>
              <button (click)="selectedCandidate.set(null)" class="text-slate-400 hover:text-slate-600"><i class="pi pi-times text-lg"></i></button>
            </div>

            <div class="space-y-4 text-xs">
              <div class="text-center pb-4 border-b border-[var(--border-color)]">
                <div class="w-16 h-16 rounded-full bg-blue-100 text-blue-600 font-black text-xl flex items-center justify-center mx-auto mb-2">
                  {{ selectedCandidate()?.name?.charAt(0) }}
                </div>
                <h2 class="font-black text-lg text-[var(--text-main)]">{{ selectedCandidate()?.name }}</h2>
                <span class="text-blue-600 font-bold block mt-0.5">{{ selectedCandidate()?.appliedPosition }}</span>
                <span class="status-badge active mt-2 inline-block">{{ selectedCandidate()?.stage }} Stage</span>
              </div>

              <div class="space-y-2">
                <div><span class="text-slate-400 block">Email:</span> <span class="font-bold">{{ selectedCandidate()?.email }}</span></div>
                <div><span class="text-slate-400 block">Phone:</span> <span class="font-bold">{{ selectedCandidate()?.phone }}</span></div>
                <div><span class="text-slate-400 block">Experience:</span> <span class="font-bold">{{ selectedCandidate()?.experienceYears }} Years</span></div>
                <div><span class="text-slate-400 block">Interviewer Rating:</span> <span class="font-bold text-amber-500">★ {{ selectedCandidate()?.rating }} / 5.0</span></div>
              </div>

              @if (selectedCandidate()?.notes) {
                <div class="p-3 bg-slate-50 dark:bg-slate-800 rounded-xl">
                  <span class="font-bold text-slate-700 dark:text-slate-300 block mb-1">Interview Notes:</span>
                  <p class="text-slate-600 dark:text-slate-300 leading-relaxed">{{ selectedCandidate()?.notes }}</p>
                </div>
              }

              <div class="pt-4 border-t border-[var(--border-color)] space-y-2">
                <button (click)="moveStage(selectedCandidate()!, 1)" class="w-full btn-primary text-xs justify-center py-2.5">Advance Candidate Stage</button>
                <button (click)="selectedCandidate.set(null)" class="w-full btn-outline text-xs justify-center py-2.5">Close Profile</button>
              </div>
            </div>
          </div>
        </div>
      }

    </div>
  `
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
