using MyCinebook.ScheduleApiService.Dtos;

namespace MyCinebook.ScheduleApiService;

public interface IBookingClient
{
    Task<IEnumerable<ResponseBookingDto>> GetBookingsFilteredByShowAsync(int showId);
}
