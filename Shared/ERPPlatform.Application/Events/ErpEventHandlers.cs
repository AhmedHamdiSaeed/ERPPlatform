using System;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using ERPPlatform.Domain.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;

namespace ERPPlatform.Application.Events
{
    public class SalesInvoiceEventHandler : ILocalEventHandler<SalesInvoiceCreatedEvent>, ITransientDependency
    {
        private readonly IRepository<JournalEntry, Guid> _journalRepository;

        public SalesInvoiceEventHandler(IRepository<JournalEntry, Guid> journalRepository)
        {
            _journalRepository = journalRepository;
        }

        public async Task HandleEventAsync(SalesInvoiceCreatedEvent eventData)
        {
            // Auto-generate Double-Entry Accounting Journal Entry:
            // Debit: Accounts Receivable (AR)
            // Credit: Sales Revenue
            var journalEntry = new JournalEntry
            {
                EntryNumber = $"JE-INV-{eventData.InvoiceNumber}",
                EntryDate = DateTime.UtcNow,
                Description = $"Automated GL Posting for Sales Invoice #{eventData.InvoiceNumber} - {eventData.CustomerName}",
                TotalDebit = eventData.TotalAmount,
                TotalCredit = eventData.TotalAmount,
                Status = "Posted",
                CreatedBy = "System Automator"
            };

            await _journalRepository.InsertAsync(journalEntry);
        }
    }

    public class PaymentReceivedEventHandler : ILocalEventHandler<PaymentReceivedEvent>, ITransientDependency
    {
        private readonly IRepository<JournalEntry, Guid> _journalRepository;

        public PaymentReceivedEventHandler(IRepository<JournalEntry, Guid> journalRepository)
        {
            _journalRepository = journalRepository;
        }

        public async Task HandleEventAsync(PaymentReceivedEvent eventData)
        {
            // Auto-generate Double-Entry Accounting Journal Entry:
            // Debit: Cash/Bank Account
            // Credit: Accounts Receivable (AR)
            var journalEntry = new JournalEntry
            {
                EntryNumber = $"JE-PAY-{eventData.ReferenceNumber}",
                EntryDate = DateTime.UtcNow,
                Description = $"Automated Payment Clearance for {eventData.CustomerName} via {eventData.PaymentMethod}",
                TotalDebit = eventData.Amount,
                TotalCredit = eventData.Amount,
                Status = "Posted",
                CreatedBy = "System Automator"
            };

            await _journalRepository.InsertAsync(journalEntry);
        }
    }
}
