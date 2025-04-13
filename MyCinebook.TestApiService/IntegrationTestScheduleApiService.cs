using System.Text.Json;
using Aspire.Hosting;
using IdentityModel.Client;

namespace MyCinebook.TestApiService;

public class IntegrationTestScheduleApiService : IAsyncLifetime
{
    private DistributedApplication? _app;

    public async Task DisposeAsync()
    {
        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MyCinebook_ScheduleApiService>();

        _app = await appHost.BuildAsync();
        await _app.StartAsync();
    }

    [Fact]
    public async Task ShouldListScheduledShows()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        if (_app == null)
        {
            throw new InvalidOperationException("The application has not been initialized.");
        }

        var httpClient = _app.CreateHttpClient("scheduleapiservice");
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("scheduleapiservice", cts.Token);

        // Act
        var response = await httpClient.GetAsync("/shows", cts.Token);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync(cts.Token);
        var jsonArray = JsonSerializer.Deserialize<JsonElement>(responseBody);

        Assert.True(jsonArray.ValueKind == JsonValueKind.Array, "Response body is not a JSON array.");
        Assert.Equal(2, jsonArray.GetArrayLength());

        foreach (var element in jsonArray.EnumerateArray())
        {
            Assert.True(element.TryGetProperty("seats", out var seatsProperty), "Element does not have a 'seats' property.");
            Assert.True(seatsProperty.ValueKind == JsonValueKind.Array, "'seats' property is not a JSON array.");
            Assert.Equal(6, seatsProperty.GetArrayLength());

            var expectedSeats = new[] { "A-1", "A-2", "A-3", "B-1", "B-2", "B-3" };
            var actualSeats = seatsProperty.EnumerateArray()
                .Select(seat =>
                {
                    return $"{seat.TryGetString("line")}-{seat.TryGetInt("number")}";
                })
                .ToArray();

            Assert.Equal(expectedSeats, actualSeats);
        }

        var firstElement = jsonArray[0];
        Assert.True(firstElement.TryGetProperty("title", out var titleProperty), "First element does not have a 'title' property.");
        Assert.Equal("The Matrix", titleProperty.GetString());

        var secondElement = jsonArray[1];
        Assert.True(secondElement.TryGetProperty("title", out var secondTitleProperty), "Second element does not have a 'title' property.");
        Assert.Equal("Inception", secondTitleProperty.GetString());
    }
}
