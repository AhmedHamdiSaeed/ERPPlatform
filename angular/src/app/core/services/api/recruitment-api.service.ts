import { Injectable } from '@angular/core';
import { ErpApiService, toDateString, AbpEntity } from './erp-api.service';
import { Candidate } from '../../models/erp-models';

interface CandidateDto extends AbpEntity {
  name: string;
  email: string;
  phone: string;
  appliedPosition: string;
  experienceYears: number;
  stage: Candidate['stage'];
  rating: number;
  skills: string[];
  appliedDate: string;
  notes: string;
}

export type CreateUpdateCandidate = Omit<Candidate, 'id'>;

@Injectable({ providedIn: 'root' })
export class RecruitmentApiService extends ErpApiService {
  getCandidates(stage?: string): Promise<Candidate[]> {
    const route = stage ? `candidate?stage=${encodeURIComponent(stage)}` : 'candidate';
    return this.getList<CandidateDto>(route).then(items =>
      items.map(c => ({
        id: c.id,
        name: c.name,
        email: c.email,
        phone: c.phone,
        appliedPosition: c.appliedPosition,
        experienceYears: c.experienceYears,
        stage: c.stage,
        rating: c.rating,
        skills: c.skills ?? [],
        appliedDate: toDateString(c.appliedDate),
        notes: c.notes
      })) as Candidate[]
    );
  }

  createCandidate(candidate: Partial<CreateUpdateCandidate>): Promise<Candidate> {
    return this.post<CandidateDto>('candidate', candidate).then(mapCandidate);
  }

  updateCandidate(id: string, candidate: Partial<CreateUpdateCandidate>): Promise<Candidate> {
    return this.put<CandidateDto>(`candidate/${id}`, candidate).then(mapCandidate);
  }

  // PUT /api/app/candidate/{id}/stage?newStage=
  updateStage(id: string, newStage: Candidate['stage']): Promise<Candidate> {
    return this.put<CandidateDto>(
      `candidate/${id}/stage?newStage=${encodeURIComponent(newStage)}`, {}
    ).then(mapCandidate);
  }

  deleteCandidate(id: string): Promise<void> {
    return this.delete(`candidate/${id}`);
  }
}

function mapCandidate(c: CandidateDto): Candidate {
  return {
    id: c.id,
    name: c.name,
    email: c.email,
    phone: c.phone,
    appliedPosition: c.appliedPosition,
    experienceYears: c.experienceYears,
    stage: c.stage,
    rating: c.rating,
    skills: c.skills ?? [],
    appliedDate: toDateString(c.appliedDate),
    notes: c.notes
  };
}
