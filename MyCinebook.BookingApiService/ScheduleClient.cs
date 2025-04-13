using System.Text.Json;

namespace MyCinebook.BookingApiService;

public class ScheduleClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IEnumerable<ShowDto>> GetShowsAsync()
    {
        var response = await _httpClient.GetAsync("/shows");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        List<ShowDto>? shows = JsonSerializer.Deserialize<List<ShowDto>>(content, _jsonSerializerOptions);

        return shows ?? [];
    }
}
