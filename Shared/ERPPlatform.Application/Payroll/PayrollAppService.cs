using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ERPPlatform.Domain.Entities;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Payroll
{
    /// <summary>Payroll run lifecycle. Once finalized a run can only be corrected by reversal.</summary>
    public static class PayrollStatus
    {
        public const string Draft = "Draft";
        public const string Calculated = "Calculated";
        public const string HrReview = "HR Review";
        public const string FinanceReview = "Finance Review";
        public const string Finalized = "Finalized";
        public const string Posted = "Posted";

        public static bool IsLocked(string status) => status == Finalized || status == Posted;
    }

    public class PayrollRunDto : EntityDto<Guid>
    {
        public string Period { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalAllowances { get; set; }
        public decimal TotalOvertime { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }
        public decimal TotalEmployerCost { get; set; }
        public string Status { get; set; } = "Approved";
        public DateTime ProcessedDate { get; set; }
        public DateTime? LockedAt { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class PayslipDto : EntityDto<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal OvertimeAmount { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public decimal EmployerCost { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = "Paid";
        public Guid? PayrollRunId { get; set; }
        public List<PayrollLineDto> Lines { get; set; } = new();
    }

    public class PayrollLineDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Earning";
        public decimal Amount { get; set; }
        public bool IsTaxable { get; set; }
    }

    public class PayrollEmployeeResultDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        public decimal Basic { get; set; }
        public decimal Allowances { get; set; }
        public decimal OvertimeAmount { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal TaxAndInsurance { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public decimal EmployerCost { get; set; }

        public decimal WorkingHours { get; set; }
        public decimal OvertimeHours { get; set; }
        public int UnpaidLeaveDays { get; set; }

        public decimal PreviousNetSalary { get; set; }
        public decimal NetDelta { get; set; }
        public int NetDeltaPercentage { get; set; }

        public List<PayrollLineDto> Lines { get; set; } = new();
    }

    public class PayrollTotalsDto
    {
        public int EmployeeCount { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Overtime { get; set; }
        public decimal TaxAndInsurance { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public decimal EmployerCost { get; set; }
    }

    public class PayrollPreviewDto
    {
        public string Period { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        public List<PayrollEmployeeResultDto> Employees { get; set; } = new();
        public PayrollTotalsDto Totals { get; set; } = new();
        public PayrollTotalsDto? PreviousTotals { get; set; }
        public List<PayrollAnomalyDto> Anomalies { get; set; } = new();

        /// <summary>Headline deltas against the previous period, already rounded.</summary>
        public decimal NetDelta { get; set; }
        public decimal GrossDelta { get; set; }
        public int HeadcountDelta { get; set; }

        public int ComponentCount { get; set; }
        /// <summary>True when no salary components are configured — the engine then falls back to basic salary only.</summary>
        public bool UsingFallback { get; set; }
        public bool HasPreviousRun { get; set; }
    }

    public class PayrollAnomalyDto : EntityDto<Guid>
    {
        public string Period { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string Severity { get; set; } = "Warning";
        public string Message { get; set; } = string.Empty;
        public decimal Delta { get; set; }
        public bool IsResolved { get; set; }
        public string ResolutionNote { get; set; } = string.Empty;
    }

    public class ResolveAnomalyInput
    {
        public string Note { get; set; } = string.Empty;
    }

    public interface IPayrollAppService : IApplicationService
    {
        Task<ListResultDto<PayrollRunDto>> GetPayrollRunsAsync();
        Task<PayrollRunDto> ProcessPayrollRunAsync(string period);
        Task<ListResultDto<PayslipDto>> GetPayslipsAsync(string period);
        Task<PayrollPreviewDto> GetPreviewAsync(string period);
        Task<ListResultDto<PayrollAnomalyDto>> GetAnomaliesAsync(string period);
        Task<PayrollAnomalyDto> ResolveAnomalyAsync(Guid id, ResolveAnomalyInput input);
        Task<PayrollRunDto> AdvanceStatusAsync(Guid id);
        Task<PayrollRunDto> ReopenAsync(Guid id);
    }

    /// <summary>What the engine produces for one period: per-employee lines, flagged issues, and config depth.</summary>
    public class PayrollCalculationResult
    {
        public List<PayrollEmployeeResultDto> Employees { get; set; } = new();
        public List<PayrollAnomalyDto> Anomalies { get; set; } = new();
        public int ComponentCount { get; set; }
    }

    /// <summary>
    /// AutoMapper maps are discovered automatically because the application module calls
    /// AbpAutoMapperOptions.AddMaps&lt;ERPPlatformApplicationModule&gt;().
    /// </summary>
    public class PayrollAutoMapperProfile : Profile
    {
        public PayrollAutoMapperProfile()
        {
            // CreationTime/CreatorId are owned by ABP's audit interceptor — never accept
            // them back from the client, or every update stamps 0001-01-01.
            CreateMap<SalaryComponent, SalaryComponentDto>()
                .ReverseMap()
                .ForMember(x => x.CreationTime, o => o.Ignore());

            CreateMap<EmployeeSalaryComponent, EmployeeSalaryComponentDto>()
                .ReverseMap()
                .ForMember(x => x.CreationTime, o => o.Ignore());
        }
    }

    /// <summary>
    /// The calculation engine. Shared by the preview (no writes) and the real run, so what
    /// HR previews is exactly what gets paid.
    /// </summary>
    public class PayrollCalculator
    {
        public const int WorkingDaysPerMonth = 22;
        public const decimal HoursPerDay = 8m;
        public const decimal OvertimeMultiplier = 1.5m;
        public const decimal SalarySpikeThreshold = 0.40m;
        public const decimal HighOvertimeHours = 40m;
        public const decimal UnexpectedDeductionRatio = 0.50m;
        public const decimal HighBonusRatio = 0.25m;

        public List<PayrollEmployeeResultDto> Employees { get; } = new();
        public List<PayrollAnomalyDto> Anomalies { get; } = new();

        public void Calculate(
            IReadOnlyList<Employee> employees,
            IReadOnlyList<SalaryComponent> components,
            IReadOnlyList<EmployeeSalaryComponent> overrides,
            IReadOnlyDictionary<Guid, decimal> workingHours,
            IReadOnlyDictionary<Guid, decimal> overtimeHours,
            IReadOnlyDictionary<Guid, int> unpaidLeaveDays,
            IReadOnlyDictionary<Guid, decimal> previousNet,
            string period)
        {
            var active = components
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Code)
                .ToList();

            // Duplicate detection is by name — flagged, never auto-merged.
            var duplicates = employees
                .GroupBy(e => (e.Name ?? string.Empty).Trim().ToLowerInvariant())
                .Where(g => g.Key.Length > 0 && g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            foreach (var employee in employees)
            {
                var lines = new List<PayrollLineDto>();
                var wh = workingHours.TryGetValue(employee.Id, out var w) ? w : 0m;
                var oh = overtimeHours.TryGetValue(employee.Id, out var o) ? o : 0m;
                var unpaid = unpaidLeaveDays.TryGetValue(employee.Id, out var u) ? u : 0;

                // ── Basic ─────────────────────────────────────────────
                var basic = ValueFor(employee, "BASIC", active, overrides, fallback: employee.Salary);
                lines.Add(new PayrollLineDto
                {
                    Code = "BASIC",
                    Name = "Basic Salary",
                    Type = "Earning",
                    Amount = Round(basic),
                    IsTaxable = true
                });

                var earned = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
                {
                    ["BASIC"] = basic
                };

                // ── Earnings ──────────────────────────────────────────
                foreach (var c in active.Where(c => c.Type == "Earning" && c.Code != "BASIC"))
                {
                    var amount = ValueFor(employee, c.Code, active, overrides, fallback: 0m);
                    if (amount == 0m) continue;

                    var value = c.CalculationType == "Percentage"
                        ? amount / 100m * earned.GetValueOrDefault(c.PercentageOfComponentCode, basic)
                        : amount;

                    value = Round(value);
                    earned[c.Code] = value;
                    lines.Add(new PayrollLineDto
                    {
                        Code = c.Code,
                        Name = c.Name,
                        Type = "Earning",
                        Amount = value,
                        IsTaxable = c.IsTaxable
                    });
                }

                // ── Overtime, derived from attendance ─────────────────
                decimal overtimeAmount = 0m;
                if (oh > 0 && basic > 0)
                {
                    var hourlyRate = basic / (WorkingDaysPerMonth * HoursPerDay);
                    overtimeAmount = Round(oh * hourlyRate * OvertimeMultiplier);
                    lines.Add(new PayrollLineDto
                    {
                        Code = "OVERTIME",
                        Name = "Overtime",
                        Type = "Earning",
                        Amount = overtimeAmount,
                        IsTaxable = true
                    });
                }

                var gross = Round(lines.Where(l => l.Type == "Earning").Sum(l => l.Amount));
                var taxableBase = Round(lines.Where(l => l.Type == "Earning" && l.IsTaxable).Sum(l => l.Amount));

                // ── Deductions ────────────────────────────────────────
                decimal taxAndInsurance = 0m;
                decimal otherDeductions = 0m;

                foreach (var c in active.Where(c => c.Type == "Deduction"))
                {
                    var configured = ValueFor(employee, c.Code, active, overrides, fallback: 0m);
                    if (configured == 0m) continue;

                    var value = c.CalculationType == "Percentage"
                        ? Round(configured / 100m * taxableBase)
                        : Round(configured);

                    // On a deduction, IsTaxable marks it as statutory (income tax / social
                    // insurance) so it can be reported separately from other withholdings.
                    if (c.IsTaxable) taxAndInsurance += value; else otherDeductions += value;

                    lines.Add(new PayrollLineDto
                    {
                        Code = c.Code,
                        Name = c.Name,
                        Type = "Deduction",
                        Amount = value,
                        IsTaxable = c.IsTaxable
                    });
                }

                // ── Unpaid leave ──────────────────────────────────────
                if (unpaid > 0 && basic > 0)
                {
                    var dailyRate = basic / WorkingDaysPerMonth;
                    var absence = Round(dailyRate * unpaid);
                    otherDeductions += absence;
                    lines.Add(new PayrollLineDto
                    {
                        Code = "ABSENCE",
                        Name = $"Unpaid leave ({unpaid} d)",
                        Type = "Deduction",
                        Amount = absence,
                        IsTaxable = false
                    });
                }

                var deductions = Round(taxAndInsurance + otherDeductions);
                var net = Round(gross - deductions);

                // ── Employer contributions ────────────────────────────
                decimal employerContributions = 0m;
                foreach (var c in active.Where(c => c.Type == "EmployerContribution"))
                {
                    var configured = ValueFor(employee, c.Code, active, overrides, fallback: 0m);
                    if (configured == 0m) continue;

                    var value = c.CalculationType == "Percentage"
                        ? Round(configured / 100m * taxableBase)
                        : Round(configured);

                    employerContributions += value;
                    lines.Add(new PayrollLineDto
                    {
                        Code = c.Code,
                        Name = c.Name,
                        Type = "EmployerContribution",
                        Amount = value,
                        IsTaxable = false
                    });
                }

                var prevNet = previousNet.TryGetValue(employee.Id, out var pn) ? pn : 0m;
                var delta = Round(net - prevNet);

                Employees.Add(new PayrollEmployeeResultDto
                {
                    EmployeeId = employee.Id,
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeName = string.IsNullOrWhiteSpace(employee.Name) ? employee.EmployeeCode : employee.Name,
                    Department = employee.DepartmentName,
                    Basic = Round(basic),
                    Allowances = Round(lines.Where(l => l.Type == "Earning" && l.Code != "BASIC" && l.Code != "OVERTIME").Sum(l => l.Amount)),
                    OvertimeAmount = overtimeAmount,
                    GrossSalary = gross,
                    TaxAndInsurance = Round(taxAndInsurance),
                    Deductions = deductions,
                    NetSalary = net,
                    EmployerCost = Round(gross + employerContributions),
                    WorkingHours = wh,
                    OvertimeHours = oh,
                    UnpaidLeaveDays = unpaid,
                    PreviousNetSalary = prevNet,
                    NetDelta = delta,
                    NetDeltaPercentage = prevNet > 0 ? (int)Math.Round((double)(delta / prevNet) * 100) : 0,
                    Lines = lines
                });

                FlagAnomalies(employee, period, net, gross, deductions, oh, wh, prevNet, delta, duplicates);
            }
        }

        /// <summary>Employee override wins; otherwise the component default; otherwise the fallback.</summary>
        private static decimal ValueFor(
            Employee employee,
            string code,
            IReadOnlyList<SalaryComponent> components,
            IReadOnlyList<EmployeeSalaryComponent> overrides,
            decimal fallback)
        {
            var overrideValue = overrides.FirstOrDefault(o =>
                o.EmployeeId == employee.Id &&
                o.IsActive &&
                string.Equals(o.ComponentCode, code, StringComparison.OrdinalIgnoreCase));

            if (overrideValue != null) return overrideValue.Amount;

            var component = components.FirstOrDefault(c =>
                c.IsActive && string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase));

            if (component == null) return fallback;

            return component.CalculationType == "Percentage" ? component.Amount : component.Amount;
        }

        private void FlagAnomalies(
            Employee employee,
            string period,
            decimal net,
            decimal gross,
            decimal deductions,
            decimal overtimeHours,
            decimal workingHours,
            decimal previousNet,
            decimal delta,
            HashSet<string> duplicateNames)
        {
            void Add(string kind, string severity, string message, decimal value = 0m)
            {
                Anomalies.Add(new PayrollAnomalyDto
                {
                    Period = period,
                    EmployeeId = employee.Id,
                    EmployeeName = employee.Name,
                    Kind = kind,
                    Severity = severity,
                    Message = message,
                    Delta = value
                });
            }

            var display = string.IsNullOrWhiteSpace(employee.Name) ? employee.EmployeeCode : employee.Name;

            if (net < 0)
            {
                Add("NegativeNet", "Critical", $"{display} has a net salary of {net:0.##} — deductions exceed earnings.");
            }
            else if (net == 0)
            {
                Add("ZeroNet", "Warning", $"{display} has a net salary of 0 — no earnings were calculated for this period.");
            }

            if (previousNet > 0 && Math.Abs(delta) / previousNet > SalarySpikeThreshold)
            {
                var direction = delta > 0 ? "increased" : "dropped";
                Add("SalarySpike", "Critical",
                    $"Net salary {direction} by {Math.Abs(delta / previousNet):P0} versus last period ({previousNet:0.##} → {net:0.##}).",
                    delta);
            }

            if (overtimeHours > HighOvertimeHours)
            {
                Add("HighOvertime", "Warning", $"{overtimeHours:0.#} overtime hours this period — unusually high.", overtimeHours);
            }

            if (deductions > gross * UnexpectedDeductionRatio && gross > 0)
            {
                Add("UnexpectedDeduction", "Warning",
                    $"Deductions are {deductions / gross:P0} of gross — verify the components applied.", deductions);
            }

            if (workingHours == 0 && overtimeHours == 0)
            {
                Add("MissingAttendance", "Warning", $"No attendance records found for {employee.Name} in this period.");
            }

            if (!string.Equals(employee.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                Add("TerminatedButPaid", "Critical", $"{employee.Name} is marked '{employee.Status}' but is included in this run.");
            }

            if (duplicateNames.Contains((employee.Name ?? string.Empty).Trim().ToLowerInvariant()))
            {
                Add("DuplicateEmployee", "Warning", $"More than one employee is named '{employee.Name}'.");
            }

            if (employee.Salary > 0 && gross - employee.Salary > employee.Salary * HighBonusRatio)
            {
                Add("HighBonus", "Warning", $"Earnings are {((gross - employee.Salary) / employee.Salary):P0} above basic salary — one-off components may be misconfigured.");
            }
        }

        private static decimal Round(decimal value) => Math.Round(value, 2);
    }

    public class PayrollAppService : ApplicationService, IPayrollAppService
    {
        private readonly IRepository<PayrollRun, Guid> _payrollRepository;
        private readonly IRepository<Payslip, Guid> _payslipRepository;
        private readonly IRepository<PayslipLine, Guid> _payslipLineRepository;
        private readonly IRepository<PayrollAnomaly, Guid> _anomalyRepository;
        private readonly IRepository<SalaryComponent, Guid> _componentRepository;
        private readonly IRepository<EmployeeSalaryComponent, Guid> _overrideRepository;
        private readonly IRepository<Employee, Guid> _employeeRepository;
        private readonly IRepository<Attendance, Guid> _attendanceRepository;
        private readonly IRepository<LeaveRequest, Guid> _leaveRepository;

        public PayrollAppService(
            IRepository<PayrollRun, Guid> payrollRepository,
            IRepository<Payslip, Guid> payslipRepository,
            IRepository<PayslipLine, Guid> payslipLineRepository,
            IRepository<PayrollAnomaly, Guid> anomalyRepository,
            IRepository<SalaryComponent, Guid> componentRepository,
            IRepository<EmployeeSalaryComponent, Guid> overrideRepository,
            IRepository<Employee, Guid> employeeRepository,
            IRepository<Attendance, Guid> attendanceRepository,
            IRepository<LeaveRequest, Guid> leaveRepository)
        {
            _payrollRepository = payrollRepository;
            _payslipRepository = payslipRepository;
            _payslipLineRepository = payslipLineRepository;
            _anomalyRepository = anomalyRepository;
            _componentRepository = componentRepository;
            _overrideRepository = overrideRepository;
            _employeeRepository = employeeRepository;
            _attendanceRepository = attendanceRepository;
            _leaveRepository = leaveRepository;
        }

        public async Task<ListResultDto<PayrollRunDto>> GetPayrollRunsAsync()
        {
            var runs = await _payrollRepository.GetListAsync();
            var dtos = runs.OrderByDescending(r => r.Period).Select(Map).ToList();
            return new ListResultDto<PayrollRunDto>(dtos);
        }

        public async Task<ListResultDto<PayslipDto>> GetPayslipsAsync(string period)
        {
            var payslips = await _payslipRepository.GetListAsync();
            var filtered = payslips
                .Where(p => string.IsNullOrWhiteSpace(period) || p.Period == period)
                .OrderBy(p => p.EmployeeName)
                .ToList();

            var lines = await _payslipLineRepository.GetListAsync();
            var byPayslip = lines.GroupBy(l => l.PayslipId).ToDictionary(g => g.Key, g => g.ToList());

            var dtos = filtered.Select(p => new PayslipDto
            {
                Id = p.Id,
                EmployeeId = p.EmployeeId,
                EmployeeName = p.EmployeeName,
                Period = p.Period,
                BaseSalary = p.BaseSalary,
                Allowances = p.Allowances,
                OvertimeAmount = p.OvertimeAmount,
                GrossSalary = p.GrossSalary,
                TaxAmount = p.TaxAmount,
                Deductions = p.Deductions,
                NetSalary = p.NetSalary,
                EmployerCost = p.EmployerCost,
                Department = p.Department,
                Status = p.Status,
                PayrollRunId = p.PayrollRunId,
                Lines = byPayslip.TryGetValue(p.Id, out var l)
                    ? l.Select(x => new PayrollLineDto
                    {
                        Code = x.ComponentCode,
                        Name = x.ComponentName,
                        Type = x.Type,
                        Amount = x.Amount,
                        IsTaxable = x.IsTaxable
                    }).ToList()
                    : new List<PayrollLineDto>()
            }).ToList();

            return new ListResultDto<PayslipDto>(dtos);
        }

        /// <summary>
        /// Dry run. Computes every employee's pay and every anomaly for the period and
        /// writes nothing — this is the screen HR reviews before committing.
        /// </summary>
        public async Task<PayrollPreviewDto> GetPreviewAsync(string period)
        {
            var (start, end) = PeriodRange(period);
            var result = await CalculateAsync(period, start, end);

            var previous = await PreviousTotalsAsync(period);

            return new PayrollPreviewDto
            {
                Period = period,
                PeriodStart = start,
                PeriodEnd = end,
                Employees = result.Employees,
                Totals = Totals(result.Employees),
                PreviousTotals = previous,
                HasPreviousRun = previous != null,
                Anomalies = result.Anomalies,
                NetDelta = previous == null ? 0 : Math.Round(Totals(result.Employees).NetSalary - previous.NetSalary, 2),
                GrossDelta = previous == null ? 0 : Math.Round(Totals(result.Employees).GrossSalary - previous.GrossSalary, 2),
                HeadcountDelta = previous == null ? 0 : result.Employees.Count - previous.EmployeeCount,
                ComponentCount = result.ComponentCount,
                UsingFallback = result.ComponentCount == 0
            };
        }

        public async Task<PayrollRunDto> ProcessPayrollRunAsync(string period)
        {
            var (start, end) = PeriodRange(period);

            var existing = (await _payrollRepository.GetListAsync())
                .FirstOrDefault(r => r.Period == period);

            if (existing != null && PayrollStatus.IsLocked(existing.Status))
            {
                throw new BusinessException("ERPPlatform:PayrollRunLocked")
                    .WithData("Period", period);
            }

            // Re-running a period replaces its draft output rather than stacking on top of it.
            if (existing != null)
            {
                var oldPayslips = (await _payslipRepository.GetListAsync())
                    .Where(p => p.PayrollRunId == existing.Id).ToList();
                foreach (var p in oldPayslips)
                {
                    var oldLines = (await _payslipLineRepository.GetListAsync()).Where(l => l.PayslipId == p.Id).ToList();
                    foreach (var l in oldLines) await _payslipLineRepository.DeleteAsync(l.Id);
                    await _payslipRepository.DeleteAsync(p.Id);
                }

                var oldAnomalies = (await _anomalyRepository.GetListAsync()).Where(a => a.Period == period).ToList();
                foreach (var a in oldAnomalies) await _anomalyRepository.DeleteAsync(a.Id);

                await _payrollRepository.DeleteAsync(existing.Id);
            }

            var result = await CalculateAsync(period, start, end);
            var totals = Totals(result.Employees);

            var run = new PayrollRun
            {
                Period = period,
                TotalEmployees = totals.EmployeeCount,
                TotalGrossSalary = totals.GrossSalary,
                TotalAllowances = totals.Allowances,
                TotalOvertime = totals.Overtime,
                TotalTax = totals.TaxAndInsurance,
                TotalDeductions = totals.Deductions,
                TotalNetSalary = totals.NetSalary,
                TotalEmployerCost = totals.EmployerCost,
                Status = PayrollStatus.Calculated,
                ProcessedDate = Clock.Now
            };
            await _payrollRepository.InsertAsync(run, autoSave: true);

            foreach (var e in result.Employees)
            {
                var payslip = new Payslip
                {
                    EmployeeId = e.EmployeeId,
                    EmployeeName = e.EmployeeName,
                    Period = period,
                    BaseSalary = e.Basic,
                    Allowances = e.Allowances,
                    OvertimeAmount = e.OvertimeAmount,
                    GrossSalary = e.GrossSalary,
                    TaxAmount = e.TaxAndInsurance,
                    Deductions = e.Deductions,
                    NetSalary = e.NetSalary,
                    EmployerCost = e.EmployerCost,
                    Department = e.Department,
                    Status = "Pending",
                    PayrollRunId = run.Id
                };
                await _payslipRepository.InsertAsync(payslip, autoSave: true);

                foreach (var line in e.Lines)
                {
                    await _payslipLineRepository.InsertAsync(new PayslipLine
                    {
                        PayslipId = payslip.Id,
                        ComponentCode = line.Code,
                        ComponentName = line.Name,
                        Type = line.Type,
                        Amount = line.Amount,
                        IsTaxable = line.IsTaxable
                    }, autoSave: true);
                }
            }

            foreach (var a in result.Anomalies)
            {
                await _anomalyRepository.InsertAsync(new PayrollAnomaly
                {
                    Period = period,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.EmployeeName,
                    Kind = a.Kind,
                    Severity = a.Severity,
                    Message = a.Message,
                    Delta = a.Delta
                }, autoSave: true);
            }

            return Map(run);
        }

        public async Task<ListResultDto<PayrollAnomalyDto>> GetAnomaliesAsync(string period)
        {
            var all = await _anomalyRepository.GetListAsync();
            var items = all
                .Where(a => string.IsNullOrWhiteSpace(period) || a.Period == period)
                .OrderByDescending(a => a.Severity)
                .ThenBy(a => a.EmployeeName)
                .Select(a => new PayrollAnomalyDto
                {
                    Id = a.Id,
                    Period = a.Period,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.EmployeeName,
                    Kind = a.Kind,
                    Severity = a.Severity,
                    Message = a.Message,
                    Delta = a.Delta,
                    IsResolved = a.IsResolved,
                    ResolutionNote = a.ResolutionNote
                }).ToList();

            return new ListResultDto<PayrollAnomalyDto>(items);
        }

        /// <summary>Marks an anomaly as reviewed. Resolving does not change any salary figure.</summary>
        public async Task<PayrollAnomalyDto> ResolveAnomalyAsync(Guid id, ResolveAnomalyInput input)
        {
            var anomaly = await _anomalyRepository.GetAsync(id);
            anomaly.IsResolved = true;
            anomaly.ResolutionNote = input?.Note ?? string.Empty;
            await _anomalyRepository.UpdateAsync(anomaly, autoSave: true);

            return new PayrollAnomalyDto
            {
                Id = anomaly.Id,
                Period = anomaly.Period,
                EmployeeId = anomaly.EmployeeId,
                EmployeeName = anomaly.EmployeeName,
                Kind = anomaly.Kind,
                Severity = anomaly.Severity,
                Message = anomaly.Message,
                Delta = anomaly.Delta,
                IsResolved = anomaly.IsResolved,
                ResolutionNote = anomaly.ResolutionNote
            };
        }

        /// <summary>Draft/Calculated → HR Review → Finance Review → Finalized. Reopening is allowed until locked.</summary>
        public async Task<PayrollRunDto> AdvanceStatusAsync(Guid id)
        {
            var run = await _payrollRepository.GetAsync(id);

            if (PayrollStatus.IsLocked(run.Status))
            {
                throw new BusinessException("ERPPlatform:PayrollRunLocked").WithData("Period", run.Period);
            }

            run.Status = run.Status switch
            {
                PayrollStatus.Draft => PayrollStatus.HrReview,
                PayrollStatus.Calculated => PayrollStatus.HrReview,
                PayrollStatus.HrReview => PayrollStatus.FinanceReview,
                PayrollStatus.FinanceReview => PayrollStatus.Finalized,
                _ => PayrollStatus.HrReview
            };

            if (run.Status == PayrollStatus.Finalized)
            {
                run.LockedAt = Clock.Now;
                run.ApprovedBy = CurrentUser.UserName ?? CurrentUser.Name ?? "system";
            }

            await _payrollRepository.UpdateAsync(run, autoSave: true);
            return Map(run);
        }

        /// <summary>Reopens a run that has not been finalized yet.</summary>
        public async Task<PayrollRunDto> ReopenAsync(Guid id)
        {
            var run = await _payrollRepository.GetAsync(id);

            if (PayrollStatus.IsLocked(run.Status))
            {
                throw new BusinessException("ERPPlatform:PayrollRunLocked").WithData("Period", run.Period);
            }

            run.Status = PayrollStatus.Draft;
            run.LockedAt = null;
            await _payrollRepository.UpdateAsync(run, autoSave: true);
            return Map(run);
        }

        // ── internals ─────────────────────────────────────────────────

        private async Task<PayrollCalculationResult> CalculateAsync(string period, DateTime start, DateTime end)
        {
            var components = await _componentRepository.GetListAsync();
            var activeComponents = components
                .Where(c => c.IsActive && c.EffectiveFrom <= end && (c.EffectiveTo == null || c.EffectiveTo >= start))
                .ToList();

            var overrides = (await _overrideRepository.GetListAsync())
                .Where(o => o.IsActive && o.EffectiveFrom <= end && (o.EffectiveTo == null || o.EffectiveTo >= start))
                .ToList();

            var employees = (await _employeeRepository.GetListAsync())
                .Where(e => string.Equals(e.Status, "Active", StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.Name)
                .ToList();

            // Anyone who is not active but still punched in this period is included and flagged.
            var inactive = (await _employeeRepository.GetListAsync())
                .Where(e => !string.Equals(e.Status, "Active", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var attendances = (await _attendanceRepository.GetListAsync())
                .Where(a => a.Date >= start && a.Date <= end)
                .ToList();

            var inactiveWithAttendance = inactive
                .Where(e => attendances.Any(a => a.EmployeeId == e.Id))
                .ToList();

            var inScope = employees.Concat(inactiveWithAttendance).OrderBy(e => e.Name).ToList();
            var ids = inScope.Select(e => e.Id).ToHashSet();

            var workingHours = attendances
                .GroupBy(a => a.EmployeeId)
                .Where(g => ids.Contains(g.Key))
                .ToDictionary(g => g.Key, g => g.Sum(a => a.WorkingHours));

            var overtimeHours = attendances
                .GroupBy(a => a.EmployeeId)
                .Where(g => ids.Contains(g.Key))
                .ToDictionary(g => g.Key, g => g.Sum(a => a.OvertimeHours));

            var unpaidLeave = (await _leaveRepository.GetListAsync())
                .Where(l => l.Status == "Approved"
                            && string.Equals(l.LeaveType, "Unpaid", StringComparison.OrdinalIgnoreCase)
                            && l.StartDate <= end && l.EndDate >= start)
                .GroupBy(l => l.EmployeeId)
                .Where(g => ids.Contains(g.Key))
                .ToDictionary(g => g.Key, g => g.Sum(l => l.DaysCount));

            var previousPeriod = PreviousPeriod(period);
            var previousNet = (await _payslipRepository.GetListAsync())
                .Where(p => p.Period == previousPeriod)
                .GroupBy(p => p.EmployeeId)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.NetSalary));

            var calculator = new PayrollCalculator();
            calculator.Calculate(inScope, activeComponents, overrides, workingHours, overtimeHours, unpaidLeave, previousNet, period);

            return new PayrollCalculationResult
            {
                Employees = calculator.Employees,
                Anomalies = calculator.Anomalies,
                ComponentCount = activeComponents.Count
            };
        }

        private async Task<PayrollTotalsDto?> PreviousTotalsAsync(string period)
        {
            var previousPeriod = PreviousPeriod(period);
            var payslips = (await _payslipRepository.GetListAsync())
                .Where(p => p.Period == previousPeriod)
                .ToList();

            if (payslips.Count == 0) return null;

            return new PayrollTotalsDto
            {
                EmployeeCount = payslips.Count,
                GrossSalary = payslips.Sum(p => p.GrossSalary),
                Allowances = payslips.Sum(p => p.Allowances),
                Overtime = payslips.Sum(p => p.OvertimeAmount),
                TaxAndInsurance = payslips.Sum(p => p.TaxAmount),
                Deductions = payslips.Sum(p => p.Deductions),
                NetSalary = payslips.Sum(p => p.NetSalary),
                EmployerCost = payslips.Sum(p => p.EmployerCost)
            };
        }

        private static PayrollTotalsDto Totals(List<PayrollEmployeeResultDto> employees) => new()
        {
            EmployeeCount = employees.Count,
            GrossSalary = Math.Round(employees.Sum(e => e.GrossSalary), 2),
            Allowances = Math.Round(employees.Sum(e => e.Allowances), 2),
            Overtime = Math.Round(employees.Sum(e => e.OvertimeAmount), 2),
            TaxAndInsurance = Math.Round(employees.Sum(e => e.TaxAndInsurance), 2),
            Deductions = Math.Round(employees.Sum(e => e.Deductions), 2),
            NetSalary = Math.Round(employees.Sum(e => e.NetSalary), 2),
            EmployerCost = Math.Round(employees.Sum(e => e.EmployerCost), 2)
        };

        public static (DateTime Start, DateTime End) PeriodRange(string period)
        {
            if (DateTime.TryParseExact(period, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                var start = new DateTime(parsed.Year, parsed.Month, 1);
                var end = start.AddMonths(1).AddDays(-1);
                return (start, end);
            }

            var now = DateTime.UtcNow;
            var fallbackStart = new DateTime(now.Year, now.Month, 1);
            return (fallbackStart, fallbackStart.AddMonths(1).AddDays(-1));
        }

        public static string PreviousPeriod(string period)
        {
            if (DateTime.TryParseExact(period, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                return parsed.AddMonths(-1).ToString("yyyy-MM", CultureInfo.InvariantCulture);
            }
            return DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM", CultureInfo.InvariantCulture);
        }

        private static PayrollRunDto Map(PayrollRun r) => new()
        {
            Id = r.Id,
            Period = r.Period,
            TotalEmployees = r.TotalEmployees,
            TotalGrossSalary = r.TotalGrossSalary,
            TotalAllowances = r.TotalAllowances,
            TotalOvertime = r.TotalOvertime,
            TotalTax = r.TotalTax,
            TotalDeductions = r.TotalDeductions,
            TotalNetSalary = r.TotalNetSalary,
            TotalEmployerCost = r.TotalEmployerCost,
            Status = r.Status,
            ProcessedDate = r.ProcessedDate,
            LockedAt = r.LockedAt,
            ApprovedBy = r.ApprovedBy,
            Notes = r.Notes
        };
    }

    // ────────────────────────────────────────────────────────────────
    // Salary components — the configuration surface of the engine
    // ────────────────────────────────────────────────────────────────

    public class SalaryComponentDto : EntityDto<Guid>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Earning";
        public string CalculationType { get; set; } = "Fixed";
        public decimal Amount { get; set; }
        public string PercentageOfComponentCode { get; set; } = "BASIC";
        public string Formula { get; set; } = string.Empty;
        public bool IsTaxable { get; set; } = true;
        public bool IsRecurring { get; set; } = true;
        public bool CountsAsEmployerCost { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string GlAccount { get; set; } = string.Empty;
        public string CostCenter { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    /// <summary>GET/POST/PUT/DELETE /api/app/salary-component</summary>
    public class SalaryComponentAppService
        : CrudAppService<SalaryComponent, SalaryComponentDto, Guid, PagedAndSortedResultRequestDto, SalaryComponentDto>
    {
        public SalaryComponentAppService(IRepository<SalaryComponent, Guid> repository) : base(repository) { }

        public override async Task<SalaryComponentDto> CreateAsync(SalaryComponentDto input)
        {
            input.Code = NormaliseCode(input.Code);
            return await base.CreateAsync(input);
        }

        public override async Task<SalaryComponentDto> UpdateAsync(Guid id, SalaryComponentDto input)
        {
            input.Code = NormaliseCode(input.Code);
            return await base.UpdateAsync(id, input);
        }

        private static string NormaliseCode(string code) =>
            (code ?? string.Empty).Trim().ToUpperInvariant().Replace(" ", "_");
    }

    public class EmployeeSalaryComponentDto : EntityDto<Guid>
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public Guid ComponentId { get; set; }
        public string ComponentCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    /// <summary>GET/POST/PUT/DELETE /api/app/employee-salary-component</summary>
    public class EmployeeSalaryComponentAppService
        : CrudAppService<EmployeeSalaryComponent, EmployeeSalaryComponentDto, Guid, PagedAndSortedResultRequestDto, EmployeeSalaryComponentDto>
    {
        public EmployeeSalaryComponentAppService(IRepository<EmployeeSalaryComponent, Guid> repository) : base(repository) { }
    }
}
