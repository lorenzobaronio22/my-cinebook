using System.Text.Json;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;

namespace my_cinebook.TestApiService.Tests
{
    public class IntegrationTestScheduleApiService
    {

        [Fact]
        public async Task ShouldListScheduledShows()
        {
            // Arrange
            var builder = await DistributedApplicationTestingBuilder
                .CreateAsync<Projects.MyCinebook_ScheduleApiService>();

            builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                clientBuilder.AddStandardResilienceHandler();
            });

            await using var app = await builder.BuildAsync();
            await app.StartAsync();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var httpClient = app.CreateHttpClient("scheduleapiservice");
            await app.ResourceNotifications.WaitForResourceHealthyAsync("scheduleapiservice", cts.Token);

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
}
