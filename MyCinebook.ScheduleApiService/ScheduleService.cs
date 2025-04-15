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

    public static async Task<ICollection<ResponseScheduledShowSeatAvailabilityDto>> FindShowAvailability(int showId, ScheduleDbContext context, IBookingClient bookingClient)
    {
        var show = await context.ScheduledShow
            .Include(s => s.Seats)
            .FirstOrDefaultAsync(s => s.ID == showId)
            ?? throw new ArgumentException($"ScheduledShow with id: {showId} not in the system!");

        var bookings = await bookingClient.GetBookingsFilteredByShowAsync(showId);

        var bookedSeats = bookings
            .SelectMany(booking => booking.Shows)
            .Where(bookedShow => bookedShow.ShowId == showId)
            .Select(bookedShow => bookedShow.Seat)
            .ToList();

        var seatAvailabilityList = show.Seats.Select(seat => new ResponseScheduledShowSeatAvailabilityDto
        {
            ID = seat.ID,
            Line = seat.Line,
            Number = seat.Number,
            Available = !bookedSeats.Any(bookedSeat => bookedSeat.Line == seat.Line && bookedSeat.Number == seat.Number)
        }).ToList();

        return seatAvailabilityList;
    }
}
