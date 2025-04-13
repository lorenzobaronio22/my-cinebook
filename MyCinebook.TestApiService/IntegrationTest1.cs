using Microsoft.Extensions.Logging;

namespace my_cinebook.TestApiService.Tests
{
    public class IntegrationTest1
    {

        [Fact]
        public async Task GetWeatherforecastReturnsOkStatusCode()
        {
            // Arrange
            var builder = await DistributedApplicationTestingBuilder
                .CreateAsync<Projects.MyCinebook_ApiService>();

            builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });

            // To output logs to the xUnit.net ITestOutputHelper, 
            // consider adding a package from https://www.nuget.org/packages?q=xunit+logging

            await using var app = await builder.BuildAsync();

            await app.StartAsync();

            // Act
            var httpClient = app.CreateHttpClient("apiservice");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cts.Token);
            var response = await httpClient.GetAsync("/weatherforecast", cts.Token);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
