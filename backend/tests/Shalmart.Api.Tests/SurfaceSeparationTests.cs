using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Shalmart.Api.Tests;

public class SurfaceSeparationTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Health_is_public()
    {
        var res = await factory.CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/admin/orders")]
    [InlineData("/api/v1/customer/orders")]
    public async Task Protected_endpoints_reject_anonymous(string url)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var res = await client.GetAsync(url);
        Assert.True(res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Redirect);
    }
}
