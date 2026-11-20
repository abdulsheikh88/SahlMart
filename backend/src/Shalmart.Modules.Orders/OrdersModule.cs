using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shalmart.SharedKernel;

namespace Shalmart.Modules.Orders;

public sealed class OrdersModule : IModule
{
    public string Name => "Orders";

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Orders services, DbContext, handlers here.
    }

    public void MapCustomerEndpoints(RouteGroupBuilder customer)
    {
        customer.MapPost("/orders", () => Results.StatusCode(501)); // TODO: idempotent checkout (Idempotency-Key header)
        customer.MapGet("/orders", () => Results.Ok(Array.Empty<object>()));
    }

    public void MapAdminEndpoints(RouteGroupBuilder admin)
    {
        admin.MapGet("/orders", () => Results.Ok(Array.Empty<object>())).RequireAuthorization(PolicyNames.OrdersRead);
    }
}
