using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.ApprovalCenter
{
    public class PendingApprovalDto
    {
        public Guid Id { get; set; }
        public string EntityType { get; set; } = string.Empty; // LeaveRequest, ExpenseRequest, SalesOrder, PurchaseRequest, WorkflowTask
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal? Amount { get; set; }
    }

    public class BatchApproveDto
    {
        public List<Guid> Ids { get; set; } = new();
        public string EntityType { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
    }

    public interface IApprovalCenterAppService : IApplicationService
    {
        Task<ListResultDto<PendingApprovalDto>> GetPendingApprovalsAsync();
        Task BatchApproveAsync(BatchApproveDto input);
    }

    public class ApprovalCenterAppService : ApplicationService, IApprovalCenterAppService
    {
        private readonly IRepository<LeaveRequest, Guid> _leaveRepo;
        private readonly IRepository<ExpenseRequest, Guid> _expenseRepo;
        private readonly IRepository<SalesOrder, Guid> _salesOrderRepo;
        private readonly IRepository<PurchaseRequest, Guid> _purchaseRequestRepo;
        private readonly IRepository<WorkflowTask, Guid> _workflowTaskRepo;

        public ApprovalCenterAppService(
            IRepository<LeaveRequest, Guid> leaveRepo,
            IRepository<ExpenseRequest, Guid> expenseRepo,
            IRepository<SalesOrder, Guid> salesOrderRepo,
            IRepository<PurchaseRequest, Guid> purchaseRequestRepo,
            IRepository<WorkflowTask, Guid> workflowTaskRepo)
        {
            _leaveRepo = leaveRepo;
            _expenseRepo = expenseRepo;
            _salesOrderRepo = salesOrderRepo;
            _purchaseRequestRepo = purchaseRequestRepo;
            _workflowTaskRepo = workflowTaskRepo;
        }

        public async Task<ListResultDto<PendingApprovalDto>> GetPendingApprovalsAsync()
        {
            var result = new List<PendingApprovalDto>();

            // Pending leave requests
            var leaves = await _leaveRepo.GetListAsync();
            result.AddRange(leaves.Where(l => l.Status == "Pending").Select(l => new PendingApprovalDto
            {
                Id = l.Id,
                EntityType = "LeaveRequest",
                Title = $"{l.LeaveType} Leave - {l.EmployeeName}",
                Description = $"{l.DaysCount} days from {l.StartDate:yyyy-MM-dd} to {l.EndDate:yyyy-MM-dd}. Reason: {l.Reason}",
                RequestedBy = l.EmployeeName,
                CreatedDate = l.CreationTime,
                Status = l.Status
            }));

            // Pending expense requests
            var expenses = await _expenseRepo.GetListAsync();
            result.AddRange(expenses.Where(e => e.Status == "Pending Approval").Select(e => new PendingApprovalDto
            {
                Id = e.Id,
                EntityType = "ExpenseRequest",
                Title = $"{e.Category} Expense - {e.EmployeeName}",
                Description = $"{e.Description} - Amount: ${e.Amount}",
                RequestedBy = e.EmployeeName,
                CreatedDate = e.CreationTime,
                Status = e.Status,
                Amount = e.Amount
            }));

            // Draft sales orders (need approval)
            var salesOrders = await _salesOrderRepo.GetListAsync();
            result.AddRange(salesOrders.Where(s => s.Status == "Draft").Select(s => new PendingApprovalDto
            {
                Id = s.Id,
                EntityType = "SalesOrder",
                Title = $"Sales Order {s.OrderNumber} - {s.CustomerName}",
                Description = $"Total: ${s.TotalAmount}",
                RequestedBy = s.CustomerName,
                CreatedDate = s.CreationTime,
                Status = s.Status,
                Amount = s.TotalAmount
            }));

            // Pending purchase requests
            var purchaseRequests = await _purchaseRequestRepo.GetListAsync();
            result.AddRange(purchaseRequests.Where(p => p.Status == "Pending Approval").Select(p => new PendingApprovalDto
            {
                Id = p.Id,
                EntityType = "PurchaseRequest",
                Title = $"PR {p.PrNumber} - {p.ItemName}",
                Description = $"Qty: {p.Quantity}, Est. Cost: ${p.EstimatedCost}",
                RequestedBy = p.RequestedBy,
                CreatedDate = p.CreationTime,
                Status = p.Status,
                Amount = p.EstimatedCost
            }));

            // Pending workflow tasks
            var workflowTasks = await _workflowTaskRepo.GetListAsync();
            result.AddRange(workflowTasks.Where(t => t.Status == "Pending").Select(t => new PendingApprovalDto
            {
                Id = t.Id,
                EntityType = "WorkflowTask",
                Title = $"{t.WorkflowName} - {t.TaskNumber}",
                Description = t.Details,
                RequestedBy = t.RequestedBy,
                CreatedDate = t.CreatedDate,
                Status = t.Status
            }));

            return new ListResultDto<PendingApprovalDto>(result.OrderByDescending(r => r.CreatedDate).ToList());
        }

        public async Task BatchApproveAsync(BatchApproveDto input)
        {
            foreach (var id in input.Ids)
            {
                switch (input.EntityType)
                {
                    case "LeaveRequest":
                        var leave = await _leaveRepo.GetAsync(id);
                        if (leave.Status == "Pending")
                        {
                            leave.Status = "Approved";
                            await _leaveRepo.UpdateAsync(leave);
                        }
                        break;
                    case "ExpenseRequest":
                        var expense = await _expenseRepo.GetAsync(id);
                        if (expense.Status == "Pending Approval")
                        {
                            expense.Status = "Approved";
                            await _expenseRepo.UpdateAsync(expense);
                        }
                        break;
                    case "SalesOrder":
                        var order = await _salesOrderRepo.GetAsync(id);
                        if (order.Status == "Draft")
                        {
                            order.Status = "Approved";
                            await _salesOrderRepo.UpdateAsync(order);
                        }
                        break;
                    case "PurchaseRequest":
                        var pr = await _purchaseRequestRepo.GetAsync(id);
                        if (pr.Status == "Pending Approval")
                        {
                            pr.Status = "Approved";
                            await _purchaseRequestRepo.UpdateAsync(pr);
                        }
                        break;
                    case "WorkflowTask":
                        var task = await _workflowTaskRepo.GetAsync(id);
                        if (task.Status == "Pending")
                        {
                            task.Status = "Approved";
                            task.Comments = input.Comments;
                            await _workflowTaskRepo.UpdateAsync(task);
                        }
                        break;
                }
            }
        }
    }
}
