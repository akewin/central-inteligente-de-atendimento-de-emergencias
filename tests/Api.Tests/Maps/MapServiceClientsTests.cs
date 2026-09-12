using System.Net;
using System.Text;
using Api.Geography;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace Api.Tests.Maps;

public sealed class MapServiceClientsTests
{
    [Fact]
    public async Task OverpassFinder_MapsNodeAndWayCenter_WithEmergencyMetadata()
    {
        const string json = """
            {
              "elements": [
                {
                  "type": "node",
                  "id": 10,
                  "lat": -22.9,
                  "lon": -47.1,
                  "tags": {
                    "name": "Hospital Central",
                    "emergency": "yes",
                    "addr:street": "Rua A",
                    "addr:housenumber": "25",
                    "addr:city": "Araras"
                  }
                },
                {
                  "type": "way",
                  "id": 20,
                  "center": { "lat": -22.91, "lon": -47.11 },
                  "tags": { "official_name": "Santa Casa" }
                }
              ]
            }
            """;
        var handler = new StubHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Contains("[\"amenity\"=\"hospital\"]", Uri.UnescapeDataString(request.RequestUri!.Query));
            return Json(json);
        });
        var finder = CreateFinder(handler, "https://overpass.example/api/");

        var result = await finder.FindNearbyAsync(-22.9m, -47.1m, 15000);

        Assert.Collection(result,
            first =>
            {
                Assert.Equal("node/10", first.Id);
                Assert.Equal("Hospital Central", first.Name);
                Assert.Equal("Rua A, 25 — Araras", first.Address);
                Assert.True(first.HasEmergencyDepartment);
            },
            second =>
            {
                Assert.Equal("way/20", second.Id);
                Assert.Equal("Santa Casa", second.Name);
                Assert.False(second.HasEmergencyDepartment);
            });
    }

    [Fact]
    public async Task OverpassFinder_FallsBackAndCachesSuccessfulResponse()
    {
        var requestCount = 0;
        var handler = new StubHandler((request, _) =>
        {
            requestCount++;
            if (request.RequestUri!.Host == "primary.example")
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

            return Json("""
                { "elements": [{ "type": "node", "id": 30, "lat": -22.9, "lon": -47.1, "tags": { "name": "Hospital de Apoio" } }] }
                """);
        });
        var finder = CreateFinder(handler,
            "https://primary.example/api/",
            "https://fallback.example/api/");

        var first = await finder.FindNearbyAsync(-22.9m, -47.1m, 15000);
        var cached = await finder.FindNearbyAsync(-22.9m, -47.1m, 15000);

        Assert.Single(first);
        Assert.Single(cached);
        Assert.Equal("Hospital de Apoio", cached[0].Name);
        Assert.Equal(2, requestCount);
    }

    [Fact]
    public async Task OsrmRouting_MapsRoadEstimatesAndThreePointRoute()
    {
        var handler = new StubHandler((request, _) =>
        {
            var path = request.RequestUri!.PathAndQuery;
            if (path.Contains("/table/", StringComparison.Ordinal))
            {
                Assert.Contains("sources=0", path);
                Assert.Contains("destinations=1;2", Uri.UnescapeDataString(path));
                return Json("""
                    { "code": "Ok", "distances": [[1200.0, 3400.0]], "durations": [[180.0, 420.0]] }
                    """);
            }

            Assert.Contains("/route/", path);
            Assert.Contains("geometries=geojson", path);
            return Json("""
                {
                  "code": "Ok",
                  "routes": [{
                    "distance": 5100.0,
                    "duration": 600.0,
                    "legs": [
                      { "distance": 2000.0, "duration": 240.0 },
                      { "distance": 3100.0, "duration": 360.0 }
                    ],
                    "geometry": { "coordinates": [[-47.1, -22.9], [-47.2, -23.0]] }
                  }]
                }
                """);
        });
        var routing = new OsrmMapRoutingService(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://osrm.example/")
        });
        var origin = new MapCoordinate(-22.9m, -47.1m);

        var estimates = await routing.EstimateFromAsync(origin,
            [new MapCoordinate(-22.91m, -47.11m), new MapCoordinate(-22.92m, -47.12m)]);
        var route = await routing.FindRouteAsync(
            [origin, new MapCoordinate(-22.91m, -47.11m), new MapCoordinate(-22.92m, -47.12m)]);

        Assert.Equal(1.2, estimates[0].DistanceKm);
        Assert.Equal(3, estimates[0].DurationMinutes);
        Assert.Equal(3.4, estimates[1].DistanceKm);
        Assert.NotNull(route);
        Assert.Equal(5.1, route.DistanceKm);
        Assert.Equal(10, route.DurationMinutes);
        Assert.Equal(2, route.Legs.Count);
        Assert.Equal(-22.9m, route.Geometry[0].Latitude);
        Assert.Equal(-47.1m, route.Geometry[0].Longitude);
    }

    private static HttpResponseMessage Json(string body) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };

    private static OverpassHospitalFinder CreateFinder(HttpMessageHandler handler, params string[] baseUrls)
        => new(
            new HttpClient(handler),
            new MapServicesOptions { OverpassBaseUrls = baseUrls },
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<OverpassHospitalFinder>.Instance);

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, string, HttpResponseMessage> _response;

        public StubHandler(Func<HttpRequestMessage, string, HttpResponseMessage> response)
            => _response = response;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return _response(request, body);
        }
    }
}
