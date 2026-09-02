namespace ERPPlatform.Domain.Reports
{
    /// <summary>
    /// Well-known report data source codes. The catalog (ReportDefinition.DataSourceCode)
    /// points at one of these, and the report runner dispatches on it.
    /// </summary>
    public static class ReportDataSources
    {
        public const string HrHeadcount = "hr-headcount";
        public const string InventoryValuation = "inventory-valuation";
        public const string OpenPurchaseOrders = "open-purchase-orders";
        public const string PendingApprovals = "pending-approvals";

        public static readonly string[] All =
        {
            HrHeadcount,
            InventoryValuation,
            OpenPurchaseOrders,
            PendingApprovals
        };
    }
}
