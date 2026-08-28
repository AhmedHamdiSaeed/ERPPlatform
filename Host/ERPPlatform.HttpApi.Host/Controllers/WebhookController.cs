using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using ERPPlatform.Domain.Events;
using Volo.Abp.EventBus.Local;

namespace ERPPlatform.HttpApi.Host.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    public class WebhookController : AbpControllerBase
    {
        private readonly ILocalEventBus _eventBus;

        public WebhookController(ILocalEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        [HttpPost("stripe")]
        public async Task<IActionResult> HandleStripeWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            var jsonPayload = await reader.ReadToEndAsync();

            // Process webhook payload (simulated Stripe PaymentIntent Succeeded)
            await _eventBus.PublishAsync(new PaymentReceivedEvent
            {
                PaymentId = Guid.NewGuid(),
                ReferenceNumber = $"STRIPE-{DateTime.UtcNow.Ticks.ToString()[^6..]}",
                CustomerName = "External Stripe Customer",
                Amount = 150.00m,
                PaymentMethod = "Stripe Gateway"
            });

            return Ok(new { received = true, timestamp = DateTime.UtcNow });
        }

        [HttpPost("sms-status")]
        public IActionResult HandleSmsCallback([FromBody] System.Text.Json.JsonElement body)
        {
            return Ok(new { status = "Acknowledged", processedAt = DateTime.UtcNow });
        }
    }
}
