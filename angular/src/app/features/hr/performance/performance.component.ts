import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HrApiService } from '../../../core/services/api/hr-api.service';
import { PerformanceReview, PerformanceGoal, Employee } from '../../../core/models/erp-models';
import { StateService } from '../../../core/services/state.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-performance',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './performance.component.html'
})
export class PerformanceComponent implements OnInit {
  private hrApi = inject(HrApiService);
  state = inject(StateService);

  activeTab: 'reviews' | 'goals' = 'reviews';
  loading = false;
  reviews: PerformanceReview[] = [];
  goals: PerformanceGoal[] = [];
  employees: Employee[] = [];

  // Evaluation Modal
  showEvalModal = false;
  selectedReview: PerformanceReview | null = null;
  evalForm = {
    selfRating: 4.0,
    managerRating: 4.0,
    finalRating: 4.0,
    goalsAchievedPercentage: 85,
    strengths: '',
    areasForImprovement: '',
    promotionRecommended: false,
    managerFeedback: ''
  };

  // New Review Modal
  showNewReviewModal = false;
  newReview = {
    employeeId: '',
    reviewCycle: '2026 Annual',
    periodStart: '2026-01-01',
    periodEnd: '2026-12-31',
    reviewerName: ''
  };

  // New Goal Modal
  showNewGoalModal = false;
  newGoal = {
    employeeId: '',
    title: '',
    description: '',
    category: 'Operational',
    weight: 20,
    targetValue: 100,
    metricUnit: '%',
    dueDate: '2026-12-31'
  };

  get completedReviewsCount(): number {
    return this.reviews.filter(r => r.status === 'Completed').length;
  }

  get promotionRecCount(): number {
    return this.reviews.filter(r => r.promotionRecommended).length;
  }

  ngOnInit(): void {
    this.loadData();
  }

  async loadData(): Promise<void> {
    this.loading = true;
    try {
      const [revs, gls, emps] = await Promise.all([
        this.hrApi.getPerformanceReviews(),
        this.hrApi.getPerformanceGoals(),
        this.hrApi.getEmployees()
      ]);
      this.reviews = revs || [];
      this.goals = gls || [];
      this.employees = emps || [];
    } catch (err) {
      console.error('Error loading performance data', err);
    } finally {
      this.loading = false;
    }
  }

  openEvalModal(review: PerformanceReview): void {
    this.selectedReview = review;
    this.evalForm = {
      selfRating: review.selfRating || 4.0,
      managerRating: review.managerRating || 4.0,
      finalRating: review.finalRating || 4.0,
      goalsAchievedPercentage: review.goalsAchievedPercentage || 85,
      strengths: review.strengths || '',
      areasForImprovement: review.areasForImprovement || '',
      promotionRecommended: review.promotionRecommended || false,
      managerFeedback: review.managerFeedback || ''
    };
    this.showEvalModal = true;
  }

  async submitEvaluation(): Promise<void> {
    if (!this.selectedReview) return;
    try {
      await this.hrApi.submitPerformanceEvaluation(this.selectedReview.id, this.evalForm);
      this.showEvalModal = false;
      this.loadData();
    } catch (err) {
      console.error('Error submitting evaluation', err);
    }
  }

  async createReview(): Promise<void> {
    if (!this.newReview.employeeId) return;
    try {
      await this.hrApi.createPerformanceReview(this.newReview);
      this.showNewReviewModal = false;
      this.loadData();
    } catch (err) {
      console.error('Error creating review', err);
    }
  }

  async createGoal(): Promise<void> {
    if (!this.newGoal.employeeId || !this.newGoal.title) return;
    try {
      await this.hrApi.createPerformanceGoal(this.newGoal);
      this.showNewGoalModal = false;
      this.loadData();
    } catch (err) {
      console.error('Error creating goal', err);
    }
  }

  async updateGoalProgress(goal: PerformanceGoal, delta: number): Promise<void> {
    const newVal = Math.max(0, Math.min(goal.targetValue, goal.currentValue + delta));
    const status = newVal >= goal.targetValue ? 'Achieved' : 'InProgress';
    try {
      await this.hrApi.updateGoalProgress(goal.id, newVal, status);
      goal.currentValue = newVal;
      goal.status = status;
    } catch (err) {
      console.error('Error updating progress', err);
    }
  }
}
