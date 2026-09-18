import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { Candidate, Department } from '../../../core/models/erp-models';
import { HrApiService } from '../../../core/services/api/hr-api.service';
import { ToastService } from '../../../core/services/toast.service';

type StageType = 'Applied' | 'Screening' | 'Interview' | 'Technical' | 'Offer' | 'Hired';

@Component({
  selector: 'app-recruitment-kanban',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './recruitment-kanban.component.html'
})
export class RecruitmentKanbanComponent implements OnInit {
  private hrApi = inject(HrApiService);
  private toast = inject(ToastService);

  candidates = signal<Candidate[]>([]);
  departments = signal<Department[]>([]);
  selectedCandidate = signal<Candidate | null>(null);
  loading = signal(false);

  showAddModal = signal(false);
  newCand: Partial<Candidate> = {
    name: '',
    email: '',
    phone: '',
    appliedPosition: '',
    experienceYears: 2,
    stage: 'Applied',
    rating: 4.0,
    expectedSalary: '',
    noticePeriod: '1 Month',
    notes: ''
  };

  showHireModal = signal(false);
  hireInput = {
    employeeCode: '',
    departmentId: '',
    departmentName: 'Engineering',
    agreedBasicSalary: 12000,
    housingAllowance: 2000,
    transportAllowance: 1000,
    joiningDate: new Date().toISOString().substring(0, 10),
    employmentType: 'FullTime',
    jobGradeName: 'Senior Specialist'
  };

  stages: StageType[] = ['Applied', 'Screening', 'Interview', 'Technical', 'Offer', 'Hired'];

  ngOnInit() {
    this.load();
  }

  async load() {
    this.loading.set(true);
    try {
      const [cands, depts] = await Promise.all([
        this.hrApi.getCandidates(),
        this.hrApi.getDepartments()
      ]);
      this.candidates.set(cands);
      this.departments.set(depts);
      if (depts.length > 0) {
        this.hireInput.departmentName = depts[0].name;
        this.hireInput.departmentId = depts[0].id;
      }
    } catch (e) {
      console.error('Failed to load recruitment data', e);
      this.toast.error('Could not load candidates from the server.');
    } finally {
      this.loading.set(false);
    }
  }

  getCandidatesByStage(stage: StageType) {
    return this.candidates().filter(c => c.stage === stage);
  }

  async moveStage(cand: Candidate, delta: number) {
    const idx = this.stages.indexOf(cand.stage as StageType);
    const newIdx = idx + delta;
    if (newIdx < 0 || newIdx >= this.stages.length) return;

    const nextStage = this.stages[newIdx];
    try {
      await this.hrApi.updateCandidateStage(cand.id, nextStage);
      this.candidates.update(list => list.map(c => c.id === cand.id ? { ...c, stage: nextStage } : c));
      this.toast.success(`${cand.name} moved to ${nextStage}.`);
      if (this.selectedCandidate()?.id === cand.id) {
        this.selectedCandidate.set({ ...cand, stage: nextStage });
      }
    } catch (e) {
      console.error('Failed to update candidate stage', e);
      this.toast.error('Could not update the candidate stage.');
    }
  }

  openAddModal() {
    this.newCand = {
      name: '',
      email: '',
      phone: '',
      appliedPosition: 'Software Engineer',
      experienceYears: 3,
      stage: 'Applied',
      rating: 4.5,
      expectedSalary: '$2,500',
      noticePeriod: '1 Month',
      notes: ''
    };
    this.showAddModal.set(true);
  }

  async saveCandidate() {
    if (!this.newCand.name || !this.newCand.email) {
      this.toast.warning('Name and Email are required.');
      return;
    }

    try {
      const created = await this.hrApi.createCandidate(this.newCand);
      this.candidates.update(list => [created, ...list]);
      this.toast.success('Candidate added to talent pipeline.');
      this.showAddModal.set(false);
    } catch (e) {
      console.error('Failed to create candidate', e);
      this.toast.error('Could not add candidate.');
    }
  }

  openHireModal(cand: Candidate) {
    this.hireInput = {
      employeeCode: `EMP-${Math.floor(1000 + Math.random() * 9000)}`,
      departmentId: this.departments().length > 0 ? this.departments()[0].id : '',
      departmentName: this.departments().length > 0 ? this.departments()[0].name : 'General',
      agreedBasicSalary: 15000,
      housingAllowance: 2500,
      transportAllowance: 1500,
      joiningDate: new Date().toISOString().substring(0, 10),
      employmentType: 'FullTime',
      jobGradeName: 'Grade 3 - Senior'
    };
    this.showHireModal.set(true);
  }

  async submitHire() {
    const cand = this.selectedCandidate();
    if (!cand) return;

    try {
      await this.hrApi.convertCandidateToEmployee(cand.id, this.hireInput);
      this.toast.success(`Successfully hired ${cand.name}! Employee record and active contract generated.`);
      this.showHireModal.set(false);
      this.selectedCandidate.set(null);
      await this.load();
    } catch (e) {
      console.error('Failed to convert candidate', e);
      this.toast.error('Failed to convert candidate to employee.');
    }
  }
}
