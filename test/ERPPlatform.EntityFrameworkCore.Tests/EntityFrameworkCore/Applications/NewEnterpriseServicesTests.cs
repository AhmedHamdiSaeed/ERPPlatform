using System;
using System.Linq;
using System.Threading.Tasks;
using ERPPlatform.Finance;
using ERPPlatform.Payroll;
using ERPPlatform.Crm;
using ERPPlatform.Audit;
using Shouldly;
using Xunit;

namespace ERPPlatform.EntityFrameworkCore.Applications
{
    public class FinanceAppServiceTests : ERPPlatformEntityFrameworkCoreTestBase
    {
        private readonly IFinanceAppService _financeService;

        public FinanceAppServiceTests()
        {
            _financeService = GetRequiredService<IFinanceAppService>();
        }

        [Fact]
        public async Task Should_Create_Account_And_Get_ChartOfAccounts()
        {
            var created = await _financeService.CreateAccountAsync(new CreateUpdateAccountDto
            {
                Code = "1010-TEST",
                Name = "Operating Cash Account",
                Type = "Asset",
                Balance = 250_000m,
                Currency = "USD"
            });

            created.ShouldNotBeNull();
            created.Code.ShouldBe("1010-TEST");
            created.Balance.ShouldBe(250_000m);

            var accounts = await _financeService.GetAccountsAsync();
            accounts.Items.ShouldContain(a => a.Code == "1010-TEST");
        }

        [Fact]
        public async Task CreateJournalEntry_Should_Enforce_DoubleEntry_Balance()
        {
            // Valid entry: Debit equals Credit
            var valid = await _financeService.CreateJournalEntryAsync(new CreateJournalEntryDto
            {
                Description = "Equipment purchase",
                TotalDebit = 5000m,
                TotalCredit = 5000m
            });

            valid.ShouldNotBeNull();
            valid.Status.ShouldBe("Posted");

            // Invalid entry: Debit does NOT equal Credit
            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await _financeService.CreateJournalEntryAsync(new CreateJournalEntryDto
                {
                    Description = "Unbalanced entry",
                    TotalDebit = 5000m,
                    TotalCredit = 3000m
                });
            });
        }

        [Fact]
        public async Task GetProfitAndLossSummary_Should_Calculate_NetProfit_And_Margin()
        {
            await _financeService.CreateAccountAsync(new CreateUpdateAccountDto
            {
                Code = "4000-TEST",
                Name = "Sales Revenue",
                Type = "Revenue",
                Balance = 100_000m
            });

            await _financeService.CreateAccountAsync(new CreateUpdateAccountDto
            {
                Code = "5000-TEST",
                Name = "Operating Expense",
                Type = "Expense",
                Balance = 60_000m
            });

            var summary = await _financeService.GetProfitAndLossSummaryAsync();
            summary.ShouldNotBeNull();
            summary.TotalRevenue.ShouldBeGreaterThan(0);
            summary.NetProfit.ShouldBe(summary.TotalRevenue - summary.TotalExpenses);
        }
    }

    public class PayrollAppServiceTests : ERPPlatformEntityFrameworkCoreTestBase
    {
        private readonly IPayrollAppService _payrollService;

        public PayrollAppServiceTests()
        {
            _payrollService = GetRequiredService<IPayrollAppService>();
        }

        [Fact]
        public async Task ProcessPayrollRun_Should_Calculate_Net_Salary_And_Deductions()
        {
            var run = await _payrollService.ProcessPayrollRunAsync("September 2026");

            run.ShouldNotBeNull();
            run.Period.ShouldBe("September 2026");
            run.TotalGrossSalary.ShouldBeGreaterThan(0);
            run.TotalDeductions.ShouldBeGreaterThan(0);
            run.TotalNetSalary.ShouldBe(run.TotalGrossSalary - run.TotalDeductions);
            run.Status.ShouldBe("Approved");
        }
    }

    public class CrmAppServiceTests : ERPPlatformEntityFrameworkCoreTestBase
    {
        private readonly ICrmAppService _crmService;

        public CrmAppServiceTests()
        {
            _crmService = GetRequiredService<ICrmAppService>();
        }

        [Fact]
        public async Task CreateDeal_And_Update_Stage_Should_Update_Probability()
        {
            var deal = await _crmService.CreateDealAsync(new CreateUpdateDealDto
            {
                Title = "Enterprise Cloud Migration",
                CustomerName = "Acme Global",
                Value = 150_000m,
                Stage = "Proposal",
                Probability = 50,
                OwnerName = "John Sales"
            });

            deal.ShouldNotBeNull();
            deal.Title.ShouldBe("Enterprise Cloud Migration");

            // Update to Closed Won -> Probability should be set to 100
            await _crmService.UpdateDealStageAsync(deal.Id, "Closed Won");

            var deals = await _crmService.GetDealsAsync();
            var updated = deals.Items.FirstOrDefault(d => d.Id == deal.Id);
            updated.ShouldNotBeNull();
            updated.Stage.ShouldBe("Closed Won");
            updated.Probability.ShouldBe(100);
        }
    }

    public class ChatAppServiceTests : ERPPlatformEntityFrameworkCoreTestBase
    {
        private readonly ERPPlatform.Chat.IChatAppService _chatService;

        public ChatAppServiceTests()
        {
            _chatService = GetRequiredService<ERPPlatform.Chat.IChatAppService>();
        }

        [Fact]
        public async Task SendMessage_Should_Persist_Channel_Message()
        {
            var sent = await _chatService.SendMessageAsync(new ERPPlatform.Chat.SendChatMessageDto
            {
                ChannelName = "engineering",
                Text = "Deployed v2.5 release to production",
                SenderName = "Ahmed Hamdi"
            });

            sent.ShouldNotBeNull();
            sent.ChannelName.ShouldBe("engineering");

            var messages = await _chatService.GetChannelMessagesAsync("engineering");
            messages.Items.ShouldContain(m => m.Text == "Deployed v2.5 release to production");
        }
    }

    public class NotificationAppServiceTests : ERPPlatformEntityFrameworkCoreTestBase
    {
        private readonly ERPPlatform.Notifications.INotificationAppService _notificationService;

        public NotificationAppServiceTests()
        {
            _notificationService = GetRequiredService<ERPPlatform.Notifications.INotificationAppService>();
        }

        [Fact]
        public async Task SendNotification_And_MarkAsRead_Should_Update_Status()
        {
            var notif = await _notificationService.SendNotificationAsync(new ERPPlatform.Notifications.CreateNotificationDto
            {
                Type = "HR",
                Title = "Leave Request Approved",
                Message = "Your leave request for 3 days has been approved."
            });

            notif.ShouldNotBeNull();
            notif.IsRead.ShouldBeFalse();

            await _notificationService.MarkAsReadAsync(notif.Id);

            var list = await _notificationService.GetNotificationsAsync();
            var updated = list.Items.FirstOrDefault(n => n.Id == notif.Id);
            updated.ShouldNotBeNull();
            updated.IsRead.ShouldBeTrue();
        }
    }
}
