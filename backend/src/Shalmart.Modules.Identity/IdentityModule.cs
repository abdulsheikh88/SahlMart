using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shalmart.SharedKernel;

namespace Shalmart.Modules.Identity;

public sealed class IdentityModule : IModule
{
    public string Name => "Identity";

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register Identity services, DbContext, handlers here.
    }

    public void MapCustomerEndpoints(RouteGroupBuilder customer)
    {
        customer.MapPost("/auth/otp/request", () => Results.StatusCode(501)).AllowAnonymous(); // TODO: send OTP
        customer.MapPost("/auth/otp/verify", () => Results.StatusCode(501)).AllowAnonymous();  // TODO: issue tokens
    }

    public void MapAdminEndpoints(RouteGroupBuilder admin)
    {
        admin.MapGet("/users", () => Results.Ok(Array.Empty<object>())).RequireAuthorization("Admin.SuperAdmin");
    }
}
