using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Crm
{
    // ────────────────────────────────────────────────────────────────
    // CRM dashboard KPIs
    // ────────────────────────────────────────────────────────────────

    public class LeadSourceStatDto
    {
        public string Source { get; set; } = string.Empty;
        public int Count { get; set; }
        public int ConvertedCount { get; set; }
    }

    public class ActivityByOwnerDto
    {
        public string Owner { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Open { get; set; }
        public int Overdue { get; set; }
    }

    public class CrmDashboardDto
    {
        // Leads
        public int TotalLeads { get; set; }
        public int NewLeads { get; set; }
        public int ContactedLeads { get; set; }
        public int QualifiedLeads { get; set; }
        public int ConvertedLeads { get; set; }
        public int LostLeads { get; set; }

        // Opportunities
        public int OpenOpportunities { get; set; }
        public int WonDeals { get; set; }
        public int LostDeals { get; set; }
        public decimal PipelineValue { get; set; }
        public decimal WeightedForecast { get; set; }
        public decimal WonValue { get; set; }

        // Ratios
        /// <summary>Leads that reached Converted, as a percentage of all leads.</summary>
        public int LeadConversionRatePercentage { get; set; }
        /// <summary>Won / (Won + Lost) opportunities.</summary>
        public int WinRatePercentage { get; set; }
        public decimal AverageDealValue { get; set; }
        /// <summary>Mean days from opportunity creation to close, across decided deals.</summary>
        public double AverageSalesCycleDays { get; set; }

        // Activities
        public int OpenActivities { get; set; }
        public int OverdueActivities { get; set; }
        public int CallsThisMonth { get; set; }
        public int MeetingsThisMonth { get; set; }
        public int TasksThisMonth { get; set; }

        public List<PipelineStageDto> PipelineByStage { get; set; } = new();
        public List<LeadSourceStatDto> LeadsBySource { get; set; } = new();
        public List<ActivityByOwnerDto> ActivitiesByOwner { get; set; } = new();
        public List<CrmActivityDto> UpcomingActivities { get; set; } = new();
    }

    /// <summary>GET /api/app/crm-dashboard/kpis</summary>
    public class CrmDashboardAppService : ApplicationService
    {
        private readonly IRepository<Lead, Guid> _leadRepository;
        private readonly IRepository<Deal, Guid> _dealRepository;
        private readonly IRepository<CrmActivity, Guid> _activityRepository;

        public CrmDashboardAppService(
            IRepository<Lead, Guid> leadRepository,
            IRepository<Deal, Guid> dealRepository,
            IRepository<CrmActivity, Guid> activityRepository)
        {
            _leadRepository = leadRepository;
            _dealRepository = dealRepository;
            _activityRepository = activityRepository;
        }

        public async Task<CrmDashboardDto> GetKpisAsync()
        {
            var now = Clock.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var leads = await _leadRepository.GetListAsync();
            var deals = await _dealRepository.GetListAsync();
            var activities = await _activityRepository.GetListAsync();

            var open = deals.Where(d => !DealStages.IsClosed(d.Stage)).ToList();
            var won = deals.Where(d => d.Stage == DealStages.ClosedWon).ToList();
            var lost = deals.Where(d => d.Stage == DealStages.ClosedLost).ToList();
            var decided = won.Count + lost.Count;

            var openActivities = activities.Where(a => a.Status == "Open").ToList();
            var overdue = openActivities.Where(a => a.DueDate < now).ToList();

            var cycleDays = won
                .Where(d => d.ClosedAt.HasValue)
                .Select(d => (d.ClosedAt!.Value - d.CreationTime).TotalDays)
                .ToList();

            var byOwner = openActivities
                .GroupBy(a => string.IsNullOrWhiteSpace(a.AssignedTo) ? "Unassigned" : a.AssignedTo)
                .Select(g => new ActivityByOwnerDto
                {
                    Owner = g.Key,
                    Total = g.Count(),
                    Open = g.Count(),
                    Overdue = g.Count(a => a.DueDate < now)
                })
                .OrderByDescending(x => x.Overdue)
                .ThenByDescending(x => x.Open)
                .ToList();

            return new CrmDashboardDto
            {
                TotalLeads = leads.Count,
                NewLeads = leads.Count(l => l.Status == "New"),
                ContactedLeads = leads.Count(l => l.Status == "Contacted"),
                QualifiedLeads = leads.Count(l => l.Status == "Qualified"),
                ConvertedLeads = leads.Count(l => l.Status == "Converted"),
                LostLeads = leads.Count(l => l.Status == "Lost"),

                OpenOpportunities = open.Count,
                WonDeals = won.Count,
                LostDeals = lost.Count,
                PipelineValue = open.Sum(d => d.Value),
                WeightedForecast = open.Sum(d => d.Value * (d.Probability / 100m)),
                WonValue = won.Sum(d => d.Value),

                LeadConversionRatePercentage = leads.Count > 0
                    ? (int)Math.Round((double)leads.Count(l => l.Status == "Converted") / leads.Count * 100)
                    : 0,
                WinRatePercentage = decided > 0 ? (int)Math.Round((double)won.Count / decided * 100) : 0,
                AverageDealValue = deals.Count > 0 ? Math.Round(deals.Average(d => d.Value), 2) : 0,
                AverageSalesCycleDays = cycleDays.Count > 0 ? Math.Round(cycleDays.Average(), 1) : 0,

                OpenActivities = openActivities.Count,
                OverdueActivities = overdue.Count,
                CallsThisMonth = activities.Count(a => a.Type == "Call" && a.CreationTime >= monthStart),
                MeetingsThisMonth = activities.Count(a => a.Type == "Meeting" && a.CreationTime >= monthStart),
                TasksThisMonth = activities.Count(a => a.Type == "Task" && a.CreationTime >= monthStart),

                PipelineByStage = DealStages.All
                    .Select(s => new PipelineStageDto
                    {
                        Stage = s,
                        DealCount = deals.Count(d => d.Stage == s),
                        Value = deals.Where(d => d.Stage == s).Sum(d => d.Value)
                    })
                    .ToList(),

                LeadsBySource = leads
                    .GroupBy(l => string.IsNullOrWhiteSpace(l.Source) ? "Unknown" : l.Source)
                    .Select(g => new LeadSourceStatDto
                    {
                        Source = g.Key,
                        Count = g.Count(),
                        ConvertedCount = g.Count(l => l.Status == "Converted")
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList(),

                ActivitiesByOwner = byOwner,

                UpcomingActivities = openActivities
                    .OrderBy(a => a.DueDate)
                    .Take(10)
                    .Select(a => new CrmActivityDto
                    {
                        Id = a.Id,
                        Type = a.Type,
                        Subject = a.Subject,
                        Description = a.Description,
                        DueDate = a.DueDate,
                        Status = a.Status,
                        Priority = a.Priority,
                        LeadId = a.LeadId,
                        CustomerId = a.CustomerId,
                        ContactId = a.ContactId,
                        DealId = a.DealId,
                        AssignedTo = a.AssignedTo,
                        Outcome = a.Outcome,
                        CreationTime = a.CreationTime,
                        IsOverdue = a.DueDate < now
                    })
                    .ToList()
            };
        }
    }

    // ────────────────────────────────────────────────────────────────
    // Customer 360
    // ────────────────────────────────────────────────────────────────

    public class TimelineItemDto
    {
        public DateTime At { get; set; }
        /// <summary>Activity | Note | Opportunity | Quotation | Order | Invoice | Payment</summary>
        public string Kind { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public Guid? ReferenceId { get; set; }
    }

    public class Customer360SummaryDto
    {
        public Guid Id { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public bool IsVip { get; set; }
        public bool IsActive { get; set; }
        public string Tags { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public decimal OutstandingBalance { get; set; }
        public string PaymentTerms { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
    }

    public class Customer360Dto
    {
        public Customer360SummaryDto Customer { get; set; } = new();

        public List<CrmContactDto> Contacts { get; set; } = new();
        public List<DealDto> Opportunities { get; set; } = new();
        public List<CrmActivityDto> Activities { get; set; } = new();
        public List<CrmNoteDto> Notes { get; set; } = new();
        public List<TimelineItemDto> Timeline { get; set; } = new();

        public List<SalesQuotationBriefDto> Quotations { get; set; } = new();
        public List<SalesOrderBriefDto> Orders { get; set; } = new();
        public List<SalesInvoiceBriefDto> Invoices { get; set; } = new();
        public List<PaymentBriefDto> Payments { get; set; } = new();

        // Rolled-up numbers for the header cards.
        public int OpenOpportunityCount { get; set; }
        public decimal PipelineValue { get; set; }
        public int OrderCount { get; set; }
        public decimal OrderValue { get; set; }
        public int InvoiceCount { get; set; }
        public decimal InvoicedValue { get; set; }
        public decimal OutstandingValue { get; set; }
        public decimal PaidValue { get; set; }
        public int OpenActivityCount { get; set; }
        public int OverdueActivityCount { get; set; }
    }

    public class SalesQuotationBriefDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SalesOrderBriefDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SalesInvoiceBriefDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class PaymentBriefDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>GET /api/app/customer-360/{id}</summary>
    public class Customer360AppService : ApplicationService
    {
        private readonly IRepository<Customer, Guid> _customerRepository;
        private readonly IRepository<CrmContact, Guid> _contactRepository;
        private readonly IRepository<Deal, Guid> _dealRepository;
        private readonly IRepository<CrmActivity, Guid> _activityRepository;
        private readonly IRepository<CrmNote, Guid> _noteRepository;
        private readonly IRepository<SalesQuotation, Guid> _quotationRepository;
        private readonly IRepository<SalesOrder, Guid> _orderRepository;
        private readonly IRepository<SalesInvoice, Guid> _invoiceRepository;
        private readonly IRepository<Payment, Guid> _paymentRepository;

        public Customer360AppService(
            IRepository<Customer, Guid> customerRepository,
            IRepository<CrmContact, Guid> contactRepository,
            IRepository<Deal, Guid> dealRepository,
            IRepository<CrmActivity, Guid> activityRepository,
            IRepository<CrmNote, Guid> noteRepository,
            IRepository<SalesQuotation, Guid> quotationRepository,
            IRepository<SalesOrder, Guid> orderRepository,
            IRepository<SalesInvoice, Guid> invoiceRepository,
            IRepository<Payment, Guid> paymentRepository)
        {
            _customerRepository = customerRepository;
            _contactRepository = contactRepository;
            _dealRepository = dealRepository;
            _activityRepository = activityRepository;
            _noteRepository = noteRepository;
            _quotationRepository = quotationRepository;
            _orderRepository = orderRepository;
            _invoiceRepository = invoiceRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<Customer360Dto> GetAsync(Guid id)
        {
            var customer = await _customerRepository.GetAsync(id);
            var now = Clock.Now;

            // Legacy rows were linked by name only, so match on either.
            var deals = await _dealRepository.GetListAsync(d =>
                d.CustomerId == id || d.CustomerName == customer.Name);

            var contacts = await _contactRepository.GetListAsync(c => c.CustomerId == id);
            var activities = await _activityRepository.GetListAsync(a => a.CustomerId == id);
            var notes = await _noteRepository.GetListAsync(n => n.CustomerId == id);
            var quotations = await _quotationRepository.GetListAsync(q => q.CustomerName == customer.Name);
            var orders = await _orderRepository.GetListAsync(o => o.CustomerName == customer.Name);
            var invoices = await _invoiceRepository.GetListAsync(i => i.CustomerName == customer.Name);
            var payments = await _paymentRepository.GetListAsync(p => p.CustomerName == customer.Name);

            var openDeals = deals.Where(d => !DealStages.IsClosed(d.Stage)).ToList();

            var dto = new Customer360Dto
            {
                Customer = new Customer360SummaryDto
                {
                    Id = customer.Id,
                    CustomerCode = customer.CustomerCode,
                    Name = customer.Name,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Address = customer.Address,
                    ContactPerson = customer.ContactPerson,
                    TaxNumber = customer.TaxNumber,
                    Industry = customer.Industry,
                    Website = customer.Website,
                    OwnerName = customer.OwnerName,
                    IsVip = customer.IsVip,
                    IsActive = customer.IsActive,
                    Tags = customer.Tags,
                    CreditLimit = customer.CreditLimit,
                    OutstandingBalance = customer.OutstandingBalance,
                    PaymentTerms = customer.PaymentTerms,
                    Currency = customer.Currency
                },

                Contacts = contacts
                    .OrderByDescending(c => c.IsPrimary)
                    .ThenBy(c => c.FullName)
                    .Select(c => new CrmContactDto
                    {
                        Id = c.Id,
                        CustomerId = c.CustomerId,
                        LeadId = c.LeadId,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        FullName = c.FullName,
                        JobTitle = c.JobTitle,
                        Email = c.Email,
                        Phone = c.Phone,
                        Mobile = c.Mobile,
                        IsPrimary = c.IsPrimary,
                        Notes = c.Notes,
                        CustomerName = customer.Name,
                        CreationTime = c.CreationTime
                    })
                    .ToList(),

                Opportunities = deals
                    .Select(d => new DealDto
                    {
                        Id = d.Id,
                        Title = d.Title,
                        CustomerName = d.CustomerName,
                        Value = d.Value,
                        Stage = d.Stage,
                        Probability = d.Probability,
                        ExpectedCloseDate = d.ExpectedCloseDate,
                        OwnerName = d.OwnerName,
                        CustomerId = d.CustomerId,
                        ContactId = d.ContactId,
                        LeadId = d.LeadId,
                        Competitor = d.Competitor,
                        LostReason = d.LostReason,
                        ClosedAt = d.ClosedAt,
                        Tags = d.Tags,
                        CreationTime = d.CreationTime
                    })
                    .ToList(),

                Activities = activities
                    .OrderBy(a => a.Status == "Open" ? 0 : 1)
                    .ThenBy(a => a.DueDate)
                    .Select(a => new CrmActivityDto
                    {
                        Id = a.Id,
                        Type = a.Type,
                        Subject = a.Subject,
                        Description = a.Description,
                        DueDate = a.DueDate,
                        Status = a.Status,
                        CompletedAt = a.CompletedAt,
                        Priority = a.Priority,
                        LeadId = a.LeadId,
                        CustomerId = a.CustomerId,
                        ContactId = a.ContactId,
                        DealId = a.DealId,
                        AssignedTo = a.AssignedTo,
                        Outcome = a.Outcome,
                        CreationTime = a.CreationTime,
                        IsOverdue = a.Status == "Open" && a.DueDate < now
                    })
                    .ToList(),

                Notes = notes
                    .OrderByDescending(n => n.IsPinned)
                    .ThenByDescending(n => n.CreationTime)
                    .Select(n => new CrmNoteDto
                    {
                        Id = n.Id,
                        LeadId = n.LeadId,
                        CustomerId = n.CustomerId,
                        ContactId = n.ContactId,
                        DealId = n.DealId,
                        Content = n.Content,
                        IsPinned = n.IsPinned,
                        AttachmentName = n.AttachmentName,
                        AttachmentUrl = n.AttachmentUrl,
                        CreationTime = n.CreationTime
                    })
                    .ToList(),

                Quotations = quotations
                    .OrderByDescending(q => q.IssueDate)
                    .Select(q => new SalesQuotationBriefDto
                    {
                        Id = q.Id,
                        Number = q.QuotationNumber,
                        Date = q.IssueDate,
                        TotalAmount = q.TotalAmount,
                        Status = q.Status
                    })
                    .ToList(),

                Orders = orders
                    .OrderByDescending(o => o.OrderDate)
                    .Select(o => new SalesOrderBriefDto
                    {
                        Id = o.Id,
                        Number = o.OrderNumber,
                        Date = o.OrderDate,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status
                    })
                    .ToList(),

                Invoices = invoices
                    .OrderByDescending(i => i.IssueDate)
                    .Select(i => new SalesInvoiceBriefDto
                    {
                        Id = i.Id,
                        Number = i.InvoiceNumber,
                        Date = i.IssueDate,
                        DueDate = i.DueDate,
                        TotalAmount = i.TotalAmount,
                        Status = i.Status
                    })
                    .ToList(),

                Payments = payments
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => new PaymentBriefDto
                    {
                        Id = p.Id,
                        Number = p.PaymentNumber,
                        Date = p.PaymentDate,
                        Amount = p.Amount,
                        Method = p.PaymentMethod,
                        Status = p.Status
                    })
                    .ToList(),

                OpenOpportunityCount = openDeals.Count,
                PipelineValue = openDeals.Sum(d => d.Value),
                OrderCount = orders.Count,
                OrderValue = orders.Sum(o => o.TotalAmount),
                InvoiceCount = invoices.Count,
                InvoicedValue = invoices.Sum(i => i.TotalAmount),
                OutstandingValue = invoices
                    .Where(i => i.Status != "Paid" && i.Status != "Cancelled")
                    .Sum(i => i.TotalAmount),
                PaidValue = payments
                    .Where(p => p.Status == "Completed")
                    .Sum(p => p.Amount),
                OpenActivityCount = activities.Count(a => a.Status == "Open"),
                OverdueActivityCount = activities.Count(a => a.Status == "Open" && a.DueDate < now)
            };

            dto.Timeline = BuildTimeline(dto);
            return dto;
        }

        private static List<TimelineItemDto> BuildTimeline(Customer360Dto dto)
        {
            var items = new List<TimelineItemDto>();

            items.AddRange(dto.Opportunities.Select(d => new TimelineItemDto
            {
                At = d.CreationTime,
                Kind = "Opportunity",
                Title = d.Title,
                Detail = d.Stage,
                Status = d.Stage,
                Amount = d.Value,
                ReferenceId = d.Id
            }));

            items.AddRange(dto.Activities.Select(a => new TimelineItemDto
            {
                At = a.CompletedAt ?? a.DueDate,
                Kind = "Activity",
                Title = a.Subject,
                Detail = a.Type,
                Status = a.Status,
                ReferenceId = a.Id
            }));

            items.AddRange(dto.Notes.Select(n => new TimelineItemDto
            {
                At = n.CreationTime,
                Kind = "Note",
                Title = n.Content,
                Detail = "Note",
                ReferenceId = n.Id
            }));

            items.AddRange(dto.Quotations.Select(q => new TimelineItemDto
            {
                At = q.Date,
                Kind = "Quotation",
                Title = q.Number,
                Detail = q.Status,
                Status = q.Status,
                Amount = q.TotalAmount,
                ReferenceId = q.Id
            }));

            items.AddRange(dto.Orders.Select(o => new TimelineItemDto
            {
                At = o.Date,
                Kind = "Order",
                Title = o.Number,
                Detail = o.Status,
                Status = o.Status,
                Amount = o.TotalAmount,
                ReferenceId = o.Id
            }));

            items.AddRange(dto.Invoices.Select(i => new TimelineItemDto
            {
                At = i.Date,
                Kind = "Invoice",
                Title = i.Number,
                Detail = i.Status,
                Status = i.Status,
                Amount = i.TotalAmount,
                ReferenceId = i.Id
            }));

            items.AddRange(dto.Payments.Select(p => new TimelineItemDto
            {
                At = p.Date,
                Kind = "Payment",
                Title = p.Number,
                Detail = p.Method,
                Status = p.Status,
                Amount = p.Amount,
                ReferenceId = p.Id
            }));

            return items.OrderByDescending(x => x.At).ToList();
        }
    }
}
