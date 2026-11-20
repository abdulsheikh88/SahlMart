using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shalmart.SharedKernel;

namespace Shalmart.Modules.Catalog;

public sealed class CatalogModule : IModule
{
    public string Name => "Catalog";

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Catalog services, DbContext, handlers here.
    }

    public void MapCustomerEndpoints(RouteGroupBuilder customer)
    {
        customer.MapGet("/catalog/categories", () => Results.Ok(Array.Empty<object>())).AllowAnonymous();
        customer.MapGet("/catalog/products", (string? storeId) => Results.Ok(Array.Empty<object>())).AllowAnonymous();
    }

    public void MapAdminEndpoints(RouteGroupBuilder admin)
    {
        admin.MapGet("/catalog/products", () => Results.Ok(Array.Empty<object>())).RequireAuthorization(PolicyNames.CatalogEdit);
    }
}
