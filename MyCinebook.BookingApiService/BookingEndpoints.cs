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
    }

    public static async Task<IResult> Post([FromBody] RequestBookingDto bookingDto, HttpContext context)
    {
        try
        {
            var scheduleClient = context.RequestServices.GetRequiredService<ScheduleClient>();
            var dbContext = context.RequestServices.GetRequiredService<BookingDbContext>();

            var newBooking = await BookingService.SaveBooking(bookingDto, scheduleClient, dbContext);

            var responseBookingDto = BookingService.MapToResponseBookingDto(newBooking);

            return Results.Created($"/bookings/{newBooking.Id}", responseBookingDto);
        }
        catch (BookingError error)
        {
            return Results.BadRequest(error.Message);
        }
        catch (Exception)
        {
            return Results.InternalServerError("An error occurred!");
        }
    }
}
