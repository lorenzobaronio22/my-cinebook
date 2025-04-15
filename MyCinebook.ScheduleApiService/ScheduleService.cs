using Microsoft.EntityFrameworkCore;
using MyCinebook.ScheduleApiService.Dtos;
using MyCinebook.ScheduleData;

namespace MyCinebook.ScheduleApiService;

public class ScheduleService
{
    public static async Task<ICollection<ResponseScheduledShowDto>> ListScheduledShows(ScheduleDbContext context)
    {
        var shows = await context.ScheduledShow.Include(s => s.Seats).ToListAsync();
        var response = shows.Select(show => new ResponseScheduledShowDto
        {
            ID = show.ID,
            Title = show.Title,
            Seats = [.. show.Seats.Select(seat => new ResponseScheduledShowSeatDto
                    {
                        ID = seat.ID,
                        Line = seat.Line,
                        Number = seat.Number
                    })]
        }).ToList();
        return response;
    }
}
