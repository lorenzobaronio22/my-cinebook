using System.Text.Json;
using System.Xml.Linq;
using Aspire.Hosting;
using IdentityModel.Client;
using static Google.Protobuf.Compiler.CodeGeneratorResponse.Types;

namespace MyCinebook.TestApiService;

[Collection("TestApplicationCollection")]
public class IntegrationTestScheduleApiService(TestApplicationFixture fixture)
{
    private readonly TestApplicationFixture _fixture = fixture;

    [Fact]
    public async Task ShouldListScheduledShows()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        if (_fixture.App == null)
        {
            throw new InvalidOperationException("The application has not been initialized.");
        }

        var httpClient = _fixture.App.CreateHttpClient("scheduleapiservice");
        await _fixture.App.ResourceNotifications.WaitForResourceHealthyAsync("scheduleapiservice", cts.Token);

        // Act
        var response = await httpClient.GetAsync("/shows", cts.Token);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync(cts.Token);
        var jsonArray = JsonSerializer.Deserialize<JsonElement>(responseBody);

        Assert.True(jsonArray.ValueKind == JsonValueKind.Array, "Response body is not a JSON array.");
        Assert.Equal(2, jsonArray.GetArrayLength());

        var firstElement = jsonArray[0];
        Assert.True(firstElement.TryGetProperty("title", out var titleProperty), "First element does not have a 'title' property.");
        Assert.Equal("The Matrix", titleProperty.GetString());
        Assert.True(firstElement.TryGetProperty("seats", out var firstElementseatsProperty), "First Element does not have a 'seats' property.");
        Assert.True(firstElementseatsProperty.ValueKind == JsonValueKind.Array, "'seats' property is not a JSON array.");
        Assert.Equal(6, firstElementseatsProperty.GetArrayLength());
        var firstElementexpectedSeats = new[] { "A-1", "A-2", "A-3", "B-1", "B-2", "B-3" };
        var firstElementactualSeats = firstElementseatsProperty.EnumerateArray()
            .Select(seat =>
            {
                return $"{seat.TryGetString("line")}-{seat.TryGetInt("number")}";
            })
            .ToArray();
        Assert.Equal(firstElementexpectedSeats, firstElementactualSeats);


        var secondElement = jsonArray[1];
        Assert.True(secondElement.TryGetProperty("title", out var secondTitleProperty), "Second element does not have a 'title' property.");
        Assert.Equal("Private show", secondTitleProperty.GetString());
        Assert.True(secondElement.TryGetProperty("seats", out var secondElementseatsProperty), "Second Element does not have a 'seats' property.");
        Assert.True(secondElementseatsProperty.ValueKind == JsonValueKind.Array, "'seats' property is not a JSON array.");
        Assert.Equal(1, secondElementseatsProperty.GetArrayLength());
        var secondElemenExpectedSeats = new[] { "A-1" };
        var secondElemenActualSeats = secondElementseatsProperty.EnumerateArray()
            .Select(seat =>
            {
                return $"{seat.TryGetString("line")}-{seat.TryGetInt("number")}";
            })
            .ToArray();
        Assert.Equal(secondElemenExpectedSeats, secondElemenActualSeats);
    }
}
