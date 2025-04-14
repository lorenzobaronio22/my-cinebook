using MyCinebook.BookingApiService.Dtos;

namespace MyCinebook.BookingApiService
{
    public interface IScheduleClient
    {
        Task<IEnumerable<ResponseScheduledShowDto>> GetShowsAsync();
    }
}