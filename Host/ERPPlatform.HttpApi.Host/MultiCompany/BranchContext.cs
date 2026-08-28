using System;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Volo.Abp.DependencyInjection;
using ERPPlatform.Domain.MultiCompany;

namespace ERPPlatform.HttpApi.Host.MultiCompany
{
    public class BranchContext : IBranchContext, ISingletonDependency
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly AsyncLocal<BranchContextScope?> _currentScope = new();

        public BranchContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? CurrentCompanyId
        {
            get
            {
                if (_currentScope.Value != null)
                {
                    return _currentScope.Value.CompanyId;
                }

                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-Company-Id", out var companyHeader))
                {
                    if (Guid.TryParse(companyHeader, out var companyId))
                    {
                        return companyId;
                    }
                }

                return null;
            }
        }

        public Guid? CurrentBranchId
        {
            get
            {
                if (_currentScope.Value != null)
                {
                    return _currentScope.Value.BranchId;
                }

                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null && httpContext.Request.Headers.TryGetValue("X-Branch-Id", out var branchHeader))
                {
                    if (Guid.TryParse(branchHeader, out var branchId))
                    {
                        return branchId;
                    }
                }

                return null;
            }
        }

        public IDisposable Change(Guid? companyId, Guid? branchId)
        {
            var parent = _currentScope.Value;
            _currentScope.Value = new BranchContextScope(companyId, branchId);
            return new DisposeAction(() => _currentScope.Value = parent);
        }

        private class BranchContextScope
        {
            public Guid? CompanyId { get; }
            public Guid? BranchId { get; }

            public BranchContextScope(Guid? companyId, Guid? branchId)
            {
                CompanyId = companyId;
                BranchId = branchId;
            }
        }

        private class DisposeAction : IDisposable
        {
            private readonly Action _action;
            public DisposeAction(Action action) => _action = action;
            public void Dispose() => _action();
        }
    }
}
