using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shalmart.SharedKernel;

/// <summary>Every business module implements this so the API host can compose it.</summary>
public interface IModule
{
    string Name { get; }
    void AddServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>Endpoints for shoppers. Mounted under /api/v1/customer.</summary>
    void MapCustomerEndpoints(RouteGroupBuilder customer);

    /// <summary>Endpoints for staff. Mounted under /api/v1/admin.</summary>
    void MapAdminEndpoints(RouteGroupBuilder admin);
}
