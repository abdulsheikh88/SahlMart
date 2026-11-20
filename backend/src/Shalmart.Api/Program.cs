using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Shalmart.Modules.Catalog;
using Shalmart.Modules.Identity;
using Shalmart.Modules.Inventory;
using Shalmart.Modules.Orders;
using Shalmart.Modules.Payments;
using Shalmart.SharedKernel;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ---- Modules (modular monolith) ----
IModule[] modules =
[
    new IdentityModule(), new CatalogModule(), new InventoryModule(),
    new OrdersModule(), new PaymentsModule()
];
foreach (var m in modules) m.AddServices(builder.Services, config);

// ---- Authentication: two separate schemes, issuers and audiences ----
builder.Services.AddAuthentication()
    .AddJwtBearer(AuthSchemes.Customer, o =>
    {
        o.Authority = config["Auth:Customer:Authority"];
        o.Audience = config["Auth:Customer:Audience"];
    })
    .AddJwtBearer(AuthSchemes.Admin, o =>
    {
        o.Authority = config["Auth:Admin:Authority"];   // Microsoft Entra ID
        o.Audience = config["Auth:Admin:Audience"];
    });

// ---- Authorization: permission claims + role policies ----
builder.Services.AddAuthorization(o =>
{
    void Perm(string name) => o.AddPolicy(name, p =>
        p.AddAuthenticationSchemes(AuthSchemes.Admin)
         .RequireAuthenticatedUser()
         .RequireClaim("permission", name));

    Perm(PolicyNames.CatalogEdit);
    Perm(PolicyNames.InventoryWrite);
    Perm(PolicyNames.OrdersRead);
    Perm(PolicyNames.OrdersRefund);

    o.AddPolicy("Admin.SuperAdmin", p =>
        p.AddAuthenticationSchemes(AuthSchemes.Admin)
         .RequireAuthenticatedUser()
         .RequireRole("SuperAdmin"));
    // TODO: add a handler that checks the target storeId against the user's store_ids claim.
});

// ---- CORS: explicit origins per surface, never wildcard ----
builder.Services.AddCors(o =>
{
    o.AddPolicy("customer", p => p
        .WithOrigins(config.GetSection("Cors:CustomerOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
    o.AddPolicy("admin", p => p
        .WithOrigins(config.GetSection("Cors:AdminOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials());
});

// ---- Rate limiting: stricter on the customer surface and OTP ----
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("customer", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1) }));
    o.AddPolicy("admin", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.User.Identity?.Name ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 300, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors();            // policies are applied per route group below
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.MapHealthChecks("/health").AllowAnonymous();

// ---- Customer surface: /api/v1/customer/* ----
var customer = app.MapGroup("/api/v1/customer")
    .RequireCors("customer").RequireRateLimiting("customer")
    .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = AuthSchemes.Customer });

// ---- Admin surface: /api/v1/admin/* ----
var admin = app.MapGroup("/api/v1/admin")
    .RequireCors("admin").RequireRateLimiting("admin")
    .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = AuthSchemes.Admin });

foreach (var m in modules)
{
    m.MapCustomerEndpoints(customer);
    m.MapAdminEndpoints(admin);
}

app.Run();

public partial class Program;   // for integration tests
