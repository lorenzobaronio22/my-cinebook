using MyCinebook.BookingApiService.Dtos;
using MyCinebook.BookingApiService.Exceptions;
using MyCinebook.BookingData;
using MyCinebook.BookingData.Models;

namespace MyCinebook.BookingApiService;

public class BookingService
{
    public static ResponseBookingDto MapToResponseBookingDto(Booking booking)
    {
        return new ResponseBookingDto
        {
            Id = booking.Id,
            CreatedAt = booking.CreatedAt,
            Shows = [.. booking.Shows.Select(show => new ResponseBookedShowDto
                {
                    ShowId = show.ShowId,
                    ShowTitle = show.ShowTitle,
                    Seat = show.Seats.Select(seat => new ResponseBookedShowSeatDto
                    {
                        Line = seat.Line,
                        Number = seat.Number
                    }).First()
                })]
        };
    }

    public static async Task<Booking> SaveBooking(RequestBookingDto booking, IScheduleClient scheduleClient, BookingDbContext dbContext)
    {
        var matchingShow = await FindShow(booking, scheduleClient);

        var availableSeat = FindAvailableSeat(matchingShow, booking, dbContext);

        return SaveBooking(matchingShow, availableSeat, dbContext);
    }

    private static async Task<ResponseScheduledShowDto> FindShow(RequestBookingDto booking, IScheduleClient scheduleClient)
    {
        var shows = await scheduleClient.GetShowsAsync();
        var matchingShow = shows.FirstOrDefault(show => show.ID == booking.ShowId);
        return matchingShow ?? throw new BookingError("Show not found.");
    }

    private static ResponseScheduledShowSeatDto FindAvailableSeat(ResponseScheduledShowDto scheduledShow, RequestBookingDto booking, BookingDbContext dbContext)
    {
        var bookedSeats = dbContext.Booking
            .Where(booking => booking.DeletedAt == null)
            .SelectMany(booking => booking.Shows)
            .Where(show => show.ShowId == scheduledShow.ID)
            .SelectMany(show => show.Seats)
            .ToList();

        var availableSeats = scheduledShow.Seats
            .Where(seat => !bookedSeats.Any(bookedSeat => bookedSeat.Line == seat.Line && bookedSeat.Number == seat.Number))
            .ToList();

        if (availableSeats == null || availableSeats.Count < 1) { 
            throw new BookingError($"Show {scheduledShow.Title} is sold-out.");
        }

        if (booking?.Seat == null)
        {
            return availableSeats.First();
        }

        var availableSpecificSeat = availableSeats
            .FirstOrDefault(seat => seat.Line == booking.Seat.Line && seat.Number == booking.Seat.Number);
        return availableSpecificSeat ?? throw new BookingError($"Seat {booking.Seat.Line}-{booking.Seat.Number} in Show {scheduledShow.Title} is not availabel.");
    }

    private static Booking SaveBooking(ResponseScheduledShowDto scheduledShow, ResponseScheduledShowSeatDto seatToReseve, BookingDbContext dbContext) {
        var newBooking = new Booking { CreatedAt = DateTime.UtcNow, Shows = [] };
        dbContext.Booking.Add(newBooking);
        var newBookedShow = new BookedShow { ShowId = scheduledShow.ID, ShowTitle = scheduledShow.Title, Booking = newBooking, Seats = [] };
        newBooking.Shows.Add(newBookedShow);
        var newBookednShowSeat = new BookedShowSeat { Line = seatToReseve.Line, Number = seatToReseve.Number, BookedShow = newBookedShow };
        newBookedShow.Seats.Add(newBookednShowSeat);
        dbContext.SaveChanges();

        return newBooking;
    }

}
