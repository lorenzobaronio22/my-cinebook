﻿using Microsoft.EntityFrameworkCore;
using MyCinebook.BookingApiService.Dtos;
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

    public static async Task<ICollection<Booking>> FindBookingByShow(int showId, BookingDbContext dbContext)
    {
        var bookings = await dbContext.Booking
            .Where(b => b.DeletedAt == null)
            .Include(b => b.Shows)
            .ThenInclude(s => s.Seats)
            .Where(b => b.Shows.Any(s => s.ShowId == showId))
            .ToListAsync();

        return bookings;
    }

    private static async Task<ResponseScheduledShowDto> FindShow(RequestBookingDto booking, IScheduleClient scheduleClient)
    {
        var shows = await scheduleClient.GetShowsAsync();
        var matchingShow = shows.FirstOrDefault(show => show.ID == booking.ShowId);
        return matchingShow ?? throw new ArgumentException("Show not found.");
    }

    public static async Task DeleteBooking(int id, BookingDbContext dbContext)
    {
        var booking = await dbContext.Booking.FindAsync(id)
            ?? throw new ArgumentException($"Booking {id} not found.");

        booking.DeletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }

    private static ResponseScheduledShowSeatDto FindAvailableSeat(ResponseScheduledShowDto scheduledShow, RequestBookingDto booking, BookingDbContext dbContext)
    {
        if (booking?.Seat != null && !scheduledShow.Seats.Any(s => s.Line == booking.Seat.Line && s.Number == booking.Seat.Number))
        {
            throw new ArgumentException($"Seat {booking.Seat.Line}-{booking.Seat.Number} not found in Show {scheduledShow.Title}.");
        }
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
            throw new ArgumentException($"Show {scheduledShow.Title} is sold-out.");
        }

        if (booking?.Seat == null)
        {
            return availableSeats.First();
        }

        var availableSpecificSeat = availableSeats
            .FirstOrDefault(seat => seat.Line == booking.Seat.Line && seat.Number == booking.Seat.Number);
        return availableSpecificSeat ?? throw new ArgumentException($"Seat {booking.Seat.Line}-{booking.Seat.Number} in Show {scheduledShow.Title} is not available.");
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
