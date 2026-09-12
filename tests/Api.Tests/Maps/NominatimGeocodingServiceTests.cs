using System.Net;
using System.Text;
using Api.Geography;
using Api.Services.Geography;

namespace Api.Tests.Maps;

public sealed class NominatimGeocodingServiceTests
{
    [Fact]
    public async Task Geocode_WhenDetailedAddressFails_FallsBackToPostalCode()
    {
        var requests = new List<Uri>();
        var handler = new StubHandler(request =>
        {
            requests.Add(request.RequestUri!);
            var body = request.RequestUri!.Query.Contains("postalcode=13606340") &&
                !request.RequestUri.Query.Contains("street=") && !request.RequestUri.Query.Contains("q=")
                ? "[{\"lat\":\"-22.3572\",\"lon\":\"-47.3842\"}]"
                : "[]";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        });
        var service = new NominatimGeocodingService(new HttpClient(handler)
        { BaseAddress = new Uri("https://nominatim.example/") });

        var result = await service.GeocodeAsync(new GeocodingAddress(
            "13606340", "Rua Lázaro Lima", "146", null,
            "Jardim José Ometto II", "3503307", "SP"));

        Assert.Equal(-22.3572m, result.Latitude);
        Assert.Equal(-47.3842m, result.Longitude);
        Assert.Equal("NOMINATIM", result.Source);
        Assert.Equal(3, requests.Count);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> response)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(response(request));
    }
}
