using Microsoft.AspNetCore.Mvc;
using MyCinebook.BookingData.Models;
using MyCinebook.BookingData;
using Microsoft.EntityFrameworkCore;
using MyCinebook.BookingApiService.Dtos;
using MyCinebook.BookingApiService.Exceptions;

namespace MyCinebook.BookingApiService;

public static class BookingEndpoints
{
    public static void MapBookingEndpoint(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("bookings");

        group.MapPost("", BookingEndpoints.Post)
        .WithName("PostBooking")
        .Produces<ResponseBookingDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id}", BookingEndpoints.Delete);
    }

    public static async Task<IResult> Post([FromBody] RequestBookingDto bookingDto, HttpContext context)
    {
        try
        {
            var scheduleClient = context.RequestServices.GetRequiredService<ScheduleClient>();
            var dbContext = context.RequestServices.GetRequiredService<BookingDbContext>();

            var newBooking = await BookingService.SaveBooking(bookingDto, scheduleClient, dbContext);

            var responseBookingDto = BookingService.MapToResponseBookingDto(newBooking);

            return TypedResults.Created($"/bookings/{newBooking.Id}", responseBookingDto);
        }
        catch (BookingError error)
        {
            return TypedResults.BadRequest(error.Message);
        }
        catch (Exception)
        {
            return TypedResults.InternalServerError("An error occurred!");
        }
    }

    private static async Task<IResult> Delete(int id, BookingDbContext dbContext)
    {
        var booking = await dbContext.Booking.FindAsync(id);
        if (booking == null)
        {
            return TypedResults.NotFound();
        }
        booking.DeletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return TypedResults.NoContent();
    }

}
