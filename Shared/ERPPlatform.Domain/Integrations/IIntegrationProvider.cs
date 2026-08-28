using System;
using System.Threading.Tasks;

namespace ERPPlatform.Domain.Integrations
{
    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string CustomerEmail { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string InvoiceId { get; set; } = string.Empty;
    }

    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string ErrorMessage { get; set; } = string.Empty;
        public string CheckoutUrl { get; set; } = string.Empty;
    }

    public interface IPaymentGateway
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
        Task<PaymentResult> VerifyPaymentAsync(string transactionId);
    }

    public interface ISmsProvider
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);
    }

    public interface IWhatsAppProvider
    {
        Task<bool> SendMessageAsync(string recipientPhone, string message);
        Task<bool> SendTemplateAsync(string recipientPhone, string templateName, object parameters);
    }
}
