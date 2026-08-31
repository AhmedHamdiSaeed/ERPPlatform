using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ERPPlatform.Payments
{
    public class PaymentDto : EntityDto<Guid>
    {
        public string PaymentNumber { get; set; } = string.Empty;
        public Guid? SalesInvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string PaymentMethod { get; set; } = "Card";
        public string Provider { get; set; } = "Stripe";
        public string ExternalPaymentId { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime PaymentDate { get; set; }
        public decimal? RefundedAmount { get; set; }
        public DateTime? RefundedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class ProcessPaymentDto
    {
        public Guid SalesInvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string PaymentMethod { get; set; } = "Card";
        public string PaymentMethodId { get; set; } = string.Empty; // Stripe PM ID
        public string Provider { get; set; } = "Stripe";
        public string Notes { get; set; } = string.Empty;
    }

    public class RefundDto
    {
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public interface IPaymentAppService : IApplicationService
    {
        Task<ListResultDto<PaymentDto>> GetPaymentsAsync();
        Task<PaymentDto> GetAsync(Guid id);
        Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto input);
        Task<PaymentDto> RefundAsync(RefundDto input);
        Task<PaymentDto> CaptureAsync(Guid paymentId);
    }

    public class PaymentAppService : ApplicationService, IPaymentAppService
    {
        private readonly IRepository<Payment, Guid> _paymentRepository;
        private readonly IRepository<SalesInvoice, Guid> _invoiceRepository;

        public PaymentAppService(
            IRepository<Payment, Guid> paymentRepository,
            IRepository<SalesInvoice, Guid> invoiceRepository)
        {
            _paymentRepository = paymentRepository;
            _invoiceRepository = invoiceRepository;
        }

        public async Task<ListResultDto<PaymentDto>> GetPaymentsAsync()
        {
            var payments = await _paymentRepository.GetListAsync();
            var dtos = payments.OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentDto
                {
                    Id = p.Id,
                    PaymentNumber = p.PaymentNumber,
                    SalesInvoiceId = p.SalesInvoiceId,
                    InvoiceNumber = p.InvoiceNumber,
                    CustomerName = p.CustomerName,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    PaymentMethod = p.PaymentMethod,
                    Provider = p.Provider,
                    ExternalPaymentId = p.ExternalPaymentId,
                    Status = p.Status,
                    PaymentDate = p.PaymentDate,
                    RefundedAmount = p.RefundedAmount,
                    RefundedAt = p.RefundedAt,
                    Notes = p.Notes
                }).ToList();
            return new ListResultDto<PaymentDto>(dtos);
        }

        public async Task<PaymentDto> GetAsync(Guid id)
        {
            var p = await _paymentRepository.GetAsync(id);
            return new PaymentDto
            {
                Id = p.Id,
                PaymentNumber = p.PaymentNumber,
                SalesInvoiceId = p.SalesInvoiceId,
                InvoiceNumber = p.InvoiceNumber,
                CustomerName = p.CustomerName,
                Amount = p.Amount,
                Currency = p.Currency,
                PaymentMethod = p.PaymentMethod,
                Provider = p.Provider,
                ExternalPaymentId = p.ExternalPaymentId,
                Status = p.Status,
                PaymentDate = p.PaymentDate,
                RefundedAmount = p.RefundedAmount,
                RefundedAt = p.RefundedAt,
                Notes = p.Notes
            };
        }

        public async Task<PaymentDto> ProcessPaymentAsync(ProcessPaymentDto input)
        {
            var invoice = await _invoiceRepository.GetAsync(input.SalesInvoiceId);

            // In a real implementation, this would call Stripe API to create a PaymentIntent/Charge
            // For now, we simulate the payment processing
            var externalId = $"pi_{Guid.NewGuid():N}"[..24];

            var payment = new Payment
            {
                PaymentNumber = $"PAY-{DateTime.UtcNow.Year}-{new Random().Next(10000, 99999)}",
                SalesInvoiceId = input.SalesInvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerName = invoice.CustomerName,
                Amount = input.Amount,
                Currency = input.Currency,
                PaymentMethod = input.PaymentMethod,
                Provider = input.Provider,
                ExternalPaymentId = externalId,
                Status = "Completed",
                PaymentDate = DateTime.UtcNow,
                Notes = input.Notes
            };

            await _paymentRepository.InsertAsync(payment);

            // Mark the invoice as paid
            invoice.Status = "Paid";
            await _invoiceRepository.UpdateAsync(invoice);

            return new PaymentDto
            {
                Id = payment.Id,
                PaymentNumber = payment.PaymentNumber,
                SalesInvoiceId = payment.SalesInvoiceId,
                InvoiceNumber = payment.InvoiceNumber,
                CustomerName = payment.CustomerName,
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaymentMethod = payment.PaymentMethod,
                Provider = payment.Provider,
                ExternalPaymentId = payment.ExternalPaymentId,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                RefundedAmount = payment.RefundedAmount,
                RefundedAt = payment.RefundedAt,
                Notes = payment.Notes
            };
        }

        public async Task<PaymentDto> RefundAsync(RefundDto input)
        {
            var payment = await _paymentRepository.GetAsync(input.PaymentId);

            // In a real implementation, this would call Stripe API to create a refund
            payment.RefundedAmount = input.Amount;
            payment.RefundedAt = DateTime.UtcNow;
            payment.Status = input.Amount >= payment.Amount ? "Refunded" : "PartiallyRefunded";
            payment.Notes = $"Refund: {input.Reason}";
            await _paymentRepository.UpdateAsync(payment);

            return new PaymentDto
            {
                Id = payment.Id,
                PaymentNumber = payment.PaymentNumber,
                SalesInvoiceId = payment.SalesInvoiceId,
                InvoiceNumber = payment.InvoiceNumber,
                CustomerName = payment.CustomerName,
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaymentMethod = payment.PaymentMethod,
                Provider = payment.Provider,
                ExternalPaymentId = payment.ExternalPaymentId,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                RefundedAmount = payment.RefundedAmount,
                RefundedAt = payment.RefundedAt,
                Notes = payment.Notes
            };
        }

        public async Task<PaymentDto> CaptureAsync(Guid paymentId)
        {
            var payment = await _paymentRepository.GetAsync(paymentId);

            // In a real implementation, this would call Stripe API to capture a previously authorized payment
            payment.Status = "Completed";
            await _paymentRepository.UpdateAsync(payment);

            return new PaymentDto
            {
                Id = payment.Id,
                PaymentNumber = payment.PaymentNumber,
                SalesInvoiceId = payment.SalesInvoiceId,
                InvoiceNumber = payment.InvoiceNumber,
                CustomerName = payment.CustomerName,
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaymentMethod = payment.PaymentMethod,
                Provider = payment.Provider,
                ExternalPaymentId = payment.ExternalPaymentId,
                Status = payment.Status,
                PaymentDate = payment.PaymentDate,
                RefundedAmount = payment.RefundedAmount,
                RefundedAt = payment.RefundedAt,
                Notes = payment.Notes
            };
        }
    }
}
