using Microsoft.AspNetCore.Mvc;
using MyCinebook.BookingData;
using MyCinebook.BookingApiService.Dtos;
using MyCinebook.BookingApiService.Exceptions;

namespace MyCinebook.BookingApiService;

public static class BookingEndpoints
{
    public static void MapBookingEndpoint(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("bookings");

        group
            .MapPost("", Post)
            .WithName(nameof(Post))
            .Produces<ResponseBookingDto>(StatusCodes.Status201Created)
            .Produces<string>(StatusCodes.Status500InternalServerError, "text/plain")
            .Produces<string>(StatusCodes.Status400BadRequest, "text/plain");

        group
            .MapDelete("/{id}", Delete)
            .WithName(nameof(Delete))
            .Produces(StatusCodes.Status204NoContent)
            .Produces<string>(StatusCodes.Status404NotFound, "text/plain")
            .Produces<string>(StatusCodes.Status500InternalServerError, "text/plain");
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
        catch (BookingError ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return TypedResults.InternalServerError("An error occurred!");
        }
    }

    private static async Task<IResult> Delete(int id, BookingDbContext dbContext)
    {
        try
        {
            await BookingService.DeleteBooking(id, dbContext);
            return TypedResults.NoContent();
        }
        catch (BookingError ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
        catch (Exception)
        {
            return TypedResults.InternalServerError("An error occurred!");
        }
    }
}
