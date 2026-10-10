using System.Net;
using System.Text;
using Folio.Infrastructure.Integrations;

namespace Folio.Infrastructure.Tests;

public sealed class HttpRegistryEntityClientTests
{
    private const string ResponseJson =
        """
        [
          { "entityId": 101, "displayValue": "HC Sparta Praha", "registrationNumber": "12345678", "companyCode": "SPA" },
          { "entityId": 102, "displayValue": "HC Slavia Praha", "registrationNumber": null, "companyCode": null }
        ]
        """;

    [Fact]
    public async Task GetEntitiesValidOnAsync_calls_folio_integration_entities_under_the_base_path()
    {
        var handler = new StubHandler(ResponseJson);
        var client = CreateClient(handler);

        await client.GetEntitiesValidOnAsync(new DateOnly(2026, 6, 30));

        Assert.Equal(HttpMethod.Get, handler.LastRequest?.Method);
        Assert.Equal(
            "https://registry-dev.example.com/api/folio-integration/entities?asOfDate=2026-06-30",
            handler.LastRequest?.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetEntitiesValidOnAsync_maps_the_response_to_snapshots()
    {
        var client = CreateClient(new StubHandler(ResponseJson));
        var date = new DateOnly(2026, 6, 30);

        var entities = await client.GetEntitiesValidOnAsync(date);

        Assert.Collection(entities,
            e =>
            {
                Assert.Equal(101, e.RegistryEntityId);
                Assert.Equal("HC Sparta Praha", e.Name);
                Assert.Equal("SPA", e.Code);
                Assert.Equal("12345678", e.RegistrationNumber);
                Assert.Equal(date, e.ValidOn);
            },
            e =>
            {
                Assert.Equal(102, e.RegistryEntityId);
                Assert.Equal("HC Slavia Praha", e.Code);
                Assert.Null(e.RegistrationNumber);
            });
    }

    private static HttpRegistryEntityClient CreateClient(HttpMessageHandler handler) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://registry-dev.example.com/api/") });

    private sealed class StubHandler(string json) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });
        }
    }
}
