using System.Net.Http.Json;
using System.Text.Json;
using IdentityModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using MyCinebook.TestApiService;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyCinebook.Tests.Integration;

[Collection("TestApplicationCollection")]
public class IntegrationTestBookingApiService(TestApplicationFixture fixture)
{
    private readonly TestApplicationFixture _fixture = fixture;

    private readonly TimeSpan CancellationTokenTimeOut = TimeSpan.FromSeconds(120);

    private readonly int FreeShowId = 1;
    private readonly string FreeSeatLine = "B";
    private readonly int FreeSeatNumber = 1;
    private readonly string BookedSeatLine = "A";
    private readonly int BookedSeatNumber = 1;
    private readonly int SoldOutShowId = 2;
    private readonly int NotExistantShowId = 9999;
    private readonly string NotExistantSeatLine = "X";
    private readonly int NotExistantSeatNumber = 9999;
    private readonly int NotExistantBooking = 9999;

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
        Assert.Contains("Show not found", responseBody);
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

    [Fact]
    public async Task PostBookings_ShouldNotBookSpecificSeat_WhenShowIsAvailableButSeatIsAlreadyBooked()
    {
        // Arrange
        using var cts = new CancellationTokenSource(CancellationTokenTimeOut);
        HttpClient httpClient = await InitializeHttpClientForBooking(cts);

        // Act
        var content = JsonContent.Create(new
        {
            ShowId = FreeShowId,
            Seat = new { Line = BookedSeatLine, Number = BookedSeatNumber }
        });
        var response = await httpClient.PostAsync("/bookings", content, cts.Token);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync(cts.Token);
        Assert.Contains("not available", responseBody);
    }

    [Fact]
    public async Task PostBookings_ShouldNotBookSpecificSeat_WhenShowIsAvailableButSeatDoesNotExists()
    {
        // Arrange
        using var cts = new CancellationTokenSource(CancellationTokenTimeOut);
        HttpClient httpClient = await InitializeHttpClientForBooking(cts);

        // Act
        var content = JsonContent.Create(new
        {
            ShowId = FreeShowId,
            Seat = new { Line = NotExistantSeatLine, Number = NotExistantSeatNumber }
        });
        var response = await httpClient.PostAsync("/bookings", content, cts.Token);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync(cts.Token);
        Assert.Contains("Seat", responseBody);
        Assert.Contains("not found", responseBody);
    }

    [Fact]
    public async Task PostBookings_ShouldNotBookSpecificSeat_WhenShowDoesNotExist()
    {
        // Arrange
        using var cts = new CancellationTokenSource(CancellationTokenTimeOut);
        HttpClient httpClient = await InitializeHttpClientForBooking(cts);

        // Act
        var content = JsonContent.Create(new
        {
            ShowId = NotExistantShowId,
            Seat = new { Line = "A", Number = 1 }
        });
        var response = await httpClient.PostAsync("/bookings", content, cts.Token);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync(cts.Token);
        Assert.Contains("Show not found", responseBody);
    }
    [Fact]
    public async Task DeleteBookings_ShouldInvalidateBookingAndFreeSeats_WhenBookingIsValid()
    {
        // Arrange
        using var cts = new CancellationTokenSource(CancellationTokenTimeOut);
        HttpClient httpClient = await InitializeHttpClientForBooking(cts);

        var arrangeContent = JsonContent.Create(new
        {
            ShowId = FreeShowId,
        });
        var arrangeResponse = await httpClient.PostAsync("/bookings", arrangeContent, cts.Token);
        var arrangeResponseBody = await arrangeResponse.Content.ReadAsStringAsync(cts.Token);
        var arrangeJsonObject = JsonSerializer.Deserialize<JsonElement>(arrangeResponseBody);
        arrangeJsonObject.TryGetProperty("id", out var propertyBookingId);
        int testBookingId = propertyBookingId.GetInt32();
        arrangeJsonObject.TryGetProperty("shows", out var showsProperty);
        var arrangeShowObject = showsProperty[0];
        arrangeShowObject.TryGetProperty("seat", out var seatProperty);
        var arrangeSeatObject = seatProperty;
        arrangeSeatObject.TryGetProperty("line", out var lineProperty);
        string testSeatLine = lineProperty.GetString() ?? throw new Exception("missing testSeatLine");
        arrangeSeatObject.TryGetProperty("number", out var numberProperty);
        int testSeatNumber = numberProperty.GetInt32();

        // Act
        var response = await httpClient.DeleteAsync($"/bookings/{testBookingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var assertContent = JsonContent.Create(new
        {
            ShowId = FreeShowId,
            Seat = new { Line = testSeatLine, Number = testSeatNumber }
        });
        var assertResponse = await httpClient.PostAsync("/bookings", assertContent, cts.Token);
        Assert.Equal(HttpStatusCode.Created, assertResponse.StatusCode);
        var assertResponseBody = await assertResponse.Content.ReadAsStringAsync(cts.Token);
        var assertJsonObject = JsonSerializer.Deserialize<JsonElement>(assertResponseBody);
        assertJsonObject.TryGetProperty("id", out var assertPropertyBookingId);
        Assert.NotEqual(testBookingId, assertPropertyBookingId.GetInt32());
        assertJsonObject.TryGetProperty("shows", out var assertShowsProperty);
        var assertShowObject = assertShowsProperty[0];
        assertShowObject.TryGetProperty("seat", out var assertSeatProperty);
        var assertSeatObject = assertSeatProperty;
        assertSeatObject.TryGetProperty("line", out var asseertLineProperty);
        Assert.Equal(testSeatLine, asseertLineProperty.GetString());
        assertSeatObject.TryGetProperty("number", out var assertNumberProperty);
        Assert.Equal(testSeatNumber, assertNumberProperty.GetInt32());

    }

    [Fact]
    public async Task DeleteBookings_ShouldNotCancelBooking_WhenBookingDoesntExist()
    {
        // Arrange
        using var cts = new CancellationTokenSource(CancellationTokenTimeOut);
        HttpClient httpClient = await InitializeHttpClientForBooking(cts);

        // Act
        var response = await httpClient.DeleteAsync($"/bookings/{NotExistantBooking}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

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
}
