using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Modules.HR.Application
{
    public class TrainingCourseDto : EntityDto<Guid>
    {
        public string CourseCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "Technical";
        public string TrainerName { get; set; } = string.Empty;
        public int DurationHours { get; set; }
        public decimal CostPerAttendee { get; set; }
        public int MaxAttendees { get; set; }
        public string DeliveryMethod { get; set; } = "Online";
        public string Status { get; set; } = "Active";
        public decimal PassingScore { get; set; }
    }

    public class CreateTrainingCourseDto
    {
        public string CourseCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "Technical";
        public string TrainerName { get; set; } = string.Empty;
        public int DurationHours { get; set; } = 16;
        public decimal CostPerAttendee { get; set; } = 0;
        public int MaxAttendees { get; set; } = 25;
        public string DeliveryMethod { get; set; } = "Online";
        public decimal PassingScore { get; set; } = 70.0m;
    }

    public class TrainingEnrollmentDto : EntityDto<Guid>
    {
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string Status { get; set; } = "Enrolled";
        public decimal Score { get; set; }
        public bool CertificateIssued { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }

    public class EnrollEmployeeDto
    {
        public Guid CourseId { get; set; }
        public Guid EmployeeId { get; set; }
    }

    public class EmployeeCertificationDto : EntityDto<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string CertificationName { get; set; } = string.Empty;
        public string IssuingOrganization { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string CredentialId { get; set; } = string.Empty;
        public string CertificateUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }

    public class CreateEmployeeCertificationDto
    {
        public Guid EmployeeId { get; set; }
        public string CertificationName { get; set; } = string.Empty;
        public string IssuingOrganization { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; }
        public string CredentialId { get; set; } = string.Empty;
        public string CertificateUrl { get; set; } = string.Empty;
    }

    public interface ITrainingAppService : IApplicationService
    {
        Task<List<TrainingCourseDto>> GetCoursesAsync();
        Task<TrainingCourseDto> CreateCourseAsync(CreateTrainingCourseDto input);
        Task<List<TrainingEnrollmentDto>> GetEnrollmentsAsync(Guid? employeeId = null, Guid? courseId = null);
        Task<TrainingEnrollmentDto> EnrollEmployeeAsync(EnrollEmployeeDto input);
        Task<TrainingEnrollmentDto> CompleteEnrollmentAsync(Guid enrollmentId, decimal score, string feedback);
        Task<List<EmployeeCertificationDto>> GetCertificationsAsync(Guid? employeeId = null);
        Task<EmployeeCertificationDto> AddCertificationAsync(CreateEmployeeCertificationDto input);
    }

    public class TrainingAppService : ApplicationService, ITrainingAppService
    {
        private readonly IRepository<TrainingCourse, Guid> _courseRepo;
        private readonly IRepository<TrainingEnrollment, Guid> _enrollmentRepo;
        private readonly IRepository<EmployeeCertification, Guid> _certRepo;
        private readonly IRepository<Employee, Guid> _employeeRepo;

        public TrainingAppService(
            IRepository<TrainingCourse, Guid> courseRepo,
            IRepository<TrainingEnrollment, Guid> enrollmentRepo,
            IRepository<EmployeeCertification, Guid> certRepo,
            IRepository<Employee, Guid> employeeRepo)
        {
            _courseRepo = courseRepo;
            _enrollmentRepo = enrollmentRepo;
            _certRepo = certRepo;
            _employeeRepo = employeeRepo;
        }

        public async Task<List<TrainingCourseDto>> GetCoursesAsync()
        {
            var courses = await _courseRepo.GetListAsync();
            return courses.Select(c => new TrainingCourseDto
            {
                Id = c.Id,
                CourseCode = c.CourseCode,
                Title = c.Title,
                Description = c.Description,
                Category = c.Category,
                TrainerName = c.TrainerName,
                DurationHours = c.DurationHours,
                CostPerAttendee = c.CostPerAttendee,
                MaxAttendees = c.MaxAttendees,
                DeliveryMethod = c.DeliveryMethod,
                Status = c.Status,
                PassingScore = c.PassingScore
            }).ToList();
        }

        public async Task<TrainingCourseDto> CreateCourseAsync(CreateTrainingCourseDto input)
        {
            var course = new TrainingCourse
            {
                CourseCode = !string.IsNullOrWhiteSpace(input.CourseCode) ? input.CourseCode : $"CRS-{DateTime.UtcNow.Ticks % 10000:D4}",
                Title = input.Title,
                Description = input.Description,
                Category = input.Category,
                TrainerName = input.TrainerName,
                DurationHours = input.DurationHours,
                CostPerAttendee = input.CostPerAttendee,
                MaxAttendees = input.MaxAttendees,
                DeliveryMethod = input.DeliveryMethod,
                PassingScore = input.PassingScore,
                Status = "Active"
            };

            await _courseRepo.InsertAsync(course);
            return new TrainingCourseDto
            {
                Id = course.Id,
                CourseCode = course.CourseCode,
                Title = course.Title,
                Description = course.Description,
                Category = course.Category,
                TrainerName = course.TrainerName,
                DurationHours = course.DurationHours,
                CostPerAttendee = course.CostPerAttendee,
                MaxAttendees = course.MaxAttendees,
                DeliveryMethod = course.DeliveryMethod,
                Status = course.Status,
                PassingScore = course.PassingScore
            };
        }

        public async Task<List<TrainingEnrollmentDto>> GetEnrollmentsAsync(Guid? employeeId = null, Guid? courseId = null)
        {
            var query = await _enrollmentRepo.GetQueryableAsync();
            if (employeeId.HasValue && employeeId.Value != Guid.Empty)
                query = query.Where(e => e.EmployeeId == employeeId.Value);
            if (courseId.HasValue && courseId.Value != Guid.Empty)
                query = query.Where(e => e.CourseId == courseId.Value);

            var list = query.OrderByDescending(e => e.EnrollmentDate).ToList();
            return list.Select(e => new TrainingEnrollmentDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                CourseTitle = e.CourseTitle,
                EmployeeId = e.EmployeeId,
                EmployeeName = e.EmployeeName,
                EnrollmentDate = e.EnrollmentDate,
                CompletionDate = e.CompletionDate,
                Status = e.Status,
                Score = e.Score,
                CertificateIssued = e.CertificateIssued,
                Feedback = e.Feedback
            }).ToList();
        }

        public async Task<TrainingEnrollmentDto> EnrollEmployeeAsync(EnrollEmployeeDto input)
        {
            var emp = await _employeeRepo.GetAsync(input.EmployeeId);
            var course = await _courseRepo.GetAsync(input.CourseId);

            var enrollment = new TrainingEnrollment
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                EmployeeId = emp.Id,
                EmployeeName = emp.Name,
                EnrollmentDate = DateTime.UtcNow,
                Status = "Enrolled"
            };

            await _enrollmentRepo.InsertAsync(enrollment);
            return new TrainingEnrollmentDto
            {
                Id = enrollment.Id,
                CourseId = enrollment.CourseId,
                CourseTitle = enrollment.CourseTitle,
                EmployeeId = enrollment.EmployeeId,
                EmployeeName = enrollment.EmployeeName,
                EnrollmentDate = enrollment.EnrollmentDate,
                Status = enrollment.Status
            };
        }

        public async Task<TrainingEnrollmentDto> CompleteEnrollmentAsync(Guid enrollmentId, decimal score, string feedback)
        {
            var enrollment = await _enrollmentRepo.GetAsync(enrollmentId);
            var course = await _courseRepo.FindAsync(enrollment.CourseId);

            enrollment.Score = score;
            enrollment.Feedback = feedback;
            enrollment.CompletionDate = DateTime.UtcNow;

            var passingScore = course?.PassingScore ?? 70.0m;
            if (score >= passingScore)
            {
                enrollment.Status = "Completed";
                enrollment.CertificateIssued = true;

                // Auto issue employee certification
                await _certRepo.InsertAsync(new EmployeeCertification
                {
                    EmployeeId = enrollment.EmployeeId,
                    EmployeeName = enrollment.EmployeeName,
                    CertificationName = $"{enrollment.CourseTitle} Certificate of Completion",
                    IssuingOrganization = "ERP Corporate Academy",
                    IssueDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddYears(2),
                    CredentialId = $"CERT-{enrollment.Id.ToString().Substring(0, 8).ToUpper()}",
                    Status = "Active"
                });
            }
            else
            {
                enrollment.Status = "Failed";
                enrollment.CertificateIssued = false;
            }

            await _enrollmentRepo.UpdateAsync(enrollment);
            return new TrainingEnrollmentDto
            {
                Id = enrollment.Id,
                CourseId = enrollment.CourseId,
                CourseTitle = enrollment.CourseTitle,
                EmployeeId = enrollment.EmployeeId,
                EmployeeName = enrollment.EmployeeName,
                EnrollmentDate = enrollment.EnrollmentDate,
                CompletionDate = enrollment.CompletionDate,
                Status = enrollment.Status,
                Score = enrollment.Score,
                CertificateIssued = enrollment.CertificateIssued,
                Feedback = enrollment.Feedback
            };
        }

        public async Task<List<EmployeeCertificationDto>> GetCertificationsAsync(Guid? employeeId = null)
        {
            var query = await _certRepo.GetQueryableAsync();
            if (employeeId.HasValue && employeeId.Value != Guid.Empty)
                query = query.Where(c => c.EmployeeId == employeeId.Value);

            var list = query.OrderByDescending(c => c.IssueDate).ToList();
            var now = DateTime.UtcNow;
            return list.Select(c =>
            {
                var status = c.Status;
                if (c.ExpiryDate.HasValue)
                {
                    if (c.ExpiryDate.Value < now) status = "Expired";
                    else if (c.ExpiryDate.Value <= now.AddDays(30)) status = "ExpiringSoon";
                }
                return new EmployeeCertificationDto
                {
                    Id = c.Id,
                    EmployeeId = c.EmployeeId,
                    EmployeeName = c.EmployeeName,
                    CertificationName = c.CertificationName,
                    IssuingOrganization = c.IssuingOrganization,
                    IssueDate = c.IssueDate,
                    ExpiryDate = c.ExpiryDate,
                    CredentialId = c.CredentialId,
                    CertificateUrl = c.CertificateUrl,
                    Status = status
                };
            }).ToList();
        }

        public async Task<EmployeeCertificationDto> AddCertificationAsync(CreateEmployeeCertificationDto input)
        {
            var emp = await _employeeRepo.GetAsync(input.EmployeeId);
            var cert = new EmployeeCertification
            {
                EmployeeId = emp.Id,
                EmployeeName = emp.Name,
                CertificationName = input.CertificationName,
                IssuingOrganization = input.IssuingOrganization,
                IssueDate = input.IssueDate,
                ExpiryDate = input.ExpiryDate,
                CredentialId = !string.IsNullOrWhiteSpace(input.CredentialId) ? input.CredentialId : $"CERT-{DateTime.UtcNow.Ticks % 100000:D5}",
                CertificateUrl = input.CertificateUrl,
                Status = "Active"
            };

            await _certRepo.InsertAsync(cert);
            return new EmployeeCertificationDto
            {
                Id = cert.Id,
                EmployeeId = cert.EmployeeId,
                EmployeeName = cert.EmployeeName,
                CertificationName = cert.CertificationName,
                IssuingOrganization = cert.IssuingOrganization,
                IssueDate = cert.IssueDate,
                ExpiryDate = cert.ExpiryDate,
                CredentialId = cert.CredentialId,
                CertificateUrl = cert.CertificateUrl,
                Status = cert.Status
            };
        }
    }
}
