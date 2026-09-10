using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Crm
{
    /// <summary>
    /// AutoMapper maps are discovered automatically because the application module calls
    /// AbpAutoMapperOptions.AddMaps&lt;ERPPlatformApplicationModule&gt;().
    /// </summary>
    public class CrmAutoMapperProfile : Profile
    {
        public CrmAutoMapperProfile()
        {
            // CreationTime/CreatorId are owned by ABP's audit interceptor — never accept
            // them back from the client, or every update stamps 0001-01-01.
            CreateMap<CrmContact, CrmContactDto>()
                .ReverseMap()
                .ForMember(x => x.CreationTime, o => o.Ignore());

            CreateMap<CrmActivity, CrmActivityDto>()
                .ReverseMap()
                .ForMember(x => x.CreationTime, o => o.Ignore());

            CreateMap<CrmNote, CrmNoteDto>()
                .ReverseMap()
                .ForMember(x => x.CreationTime, o => o.Ignore())
                .ForMember(x => x.CreatorId, o => o.Ignore());
        }
    }

    // ────────────────────────────────────────────────────────────────
    // Contacts
    // ────────────────────────────────────────────────────────────────

    public class CrmContactDto : EntityDto<Guid>
    {
        public Guid? CustomerId { get; set; }
        public Guid? LeadId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public string Notes { get; set; } = string.Empty;
        /// <summary>Denormalised account name so lists can render without a second call.</summary>
        public string CustomerName { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
    }

    public class CrmContactListInput : PagedAndSortedResultRequestDto
    {
        public Guid? CustomerId { get; set; }
        public Guid? LeadId { get; set; }
        /// <summary>Free-text match on name, email, phone or job title.</summary>
        public string? Filter { get; set; }
    }

    /// <summary>GET/POST/PUT/DELETE /api/app/crm-contact</summary>
    public class CrmContactAppService
        : CrudAppService<CrmContact, CrmContactDto, Guid, CrmContactListInput, CrmContactDto>
    {
        private readonly IRepository<Customer, Guid> _customerRepository;

        public CrmContactAppService(
            IRepository<CrmContact, Guid> repository,
            IRepository<Customer, Guid> customerRepository) : base(repository)
        {
            _customerRepository = customerRepository;
        }

        protected override async Task<IQueryable<CrmContact>> CreateFilteredQueryAsync(CrmContactListInput input)
        {
            var query = await base.CreateFilteredQueryAsync(input);

            if (input.CustomerId.HasValue)
            {
                query = query.Where(x => x.CustomerId == input.CustomerId.Value);
            }

            if (input.LeadId.HasValue)
            {
                query = query.Where(x => x.LeadId == input.LeadId.Value);
            }

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                var f = input.Filter.Trim().ToLowerInvariant();
                query = query.Where(x =>
                    x.FirstName.ToLower().Contains(f) ||
                    x.LastName.ToLower().Contains(f) ||
                    x.FullName.ToLower().Contains(f) ||
                    x.Email.ToLower().Contains(f) ||
                    x.Phone.ToLower().Contains(f) ||
                    x.JobTitle.ToLower().Contains(f));
            }

            return query;
        }

        public override async Task<CrmContactDto> CreateAsync(CrmContactDto input)
        {
            Normalise(input);
            var entity = ObjectMapper.Map<CrmContactDto, CrmContact>(input);
            await Repository.InsertAsync(entity, autoSave: true);
            await EnsureSinglePrimaryAsync(entity);
            return await MapAndDecorate(entity);
        }

        public override async Task<CrmContactDto> UpdateAsync(Guid id, CrmContactDto input)
        {
            var entity = await Repository.GetAsync(id);
            ObjectMapper.Map(input, entity);
            Normalise(entity);
            await Repository.UpdateAsync(entity, autoSave: true);
            await EnsureSinglePrimaryAsync(entity);
            return await MapAndDecorate(entity);
        }

        public override async Task<PagedResultDto<CrmContactDto>> GetListAsync(CrmContactListInput input)
        {
            var result = await base.GetListAsync(input);
            if (result.Items.Count == 0)
            {
                return result;
            }

            var ids = result.Items
                .Where(x => x.CustomerId.HasValue)
                .Select(x => x.CustomerId!.Value)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                return result;
            }

            var names = (await _customerRepository.GetListAsync(c => ids.Contains(c.Id)))
                .ToDictionary(c => c.Id, c => c.Name);

            foreach (var item in result.Items)
            {
                if (item.CustomerId.HasValue && names.TryGetValue(item.CustomerId.Value, out var name))
                {
                    item.CustomerName = name;
                }
            }

            return result;
        }

        /// <summary>POST /api/app/crm-contact/{id}/make-primary</summary>
        public async Task<CrmContactDto> MakePrimaryAsync(Guid id)
        {
            var contact = await Repository.GetAsync(id);
            contact.IsPrimary = true;
            await Repository.UpdateAsync(contact, autoSave: true);
            await EnsureSinglePrimaryAsync(contact);
            return await MapAndDecorate(contact);
        }

        private static void Normalise(CrmContactDto dto)
        {
            dto.FullName = (dto.FirstName + " " + dto.LastName).Trim();
        }

        private static void Normalise(CrmContact entity)
        {
            entity.FullName = (entity.FirstName + " " + entity.LastName).Trim();
        }

        /// <summary>Only one contact per account may be flagged as primary.</summary>
        private async Task EnsureSinglePrimaryAsync(CrmContact contact)
        {
            if (!contact.IsPrimary)
            {
                return;
            }

            var siblings = await Repository.GetListAsync(x =>
                x.Id != contact.Id &&
                x.IsPrimary &&
                x.CustomerId == contact.CustomerId &&
                x.LeadId == contact.LeadId);

            foreach (var sibling in siblings)
            {
                sibling.IsPrimary = false;
                await Repository.UpdateAsync(sibling, autoSave: true);
            }
        }

        private async Task<CrmContactDto> MapAndDecorate(CrmContact entity)
        {
            var dto = ObjectMapper.Map<CrmContact, CrmContactDto>(entity);
            dto.CreationTime = entity.CreationTime;
            if (entity.CustomerId.HasValue)
            {
                var customer = await _customerRepository.FindAsync(entity.CustomerId.Value);
                dto.CustomerName = customer?.Name ?? string.Empty;
            }

            return dto;
        }
    }

    // ────────────────────────────────────────────────────────────────
    // Activities (calls, meetings, tasks, follow-ups)
    // ────────────────────────────────────────────────────────────────

    public class CrmActivityDto : EntityDto<Guid>
    {
        /// <summary>Call | Meeting | Task | Email | FollowUp</summary>
        public string Type { get; set; } = "Task";
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        /// <summary>Open | Completed | Cancelled</summary>
        public string Status { get; set; } = "Open";
        public DateTime? CompletedAt { get; set; }
        /// <summary>Low | Normal | High</summary>
        public string Priority { get; set; } = "Normal";

        public Guid? LeadId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ContactId { get; set; }
        public Guid? DealId { get; set; }

        public string AssignedTo { get; set; } = string.Empty;
        public Guid? AssignedToUserId { get; set; }
        public string Outcome { get; set; } = string.Empty;

        public DateTime CreationTime { get; set; }
        /// <summary>True when the activity is still open and its due date has passed.</summary>
        public bool IsOverdue { get; set; }

        // Denormalised labels for the timeline.
        public string CustomerName { get; set; } = string.Empty;
        public string LeadName { get; set; } = string.Empty;
        public string DealTitle { get; set; } = string.Empty;
    }

    public class CrmActivityListInput : PagedAndSortedResultRequestDto
    {
        public Guid? LeadId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ContactId { get; set; }
        public Guid? DealId { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public bool? OnlyOverdue { get; set; }
        public bool? OnlyOpen { get; set; }
    }

    public class CompleteActivityInput
    {
        /// <summary>Free-text outcome, e.g. "Left voicemail, call back Thursday".</summary>
        public string Outcome { get; set; } = string.Empty;
    }

    /// <summary>GET/POST/PUT/DELETE /api/app/crm-activity</summary>
    public class CrmActivityAppService
        : CrudAppService<CrmActivity, CrmActivityDto, Guid, CrmActivityListInput, CrmActivityDto>
    {
        private readonly IRepository<Customer, Guid> _customerRepository;
        private readonly IRepository<Lead, Guid> _leadRepository;
        private readonly IRepository<Deal, Guid> _dealRepository;

        public CrmActivityAppService(
            IRepository<CrmActivity, Guid> repository,
            IRepository<Customer, Guid> customerRepository,
            IRepository<Lead, Guid> leadRepository,
            IRepository<Deal, Guid> dealRepository) : base(repository)
        {
            _customerRepository = customerRepository;
            _leadRepository = leadRepository;
            _dealRepository = dealRepository;
        }

        protected override async Task<IQueryable<CrmActivity>> CreateFilteredQueryAsync(CrmActivityListInput input)
        {
            var query = await base.CreateFilteredQueryAsync(input);

            if (input.LeadId.HasValue) query = query.Where(x => x.LeadId == input.LeadId.Value);
            if (input.CustomerId.HasValue) query = query.Where(x => x.CustomerId == input.CustomerId.Value);
            if (input.ContactId.HasValue) query = query.Where(x => x.ContactId == input.ContactId.Value);
            if (input.DealId.HasValue) query = query.Where(x => x.DealId == input.DealId.Value);
            if (input.AssignedToUserId.HasValue) query = query.Where(x => x.AssignedToUserId == input.AssignedToUserId.Value);

            if (!string.IsNullOrWhiteSpace(input.Type)) query = query.Where(x => x.Type == input.Type);
            if (!string.IsNullOrWhiteSpace(input.Status)) query = query.Where(x => x.Status == input.Status);
            if (input.OnlyOpen == true) query = query.Where(x => x.Status == "Open");
            if (input.OnlyOverdue == true)
            {
                var now = Clock.Now;
                query = query.Where(x => x.Status == "Open" && x.DueDate < now);
            }

            return query;
        }

        public override async Task<CrmActivityDto> CreateAsync(CrmActivityDto input)
        {
            var entity = ObjectMapper.Map<CrmActivityDto, CrmActivity>(input);
            await Repository.InsertAsync(entity, autoSave: true);
            return await MapAndDecorate(entity);
        }

        public override async Task<CrmActivityDto> UpdateAsync(Guid id, CrmActivityDto input)
        {
            var entity = await Repository.GetAsync(id);
            ObjectMapper.Map(input, entity);
            await Repository.UpdateAsync(entity, autoSave: true);
            return await MapAndDecorate(entity);
        }

        public override async Task<PagedResultDto<CrmActivityDto>> GetListAsync(CrmActivityListInput input)
        {
            var result = await base.GetListAsync(input);
            if (result.Items.Count == 0)
            {
                return result;
            }

            var customerIds = Ids(result.Items.Select(x => x.CustomerId));
            var leadIds = Ids(result.Items.Select(x => x.LeadId));
            var dealIds = Ids(result.Items.Select(x => x.DealId));

            var customers = customerIds.Count == 0
                ? new Dictionary<Guid, string>()
                : (await _customerRepository.GetListAsync(c => customerIds.Contains(c.Id)))
                    .ToDictionary(c => c.Id, c => c.Name);

            var leads = leadIds.Count == 0
                ? new Dictionary<Guid, string>()
                : (await _leadRepository.GetListAsync(l => leadIds.Contains(l.Id)))
                    .ToDictionary(l => l.Id, l => l.Name);

            var deals = dealIds.Count == 0
                ? new Dictionary<Guid, string>()
                : (await _dealRepository.GetListAsync(d => dealIds.Contains(d.Id)))
                    .ToDictionary(d => d.Id, d => d.Title);

            foreach (var item in result.Items)
            {
                item.IsOverdue = item.Status == "Open" && item.DueDate < Clock.Now;
                if (item.CustomerId.HasValue) item.CustomerName = customers.GetValueOrDefault(item.CustomerId.Value) ?? string.Empty;
                if (item.LeadId.HasValue) item.LeadName = leads.GetValueOrDefault(item.LeadId.Value) ?? string.Empty;
                if (item.DealId.HasValue) item.DealTitle = deals.GetValueOrDefault(item.DealId.Value) ?? string.Empty;
            }

            return result;
        }

        /// <summary>POST /api/app/crm-activity/{id}/complete</summary>
        public async Task<CrmActivityDto> CompleteAsync(Guid id, CompleteActivityInput input)
        {
            var entity = await Repository.GetAsync(id);
            entity.Status = "Completed";
            entity.CompletedAt = Clock.Now;
            entity.Outcome = input?.Outcome ?? string.Empty;
            await Repository.UpdateAsync(entity, autoSave: true);
            return await MapAndDecorate(entity);
        }

        /// <summary>POST /api/app/crm-activity/{id}/reopen</summary>
        public async Task<CrmActivityDto> ReopenAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            entity.Status = "Open";
            entity.CompletedAt = null;
            await Repository.UpdateAsync(entity, autoSave: true);
            return await MapAndDecorate(entity);
        }

        private static List<Guid> Ids(IEnumerable<Guid?> ids)
        {
            return ids.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        }

        private async Task<CrmActivityDto> MapAndDecorate(CrmActivity entity)
        {
            var dto = ObjectMapper.Map<CrmActivity, CrmActivityDto>(entity);
            dto.CreationTime = entity.CreationTime;
            dto.IsOverdue = dto.Status == "Open" && dto.DueDate < Clock.Now;

            if (entity.CustomerId.HasValue)
            {
                var c = await _customerRepository.FindAsync(entity.CustomerId.Value);
                dto.CustomerName = c?.Name ?? string.Empty;
            }

            if (entity.LeadId.HasValue)
            {
                var l = await _leadRepository.FindAsync(entity.LeadId.Value);
                dto.LeadName = l?.Name ?? string.Empty;
            }

            if (entity.DealId.HasValue)
            {
                var d = await _dealRepository.FindAsync(entity.DealId.Value);
                dto.DealTitle = d?.Title ?? string.Empty;
            }

            return dto;
        }
    }

    // ────────────────────────────────────────────────────────────────
    // Notes & attachments
    // ────────────────────────────────────────────────────────────────

    public class CrmNoteDto : EntityDto<Guid>
    {
        public Guid? LeadId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ContactId { get; set; }
        public Guid? DealId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsPinned { get; set; }
        public string AttachmentName { get; set; } = string.Empty;
        public string AttachmentUrl { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
        public Guid? CreatorId { get; set; }
    }

    public class CrmNoteListInput : PagedAndSortedResultRequestDto
    {
        public Guid? LeadId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ContactId { get; set; }
        public Guid? DealId { get; set; }
    }

    /// <summary>GET/POST/PUT/DELETE /api/app/crm-note</summary>
    public class CrmNoteAppService
        : CrudAppService<CrmNote, CrmNoteDto, Guid, CrmNoteListInput, CrmNoteDto>
    {
        public CrmNoteAppService(IRepository<CrmNote, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<CrmNote>> CreateFilteredQueryAsync(CrmNoteListInput input)
        {
            var query = await base.CreateFilteredQueryAsync(input);

            if (input.LeadId.HasValue) query = query.Where(x => x.LeadId == input.LeadId.Value);
            if (input.CustomerId.HasValue) query = query.Where(x => x.CustomerId == input.CustomerId.Value);
            if (input.ContactId.HasValue) query = query.Where(x => x.ContactId == input.ContactId.Value);
            if (input.DealId.HasValue) query = query.Where(x => x.DealId == input.DealId.Value);

            return query.OrderByDescending(x => x.IsPinned).ThenByDescending(x => x.CreationTime);
        }

        public override async Task<CrmNoteDto> CreateAsync(CrmNoteDto input)
        {
            var entity = ObjectMapper.Map<CrmNoteDto, CrmNote>(input);
            await Repository.InsertAsync(entity, autoSave: true);
            return Map(entity);
        }

        public override async Task<CrmNoteDto> UpdateAsync(Guid id, CrmNoteDto input)
        {
            var entity = await Repository.GetAsync(id);
            ObjectMapper.Map(input, entity);
            await Repository.UpdateAsync(entity, autoSave: true);
            return Map(entity);
        }

        public override async Task<CrmNoteDto> GetAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            return Map(entity);
        }

        public override async Task<PagedResultDto<CrmNoteDto>> GetListAsync(CrmNoteListInput input)
        {
            var result = await base.GetListAsync(input);
            foreach (var dto in result.Items)
            {
                var entity = await Repository.FindAsync(dto.Id);
                if (entity != null)
                {
                    dto.CreationTime = entity.CreationTime;
                    dto.CreatorId = entity.CreatorId;
                }
            }

            return result;
        }

        private static CrmNoteDto Map(CrmNote entity)
        {
            return new CrmNoteDto
            {
                Id = entity.Id,
                LeadId = entity.LeadId,
                CustomerId = entity.CustomerId,
                ContactId = entity.ContactId,
                DealId = entity.DealId,
                Content = entity.Content,
                IsPinned = entity.IsPinned,
                AttachmentName = entity.AttachmentName,
                AttachmentUrl = entity.AttachmentUrl,
                CreationTime = entity.CreationTime,
                CreatorId = entity.CreatorId
            };
        }
    }
}
