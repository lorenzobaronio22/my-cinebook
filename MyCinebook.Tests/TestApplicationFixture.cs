using Aspire.Hosting;

namespace MyCinebook.TestApiService;

public class TestApplicationFixture : IAsyncLifetime
{
    public DistributedApplication? App { get; private set; }

    public async Task InitializeAsync()
    {
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MyCinebook_AppHost>();
        App = await appHost.BuildAsync();
        await App.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (App != null)
        {
            await App.DisposeAsync();
        }
    }
}
