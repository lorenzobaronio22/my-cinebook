using System.Net.Http.Json;
using System.Text.Json;

namespace MyCinebook.TestApiService;

[Collection("TestApplicationCollection")]
public class IntegrationTestBookingApiService(TestApplicationFixture fixture)
{
    private readonly TestApplicationFixture _fixture = fixture;

    [Fact]
    public async Task ShouldBookOneSeat()
    {
        // Arrange
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
        if (_fixture.App == null)
        {
            throw new InvalidOperationException("The application has not been initialized.");
        }

        var httpClient = _fixture.App.CreateHttpClient("bookapiservice");
        await _fixture.App.ResourceNotifications.WaitForResourceHealthyAsync("bookapiservice", cts.Token);

        // Act
        var content = JsonContent.Create(new
        {
            ShowId = 1
        });
        var response = await httpClient.PostAsync("/bookings", content, cts.Token);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync(cts.Token);
        var jsonObject = JsonSerializer.Deserialize<JsonElement>(responseBody);
        Assert.Equal(JsonValueKind.Object, jsonObject.ValueKind);

        Assert.True(jsonObject.TryGetProperty("id", out var bookingId), "The response JSON does not contain an 'id' attribute.");

        Assert.Equal(JsonValueKind.Number, bookingId.ValueKind);
        Assert.True(bookingId.TryGetInt32(out _), "The 'id' attribute is not of type int.");

        Assert.True(jsonObject.TryGetProperty("createdAt", out var createdAt), "The response JSON does not contain a 'createdAt' attribute.");

        Assert.Equal(JsonValueKind.String, createdAt.ValueKind);
        Assert.True(DateTime.TryParse(createdAt.GetString(), out var createdAtDateTime), "The 'createdAt' attribute is not a valid DateTime string.");
        Assert.Equal(DateTime.Today, createdAtDateTime.Date);
    }
}
