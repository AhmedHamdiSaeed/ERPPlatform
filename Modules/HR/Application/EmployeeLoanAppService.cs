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
    public class EmployeeLoanDto : EntityDto<Guid>
    {
        public string LoanNumber { get; set; } = string.Empty;
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string LoanType { get; set; } = "Personal";
        public decimal PrincipalAmount { get; set; }
        public int TotalInstallments { get; set; }
        public decimal MonthlyInstallment { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal RemainingBalance { get; set; }
        public DateTime StartDeductionPeriod { get; set; }
        public string Status { get; set; } = "PendingApproval";
        public string ApprovedBy { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public List<LoanInstallmentDto> Installments { get; set; } = new();
    }

    public class LoanInstallmentDto : EntityDto<Guid>
    {
        public Guid LoanId { get; set; }
        public Guid EmployeeId { get; set; }
        public string Period { get; set; } = string.Empty;
        public int InstallmentNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsDeducted { get; set; }
        public DateTime? DeductedAt { get; set; }
    }

    public class RequestEmployeeLoanDto
    {
        public Guid EmployeeId { get; set; }
        public string LoanType { get; set; } = "Personal";
        public decimal PrincipalAmount { get; set; }
        public int TotalInstallments { get; set; } = 12;
        public DateTime StartDeductionPeriod { get; set; } = DateTime.UtcNow.AddMonths(1);
        public string Purpose { get; set; } = string.Empty;
    }

    public interface IEmployeeLoanAppService : ICrudAppService<EmployeeLoanDto, Guid, PagedAndSortedResultRequestDto, RequestEmployeeLoanDto>
    {
        Task<List<EmployeeLoanDto>> GetLoansByEmployeeIdAsync(Guid employeeId);
        Task<EmployeeLoanDto> ApproveLoanAsync(Guid loanId, string approvedBy);
        Task<EmployeeLoanDto> RejectLoanAsync(Guid loanId, string reason);
    }

    public class EmployeeLoanAppService : CrudAppService<EmployeeLoan, EmployeeLoanDto, Guid, PagedAndSortedResultRequestDto, RequestEmployeeLoanDto>, IEmployeeLoanAppService
    {
        private readonly IRepository<LoanInstallment, Guid> _installmentRepository;
        private readonly IRepository<Employee, Guid> _employeeRepository;

        public EmployeeLoanAppService(
            IRepository<EmployeeLoan, Guid> repository,
            IRepository<LoanInstallment, Guid> installmentRepository,
            IRepository<Employee, Guid> employeeRepository) : base(repository)
        {
            _installmentRepository = installmentRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<List<EmployeeLoanDto>> GetLoansByEmployeeIdAsync(Guid employeeId)
        {
            var list = await Repository.GetListAsync(l => l.EmployeeId == employeeId);
            var installments = await _installmentRepository.GetListAsync(i => i.EmployeeId == employeeId);

            return list.OrderByDescending(l => l.CreationTime).Select(l =>
            {
                var dto = MapEntityToDto(l);
                dto.Installments = installments.Where(i => i.LoanId == l.Id).OrderBy(i => i.InstallmentNumber).Select(MapInstallmentToDto).ToList();
                return dto;
            }).ToList();
        }

        public override async Task<EmployeeLoanDto> CreateAsync(RequestEmployeeLoanDto input)
        {
            var emp = await _employeeRepository.GetAsync(input.EmployeeId);
            var loanNum = $"LN-{DateTime.UtcNow:yyyyMM}-{new Random().Next(100, 999)}";

            var loan = new EmployeeLoan
            {
                LoanNumber = loanNum,
                EmployeeId = emp.Id,
                EmployeeName = emp.Name,
                LoanType = input.LoanType,
                PrincipalAmount = input.PrincipalAmount,
                TotalInstallments = input.TotalInstallments > 0 ? input.TotalInstallments : 12,
                TotalPaidAmount = 0,
                StartDeductionPeriod = input.StartDeductionPeriod,
                Status = "PendingApproval",
                Purpose = input.Purpose
            };

            await Repository.InsertAsync(loan);
            return MapEntityToDto(loan);
        }

        public async Task<EmployeeLoanDto> ApproveLoanAsync(Guid loanId, string approvedBy)
        {
            var loan = await Repository.GetAsync(loanId);
            loan.Status = "Active";
            loan.ApprovedBy = approvedBy;
            await Repository.UpdateAsync(loan);

            // Generate monthly installments schedule
            var monthlyAmt = loan.MonthlyInstallment;
            var curDate = loan.StartDeductionPeriod;

            var installments = new List<LoanInstallment>();
            for (int i = 1; i <= loan.TotalInstallments; i++)
            {
                var inst = new LoanInstallment
                {
                    LoanId = loan.Id,
                    EmployeeId = loan.EmployeeId,
                    Period = curDate.ToString("yyyy-MM"),
                    InstallmentNumber = i,
                    Amount = monthlyAmt,
                    DueDate = curDate,
                    IsDeducted = false
                };
                installments.Add(inst);
                curDate = curDate.AddMonths(1);
            }

            foreach (var inst in installments)
            {
                await _installmentRepository.InsertAsync(inst);
            }

            var dto = MapEntityToDto(loan);
            dto.Installments = installments.Select(MapInstallmentToDto).ToList();
            return dto;
        }

        public async Task<EmployeeLoanDto> RejectLoanAsync(Guid loanId, string reason)
        {
            var loan = await Repository.GetAsync(loanId);
            loan.Status = "Rejected";
            loan.Purpose = (loan.Purpose + $" | Rejected: {reason}").TrimStart('|', ' ');
            await Repository.UpdateAsync(loan);
            return MapEntityToDto(loan);
        }

        private static EmployeeLoanDto MapEntityToDto(EmployeeLoan l)
        {
            return new EmployeeLoanDto
            {
                Id = l.Id,
                LoanNumber = l.LoanNumber,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.EmployeeName,
                LoanType = l.LoanType,
                PrincipalAmount = l.PrincipalAmount,
                TotalInstallments = l.TotalInstallments,
                MonthlyInstallment = l.MonthlyInstallment,
                TotalPaidAmount = l.TotalPaidAmount,
                RemainingBalance = l.RemainingBalance,
                StartDeductionPeriod = l.StartDeductionPeriod,
                Status = l.Status,
                ApprovedBy = l.ApprovedBy,
                Purpose = l.Purpose
            };
        }

        private static LoanInstallmentDto MapInstallmentToDto(LoanInstallment i)
        {
            return new LoanInstallmentDto
            {
                Id = i.Id,
                LoanId = i.LoanId,
                EmployeeId = i.EmployeeId,
                Period = i.Period,
                InstallmentNumber = i.InstallmentNumber,
                Amount = i.Amount,
                DueDate = i.DueDate,
                IsDeducted = i.IsDeducted,
                DeductedAt = i.DeductedAt
            };
        }

        protected override Task<EmployeeLoanDto> MapToGetOutputDtoAsync(EmployeeLoan entity)
        {
            return Task.FromResult(MapEntityToDto(entity));
        }
    }
}
