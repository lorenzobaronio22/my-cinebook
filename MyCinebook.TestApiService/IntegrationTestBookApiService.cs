using Aspire.Hosting;

namespace MyCinebook.TestApiService;

public class IntegrationTestBookApiService : IAsyncLifetime
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
            .CreateAsync<Projects.MyCinebook_BookApiService>();

        _app = await appHost.BuildAsync();
        await _app.StartAsync();
    }

    [Fact]
    public async Task ShouldStartApp()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        if (_app == null)
        {
            throw new InvalidOperationException("The application has not been initialized.");
        }

    }
}
