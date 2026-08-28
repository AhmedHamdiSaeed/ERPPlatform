using System;

namespace ERPPlatform.Domain.MultiCompany
{
    public interface IBranchContext
    {
        Guid? CurrentCompanyId { get; }
        Guid? CurrentBranchId { get; }
        IDisposable Change(Guid? companyId, Guid? branchId);
    }
}
