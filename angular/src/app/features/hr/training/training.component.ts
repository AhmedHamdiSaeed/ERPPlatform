import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HrApiService } from '../../../core/services/api/hr-api.service';
import { TrainingCourse, TrainingEnrollment, EmployeeCertification, Employee } from '../../../core/models/erp-models';
import { StateService } from '../../../core/services/state.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-training',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './training.component.html'
})
export class TrainingComponent implements OnInit {
  private hrApi = inject(HrApiService);
  state = inject(StateService);

  activeTab: 'courses' | 'enrollments' | 'certifications' = 'courses';
  loading = false;
  courses: TrainingCourse[] = [];
  enrollments: TrainingEnrollment[] = [];
  certifications: EmployeeCertification[] = [];
  employees: Employee[] = [];

  // Modals
  showNewCourseModal = false;
  newCourse: Partial<TrainingCourse> = {
    courseCode: '',
    title: '',
    description: '',
    category: 'Technical',
    trainerName: '',
    durationHours: 16,
    costPerAttendee: 0,
    maxAttendees: 25,
    deliveryMethod: 'Online',
    passingScore: 70
  };

  showEnrollModal = false;
  enrollForm = {
    courseId: '',
    employeeId: ''
  };

  showGradingModal = false;
  selectedEnrollment: TrainingEnrollment | null = null;
  gradeForm = {
    score: 85,
    feedback: 'Completed coursework and demonstrated mastery.'
  };

  ngOnInit(): void {
    this.loadData();
  }

  async loadData(): Promise<void> {
    this.loading = true;
    try {
      const [crs, enrs, certs, emps] = await Promise.all([
        this.hrApi.getTrainingCourses(),
        this.hrApi.getTrainingEnrollments(),
        this.hrApi.getCertifications(),
        this.hrApi.getEmployees()
      ]);
      this.courses = crs || [];
      this.enrollments = enrs || [];
      this.certifications = certs || [];
      this.employees = emps || [];
    } catch (err) {
      console.error('Error loading training data', err);
    } finally {
      this.loading = false;
    }
  }

  openEnrollModal(course?: TrainingCourse): void {
    this.enrollForm = {
      courseId: course?.id || (this.courses.length > 0 ? this.courses[0].id : ''),
      employeeId: this.employees.length > 0 ? this.employees[0].id : ''
    };
    this.showEnrollModal = true;
  }

  async enroll(): Promise<void> {
    if (!this.enrollForm.courseId || !this.enrollForm.employeeId) return;
    try {
      await this.hrApi.enrollEmployee(this.enrollForm);
      this.showEnrollModal = false;
      this.loadData();
    } catch (err) {
      console.error('Error enrolling employee', err);
    }
  }

  openGradingModal(enr: TrainingEnrollment): void {
    this.selectedEnrollment = enr;
    this.gradeForm = {
      score: enr.score || 85,
      feedback: enr.feedback || 'Completed coursework successfully.'
    };
    this.showGradingModal = true;
  }

  async submitGrading(): Promise<void> {
    if (!this.selectedEnrollment) return;
    try {
      await this.hrApi.completeEnrollment(this.selectedEnrollment.id, this.gradeForm.score, this.gradeForm.feedback);
      this.showGradingModal = false;
      this.loadData();
    } catch (err) {
      console.error('Error grading enrollment', err);
    }
  }

  async createCourse(): Promise<void> {
    if (!this.newCourse.title) return;
    try {
      await this.hrApi.createTrainingCourse(this.newCourse);
      this.showNewCourseModal = false;
      this.loadData();
    } catch (err) {
      console.error('Error creating course', err);
    }
  }
}
