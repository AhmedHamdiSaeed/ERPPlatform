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
    public class PerformanceReviewDto : EntityDto<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string ReviewCycle { get; set; } = "2026 Annual";
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public decimal SelfRating { get; set; }
        public decimal ManagerRating { get; set; }
        public decimal FinalRating { get; set; }
        public string Status { get; set; } = "Draft";
        public decimal GoalsAchievedPercentage { get; set; }
        public string Strengths { get; set; } = string.Empty;
        public string AreasForImprovement { get; set; } = string.Empty;
        public bool PromotionRecommended { get; set; }
        public string ManagerFeedback { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
    }

    public class CreatePerformanceReviewDto
    {
        public Guid EmployeeId { get; set; }
        public string ReviewCycle { get; set; } = "2026 Annual";
        public DateTime PeriodStart { get; set; } = DateTime.UtcNow.AddMonths(-6);
        public DateTime PeriodEnd { get; set; } = DateTime.UtcNow;
        public string ReviewerName { get; set; } = string.Empty;
    }

    public class SubmitReviewEvaluationDto
    {
        public decimal SelfRating { get; set; }
        public decimal ManagerRating { get; set; }
        public decimal FinalRating { get; set; }
        public decimal GoalsAchievedPercentage { get; set; }
        public string Strengths { get; set; } = string.Empty;
        public string AreasForImprovement { get; set; } = string.Empty;
        public bool PromotionRecommended { get; set; }
        public string ManagerFeedback { get; set; } = string.Empty;
    }

    public class PerformanceGoalDto : EntityDto<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "Operational";
        public int Weight { get; set; } = 20;
        public decimal TargetValue { get; set; }
        public decimal CurrentValue { get; set; }
        public string MetricUnit { get; set; } = "%";
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "InProgress";
        public decimal Score { get; set; }
    }

    public class CreatePerformanceGoalDto
    {
        public Guid EmployeeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "Operational";
        public int Weight { get; set; } = 20;
        public decimal TargetValue { get; set; } = 100;
        public decimal CurrentValue { get; set; } = 0;
        public string MetricUnit { get; set; } = "%";
        public DateTime DueDate { get; set; } = DateTime.UtcNow.AddMonths(3);
    }

    public interface IPerformanceAppService : IApplicationService
    {
        Task<List<PerformanceReviewDto>> GetReviewsAsync(Guid? employeeId = null);
        Task<PerformanceReviewDto> CreateReviewAsync(CreatePerformanceReviewDto input);
        Task<PerformanceReviewDto> SubmitEvaluationAsync(Guid reviewId, SubmitReviewEvaluationDto input);
        Task<List<PerformanceGoalDto>> GetGoalsAsync(Guid? employeeId = null);
        Task<PerformanceGoalDto> CreateGoalAsync(CreatePerformanceGoalDto input);
        Task<PerformanceGoalDto> UpdateGoalProgressAsync(Guid goalId, decimal currentValue, string status);
    }

    public class PerformanceAppService : ApplicationService, IPerformanceAppService
    {
        private readonly IRepository<PerformanceReview, Guid> _reviewRepo;
        private readonly IRepository<PerformanceGoal, Guid> _goalRepo;
        private readonly IRepository<Employee, Guid> _employeeRepo;

        public PerformanceAppService(
            IRepository<PerformanceReview, Guid> reviewRepo,
            IRepository<PerformanceGoal, Guid> goalRepo,
            IRepository<Employee, Guid> employeeRepo)
        {
            _reviewRepo = reviewRepo;
            _goalRepo = goalRepo;
            _employeeRepo = employeeRepo;
        }

        public async Task<List<PerformanceReviewDto>> GetReviewsAsync(Guid? employeeId = null)
        {
            var query = await _reviewRepo.GetQueryableAsync();
            if (employeeId.HasValue && employeeId.Value != Guid.Empty)
            {
                query = query.Where(r => r.EmployeeId == employeeId.Value);
            }

            var reviews = query.OrderByDescending(r => r.CreationTime).ToList();
            return reviews.Select(MapReviewDto).ToList();
        }

        public async Task<PerformanceReviewDto> CreateReviewAsync(CreatePerformanceReviewDto input)
        {
            var emp = await _employeeRepo.FindAsync(input.EmployeeId);
            var review = new PerformanceReview
            {
                EmployeeId = input.EmployeeId,
                EmployeeName = emp?.Name ?? "Employee",
                ReviewCycle = input.ReviewCycle,
                PeriodStart = input.PeriodStart,
                PeriodEnd = input.PeriodEnd,
                ReviewerName = !string.IsNullOrWhiteSpace(input.ReviewerName) ? input.ReviewerName : (emp?.ManagerName ?? "Direct Manager"),
                Status = "Draft"
            };

            await _reviewRepo.InsertAsync(review);
            return MapReviewDto(review);
        }

        public async Task<PerformanceReviewDto> SubmitEvaluationAsync(Guid reviewId, SubmitReviewEvaluationDto input)
        {
            var review = await _reviewRepo.GetAsync(reviewId);
            review.SelfRating = input.SelfRating;
            review.ManagerRating = input.ManagerRating;
            review.FinalRating = input.FinalRating > 0 ? input.FinalRating : ((input.SelfRating + input.ManagerRating) / 2.0m);
            review.GoalsAchievedPercentage = input.GoalsAchievedPercentage;
            review.Strengths = input.Strengths;
            review.AreasForImprovement = input.AreasForImprovement;
            review.PromotionRecommended = input.PromotionRecommended;
            review.ManagerFeedback = input.ManagerFeedback;
            review.Status = "Completed";
            review.CompletedAt = DateTime.UtcNow;

            await _reviewRepo.UpdateAsync(review);
            return MapReviewDto(review);
        }

        public async Task<List<PerformanceGoalDto>> GetGoalsAsync(Guid? employeeId = null)
        {
            var query = await _goalRepo.GetQueryableAsync();
            if (employeeId.HasValue && employeeId.Value != Guid.Empty)
            {
                query = query.Where(g => g.EmployeeId == employeeId.Value);
            }

            var goals = query.OrderBy(g => g.DueDate).ToList();
            return goals.Select(MapGoalDto).ToList();
        }

        public async Task<PerformanceGoalDto> CreateGoalAsync(CreatePerformanceGoalDto input)
        {
            var emp = await _employeeRepo.FindAsync(input.EmployeeId);
            var goal = new PerformanceGoal
            {
                EmployeeId = input.EmployeeId,
                EmployeeName = emp?.Name ?? "Employee",
                Title = input.Title,
                Description = input.Description,
                Category = input.Category,
                Weight = input.Weight,
                TargetValue = input.TargetValue,
                CurrentValue = input.CurrentValue,
                MetricUnit = input.MetricUnit,
                DueDate = input.DueDate,
                Status = "InProgress"
            };

            await _goalRepo.InsertAsync(goal);
            return MapGoalDto(goal);
        }

        public async Task<PerformanceGoalDto> UpdateGoalProgressAsync(Guid goalId, decimal currentValue, string status)
        {
            var goal = await _goalRepo.GetAsync(goalId);
            goal.CurrentValue = currentValue;
            goal.Status = status;
            if (goal.TargetValue > 0 && goal.CurrentValue >= goal.TargetValue)
            {
                goal.Status = "Achieved";
                goal.Score = 5.0m;
            }

            await _goalRepo.UpdateAsync(goal);
            return MapGoalDto(goal);
        }

        private static PerformanceReviewDto MapReviewDto(PerformanceReview r) => new()
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            EmployeeName = r.EmployeeName,
            ReviewCycle = r.ReviewCycle,
            PeriodStart = r.PeriodStart,
            PeriodEnd = r.PeriodEnd,
            ReviewerName = r.ReviewerName,
            SelfRating = r.SelfRating,
            ManagerRating = r.ManagerRating,
            FinalRating = r.FinalRating,
            Status = r.Status,
            GoalsAchievedPercentage = r.GoalsAchievedPercentage,
            Strengths = r.Strengths,
            AreasForImprovement = r.AreasForImprovement,
            PromotionRecommended = r.PromotionRecommended,
            ManagerFeedback = r.ManagerFeedback,
            CompletedAt = r.CompletedAt
        };

        private static PerformanceGoalDto MapGoalDto(PerformanceGoal g) => new()
        {
            Id = g.Id,
            EmployeeId = g.EmployeeId,
            EmployeeName = g.EmployeeName,
            Title = g.Title,
            Description = g.Description,
            Category = g.Category,
            Weight = g.Weight,
            TargetValue = g.TargetValue,
            CurrentValue = g.CurrentValue,
            MetricUnit = g.MetricUnit,
            DueDate = g.DueDate,
            Status = g.Status,
            Score = g.Score
        };
    }
}
