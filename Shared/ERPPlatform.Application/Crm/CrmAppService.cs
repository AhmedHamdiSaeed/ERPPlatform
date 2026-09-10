using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Crm
{
    /// <summary>
    /// Canonical pipeline stages. The kanban board, the pipeline summary and the
    /// stage-transition rules all read from here so they can never drift apart.
    /// </summary>
    public static class DealStages
    {
        public const string New = "New";
        public const string Qualified = "Qualified";
        public const string Proposal = "Proposal";
        public const string Negotiation = "Negotiation";
        public const string ClosedWon = "Closed Won";
        public const string ClosedLost = "Closed Lost";

        /// <summary>Legacy stage kept so existing rows still render.</summary>
        public const string Prospecting = "Prospecting";

        public static readonly string[] All =
        {
            New, Qualified, Proposal, Negotiation, ClosedWon, ClosedLost, Prospecting
        };

        public static readonly string[] Open = { New, Qualified, Proposal, Negotiation, Prospecting };

        /// <summary>Default win probability per stage (percent).</summary>
        public static int ProbabilityFor(string stage) => stage switch
        {
            New => 10,
            Prospecting => 20,
            Qualified => 40,
            Proposal => 60,
            Negotiation => 80,
            ClosedWon => 100,
            ClosedLost => 0,
            _ => 20
        };

        public static bool IsClosed(string stage) =>
            stage == ClosedWon || stage == ClosedLost;
    }

    public class DealDto : EntityDto<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Stage { get; set; } = "Prospecting";
        public int Probability { get; set; } = 20;
        public DateTime ExpectedCloseDate { get; set; }
        public string OwnerName { get; set; } = string.Empty;

        // ── Opportunity fields (Phase 2) ───────────────────────────────
        public Guid? CustomerId { get; set; }
        public Guid? ContactId { get; set; }
        public Guid? LeadId { get; set; }
        public Guid? OwnerUserId { get; set; }
        public string Competitor { get; set; } = string.Empty;
        public string LostReason { get; set; } = string.Empty;
        public DateTime? ClosedAt { get; set; }
        public string Tags { get; set; } = string.Empty;

        public DateTime CreationTime { get; set; }
    }

    public class CreateUpdateDealDto
    {
        public string Title { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Stage { get; set; } = "Prospecting";
        public int Probability { get; set; } = 20;

        public Guid? CustomerId { get; set; }
        public Guid? ContactId { get; set; }
        public Guid? LeadId { get; set; }
        public Guid? OwnerUserId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string Competitor { get; set; } = string.Empty;
        public string LostReason { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public DateTime? ExpectedCloseDate { get; set; }
    }

    public class DealLostInput
    {
        public string LostReason { get; set; } = string.Empty;
        public string Competitor { get; set; } = string.Empty;
    }

    public class PipelineSummaryDto
    {
        public decimal TotalPipelineValue { get; set; }
        public decimal WeightedForecast { get; set; }
        public int TotalDealsCount { get; set; }
        public int WinRatePercentage { get; set; }
        public int OpenDealsCount { get; set; }
        public int WonDealsCount { get; set; }
        public int LostDealsCount { get; set; }
        public decimal WonValue { get; set; }
        public List<PipelineStageDto> Stages { get; set; } = new();
    }

    public class PipelineStageDto
    {
        public string Stage { get; set; } = string.Empty;
        public int DealCount { get; set; }
        public decimal Value { get; set; }
    }

    public interface ICrmAppService : IApplicationService
    {
        Task<ListResultDto<DealDto>> GetDealsAsync();
        Task<DealDto> CreateDealAsync(CreateUpdateDealDto input);
        Task UpdateDealStageAsync(Guid id, string newStage);
        Task<PipelineSummaryDto> GetPipelineSummaryAsync();
    }

    public class CrmAppService : ApplicationService, ICrmAppService
    {
        private readonly IRepository<Deal, Guid> _dealRepository;
        private readonly IRepository<Customer, Guid> _customerRepository;

        public CrmAppService(
            IRepository<Deal, Guid> dealRepository,
            IRepository<Customer, Guid> customerRepository)
        {
            _dealRepository = dealRepository;
            _customerRepository = customerRepository;
        }

        public async Task<ListResultDto<DealDto>> GetDealsAsync()
        {
            var deals = await _dealRepository.GetListAsync();
            return new ListResultDto<DealDto>(deals.Select(ToDto).ToList());
        }

        public async Task<DealDto> CreateDealAsync(CreateUpdateDealDto input)
        {
            var deal = new Deal
            {
                Title = input.Title,
                CustomerName = input.CustomerName,
                Value = input.Value,
                Stage = string.IsNullOrWhiteSpace(input.Stage) ? DealStages.New : input.Stage,
                Probability = input.Probability > 0 ? input.Probability : DealStages.ProbabilityFor(input.Stage),
                ExpectedCloseDate = input.ExpectedCloseDate ?? Clock.Now.AddDays(30),
                OwnerName = string.IsNullOrWhiteSpace(input.OwnerName) ? "Account Executive" : input.OwnerName,
                CustomerId = input.CustomerId,
                ContactId = input.ContactId,
                LeadId = input.LeadId,
                OwnerUserId = input.OwnerUserId,
                Competitor = input.Competitor ?? string.Empty,
                LostReason = input.LostReason ?? string.Empty,
                Tags = input.Tags ?? string.Empty
            };

            await ResolveCustomerAsync(deal);
            await _dealRepository.InsertAsync(deal, autoSave: true);

            return ToDto(deal);
        }

        /// <summary>PUT /api/app/crm/{id}/deal — full edit of an opportunity.</summary>
        public async Task<DealDto> UpdateDealAsync(Guid id, CreateUpdateDealDto input)
        {
            var deal = await _dealRepository.GetAsync(id);

            deal.Title = input.Title;
            deal.CustomerName = input.CustomerName;
            deal.Value = input.Value;
            deal.Stage = input.Stage;
            deal.Probability = input.Probability > 0 ? input.Probability : DealStages.ProbabilityFor(input.Stage);
            deal.ExpectedCloseDate = input.ExpectedCloseDate ?? deal.ExpectedCloseDate;
            deal.OwnerName = input.OwnerName;
            deal.OwnerUserId = input.OwnerUserId;
            deal.CustomerId = input.CustomerId;
            deal.ContactId = input.ContactId;
            deal.LeadId = input.LeadId;
            deal.Competitor = input.Competitor ?? string.Empty;
            deal.LostReason = input.LostReason ?? string.Empty;
            deal.Tags = input.Tags ?? string.Empty;

            await ResolveCustomerAsync(deal);
            await _dealRepository.UpdateAsync(deal, autoSave: true);

            return ToDto(deal);
        }

        public async Task UpdateDealStageAsync(Guid id, string newStage)
        {
            var deal = await _dealRepository.GetAsync(id);
            await ApplyStageAsync(deal, newStage);
            await _dealRepository.UpdateAsync(deal, autoSave: true);
        }

        /// <summary>POST /api/app/crm/{id}/mark-won</summary>
        public async Task<DealDto> MarkWonAsync(Guid id)
        {
            var deal = await _dealRepository.GetAsync(id);
            await ApplyStageAsync(deal, DealStages.ClosedWon);
            await _dealRepository.UpdateAsync(deal, autoSave: true);
            return ToDto(deal);
        }

        /// <summary>POST /api/app/crm/{id}/mark-lost</summary>
        public async Task<DealDto> MarkLostAsync(Guid id, DealLostInput input)
        {
            var deal = await _dealRepository.GetAsync(id);
            deal.LostReason = input?.LostReason ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(input?.Competitor))
            {
                deal.Competitor = input.Competitor;
            }

            await ApplyStageAsync(deal, DealStages.ClosedLost);
            await _dealRepository.UpdateAsync(deal, autoSave: true);
            return ToDto(deal);
        }

        /// <summary>DELETE /api/app/crm/{id}/deal</summary>
        public async Task DeleteDealAsync(Guid id)
        {
            await _dealRepository.DeleteAsync(id);
        }

        public async Task<PipelineSummaryDto> GetPipelineSummaryAsync()
        {
            var deals = await _dealRepository.GetListAsync();

            var won = deals.Where(d => d.Stage == DealStages.ClosedWon).ToList();
            var lost = deals.Where(d => d.Stage == DealStages.ClosedLost).ToList();
            var open = deals.Where(d => !DealStages.IsClosed(d.Stage)).ToList();

            var decided = won.Count + lost.Count;
            var winRate = decided > 0 ? (int)Math.Round((double)won.Count / decided * 100) : 0;

            var stages = DealStages.All
                .Select(s => new PipelineStageDto
                {
                    Stage = s,
                    DealCount = deals.Count(d => d.Stage == s),
                    Value = deals.Where(d => d.Stage == s).Sum(d => d.Value)
                })
                .Where(s => s.DealCount > 0)
                .ToList();

            return new PipelineSummaryDto
            {
                TotalPipelineValue = open.Sum(d => d.Value),
                WeightedForecast = open.Sum(d => d.Value * (d.Probability / 100m)),
                TotalDealsCount = deals.Count,
                OpenDealsCount = open.Count,
                WonDealsCount = won.Count,
                LostDealsCount = lost.Count,
                WonValue = won.Sum(d => d.Value),
                WinRatePercentage = winRate,
                Stages = stages
            };
        }

        private Task ApplyStageAsync(Deal deal, string newStage)
        {
            if (string.IsNullOrWhiteSpace(newStage))
            {
                throw new UserFriendlyException("A stage is required.");
            }

            deal.Stage = newStage;
            deal.Probability = DealStages.ProbabilityFor(newStage);

            if (DealStages.IsClosed(newStage))
            {
                deal.ClosedAt = Clock.Now;
            }
            else
            {
                deal.ClosedAt = null;
            }

            return Task.CompletedTask;
        }

        /// <summary>Back-fills CustomerId from CustomerName when only the name is known.</summary>
        private async Task ResolveCustomerAsync(Deal deal)
        {
            if (deal.CustomerId.HasValue)
            {
                var customer = await _customerRepository.FindAsync(deal.CustomerId.Value);
                if (customer != null)
                {
                    deal.CustomerName = customer.Name;
                }

                return;
            }

            if (string.IsNullOrWhiteSpace(deal.CustomerName))
            {
                return;
            }

            var match = await _customerRepository.FindAsync(c => c.Name == deal.CustomerName);
            deal.CustomerId = match?.Id;
        }

        private static DealDto ToDto(Deal d) => new DealDto
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
            OwnerUserId = d.OwnerUserId,
            Competitor = d.Competitor,
            LostReason = d.LostReason,
            ClosedAt = d.ClosedAt,
            Tags = d.Tags,
            CreationTime = d.CreationTime
        };
    }
}
