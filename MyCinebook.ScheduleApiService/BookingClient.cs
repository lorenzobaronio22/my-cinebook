using System.Text.Json;
using MyCinebook.ScheduleApiService.Dtos;

namespace MyCinebook.ScheduleApiService;

public class BookingClient(HttpClient httpClient) : IBookingClient
{
    private readonly HttpClient httpClient = httpClient;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IEnumerable<ResponseBookingDto>> GetBookingsFilteredByShowAsync(int showId)
    {
        var response = await httpClient.GetAsync($"/bookings?showId={showId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        List<ResponseBookingDto>? result = JsonSerializer.Deserialize<List<ResponseBookingDto>>(content, _jsonSerializerOptions);

        return result ?? [];
    }
}
