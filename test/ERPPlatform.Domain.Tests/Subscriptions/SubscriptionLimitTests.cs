using System;
using Xunit;
using ERPPlatform.Domain.Subscriptions;

namespace ERPPlatform.Domain.Tests.Subscriptions
{
    public class SubscriptionLimitTests
    {
        [Fact]
        public void FeatureConstants_Should_Have_Correct_Format()
        {
            Assert.Equal("ERP.Invoices.Monthly", ErpFeatures.InvoicesMonthly);
            Assert.Equal("ERP.Users.Max", ErpFeatures.UsersMax);
            Assert.Equal("ERP.Branches.Max", ErpFeatures.BranchesMax);
            Assert.Equal("ERP.Employees.Max", ErpFeatures.EmployeesMax);
            Assert.Equal("ERP.Quotations.Monthly", ErpFeatures.QuotationsMonthly);
            Assert.Equal("ERP.Storage.MaxGB", ErpFeatures.StorageMaxGb);
            Assert.Equal("ERP.AdvancedReports", ErpFeatures.AdvancedReports);
            Assert.Equal("ERP.ApiAccess", ErpFeatures.ApiAccess);
            Assert.Equal("ERP.MultiCurrency", ErpFeatures.MultiCurrency);
        }

        [Theory]
        [InlineData(100, 73, true)]
        [InlineData(100, 100, false)]
        [InlineData(100, 101, false)]
        public void MonthlyLimit_Evaluation_Logic(long limit, long currentUsage, bool expectedAllowed)
        {
            bool allowed = (currentUsage + 1) <= limit;
            Assert.Equal(expectedAllowed, allowed);
        }
    }
}
