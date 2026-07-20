using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SmeErp.Infrastructure.Identity;

namespace SmeErp.Infrastructure.Services;

public class CurrentCompanyService : ICurrentCompanyService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public CurrentCompanyService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<int?> GetCompanyIdAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User is null || httpContext.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var user = await _userManager.GetUserAsync(httpContext.User);
        return user?.CompanyId;
    }
}
