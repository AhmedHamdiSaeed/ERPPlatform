using System;

namespace ERPPlatform.Domain.MultiCompany
{
    public interface IMultiBranch
    {
        Guid? CompanyId { get; set; }
        Guid? BranchId { get; set; }
    }
}
