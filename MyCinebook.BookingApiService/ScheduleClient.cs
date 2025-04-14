using System.Text.Json;
using MyCinebook.BookingApiService.Dtos;

namespace MyCinebook.BookingApiService;

public class ScheduleClient(HttpClient httpClient) : IScheduleClient
{
    private readonly HttpClient _httpClient = httpClient;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IEnumerable<ResponseScheduledShowDto>> GetShowsAsync()
    {
        var response = await _httpClient.GetAsync("/shows");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        List<ResponseScheduledShowDto>? shows = JsonSerializer.Deserialize<List<ResponseScheduledShowDto>>(content, _jsonSerializerOptions);

        return shows ?? [];
    }
}
