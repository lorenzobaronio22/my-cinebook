using System.Net.Http.Json;
using Aspire.Hosting;

namespace MyCinebook.TestApiService;

public class IntegrationTestBookingApiService : IAsyncLifetime
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
            .CreateAsync<Projects.MyCinebook_BookingApiService>();

        _app = await appHost.BuildAsync();
        await _app.StartAsync();
    }

    [Fact]
    public async Task ShouldBookOneSeat()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        if (_app == null)
        {
            throw new InvalidOperationException("The application has not been initialized.");
        }

        var httpClient = _app.CreateHttpClient("bookapiservice");
        await _app.ResourceNotifications.WaitForResourceHealthyAsync("bookapiservice", cts.Token);

        // Act
        var content = JsonContent.Create(new
        {
            Show = "The Matrix"
        });
        var response = await httpClient.PostAsync("/bookings", content, cts.Token);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
