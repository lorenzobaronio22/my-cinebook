using System.Net.Http.Json;
using System.Text.Json;
using IdentityModel.Client;
using MyCinebook.TestApiService;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyCinebook.Tests.Integration;

[Collection("TestApplicationCollection")]
public class IntegrationTestBookingApiService(TestApplicationFixture fixture)
{
    private readonly TestApplicationFixture _fixture = fixture;

    private readonly TimeSpan CancellationTokenTimeOut = TimeSpan.FromSeconds(120);

    private readonly int FreeShowId = 1;
    private readonly int SoldOutShowId = 2;
    private readonly int NotExistantShowId = 99;
    private readonly string FreeSeatLine = "B";
    private readonly int FreeSeatNumber = 1;

    [Fact]
    public async Task PostBookings_ShouldBookOneSeat_WhenShowIsAvailable()
    {
        // Arrange
        using var cts = new CancellationTokenSource(CancellationTokenTimeOut);
        HttpClient httpClient = await InitializeHttpClientForBooking(cts);

        // Act
        var content = JsonContent.Create(new
        {
            ShowId = FreeShowId
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

        Assert.True(jsonObject.TryGetProperty("shows", out var showsProperty), "The response JSON does not contain a 'shows' attribute.");
        Assert.Equal(JsonValueKind.Array, showsProperty.ValueKind);
        Assert.Equal(1, showsProperty.GetArrayLength());

        var showObject = showsProperty[0];
        Assert.Equal(JsonValueKind.Object, showObject.ValueKind);

        Assert.True(showObject.TryGetProperty("showId", out var showIdProperty), "The show object does not contain a 'showId' attribute.");
        Assert.Equal(JsonValueKind.Number, showIdProperty.ValueKind);
        Assert.Equal(FreeShowId, showIdProperty.GetInt32());

        Assert.True(showObject.TryGetProperty("seat", out var seatProperty), "The show object does not contain a 'seat' attribute.");
        Assert.Equal(JsonValueKind.Object, seatProperty.ValueKind);

        var seatObject = seatProperty;
        Assert.True(seatObject.TryGetProperty("line", out var lineProperty), "The seat object does not contain a 'line' attribute.");
        Assert.Equal(JsonValueKind.String, lineProperty.ValueKind);
        Assert.Equal(1, lineProperty.GetString()?.Length);

        Assert.True(seatObject.TryGetProperty("number", out var numberProperty), "The seat object does not contain a 'number' attribute.");
        Assert.Equal(JsonValueKind.Number, numberProperty.ValueKind);
        Assert.True(numberProperty.GetInt32() > 0, "The seat number is lower than 1");
    }

    [Fact]
    public async Task PostBookings_ShouldNotBook_WhenShowIsSoldOut()
    {
        // Arrange
        using var cts = new CancellationTokenSource(CancellationTokenTimeOut);
        HttpClient httpClient = await InitializeHttpClientForBooking(cts);

        // Act
        var content = JsonContent.Create(new
        {
            ShowId = SoldOutShowId
        });
        var response = await httpClient.PostAsync("/bookings", content, cts.Token);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync(cts.Token);
        Assert.Contains("sold-out", responseBody);
    }

    [Fact]
    public async Task PostBookings_ShouldNotBook_WhenShowDoesNotExist()
    {
        // Arrange
        using var cts = new CancellationTokenSource(CancellationTokenTimeOut);
        HttpClient httpClient = await InitializeHttpClientForBooking(cts);

        // Act
        var content = JsonContent.Create(new
        {
            ShowId = NotExistantShowId
        });
        var response = await httpClient.PostAsync("/bookings", content, cts.Token);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync(cts.Token);
        Assert.Contains("not found", responseBody);
    }

    private async Task<HttpClient> InitializeHttpClientForBooking(CancellationTokenSource cts)
    {
        if (_fixture.App == null)
        {
            throw new InvalidOperationException("The application has not been initialized.");
        }

        var httpClient = _fixture.App.CreateHttpClient("bookapiservice");
        await _fixture.App.ResourceNotifications.WaitForResourceHealthyAsync("bookapiservice", cts.Token);
        return httpClient;
    }

    [Fact]
    public async Task PostBookings_ShouldBookSpecificSeat_WhenShowAndSeatAreAvailable()
    {
        // Arrange
        using var cts = new CancellationTokenSource(CancellationTokenTimeOut);
        HttpClient httpClient = await InitializeHttpClientForBooking(cts);

        // Act
        var content = JsonContent.Create(new
        {
            ShowId = FreeShowId,
            Seat = new { Line = FreeSeatLine, Number = FreeSeatNumber }
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

        Assert.True(jsonObject.TryGetProperty("shows", out var showsProperty), "The response JSON does not contain a 'shows' attribute.");
        Assert.Equal(JsonValueKind.Array, showsProperty.ValueKind);
        Assert.Equal(1, showsProperty.GetArrayLength());

        var showObject = showsProperty[0];
        Assert.Equal(JsonValueKind.Object, showObject.ValueKind);

        Assert.True(showObject.TryGetProperty("showId", out var showIdProperty), "The show object does not contain a 'showId' attribute.");
        Assert.Equal(JsonValueKind.Number, showIdProperty.ValueKind);
        Assert.Equal(FreeShowId, showIdProperty.GetInt32());

        Assert.True(showObject.TryGetProperty("seat", out var seatProperty), "The show object does not contain a 'seat' attribute.");
        Assert.Equal(JsonValueKind.Object, seatProperty.ValueKind);

        var seatObject = seatProperty;
        Assert.True(seatObject.TryGetProperty("line", out var lineProperty), "The seat object does not contain a 'line' attribute.");
        Assert.Equal(JsonValueKind.String, lineProperty.ValueKind);
        Assert.Equal(FreeSeatLine, lineProperty.GetString());

        Assert.True(seatObject.TryGetProperty("number", out var numberProperty), "The seat object does not contain a 'number' attribute.");
        Assert.Equal(JsonValueKind.Number, numberProperty.ValueKind);
        Assert.Equal(FreeSeatNumber, numberProperty.GetInt32());

    }
}
