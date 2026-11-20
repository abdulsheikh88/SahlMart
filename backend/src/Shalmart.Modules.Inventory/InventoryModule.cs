using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shalmart.SharedKernel;

namespace Shalmart.Modules.Inventory;

public sealed class InventoryModule : IModule
{
    public string Name => "Inventory";

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Inventory services, DbContext, handlers here.
    }

    public void MapCustomerEndpoints(RouteGroupBuilder customer)
    {
        // Customers see stock only through the catalog; no direct endpoints.
    }

    public void MapAdminEndpoints(RouteGroupBuilder admin)
    {
        admin.MapGet("/inventory/{storeId}", (string storeId) => Results.Ok(Array.Empty<object>())).RequireAuthorization(PolicyNames.InventoryWrite);
    }
}
