using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shalmart.SharedKernel;

namespace Shalmart.Modules.Payments;

public sealed class PaymentsModule : IModule
{
    public string Name => "Payments";

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Payments services, DbContext, handlers here.
    }

    public void MapCustomerEndpoints(RouteGroupBuilder customer)
    {
        customer.MapPost("/payments/intent", () => Results.StatusCode(501)); // TODO: create gateway order
    }

    public void MapAdminEndpoints(RouteGroupBuilder admin)
    {
        admin.MapPost("/payments/{id}/refund", (string id) => Results.StatusCode(501)).RequireAuthorization(PolicyNames.OrdersRefund);
    }
}
