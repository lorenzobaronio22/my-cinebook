using Microsoft.AspNetCore.Mvc;
using MyCinebook.BookingData;
using MyCinebook.BookingApiService.Dtos;

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

        group
            .MapGet("", Get)
            .WithName(nameof(Get))
            .Produces<ICollection<ResponseBookingDto>>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status404NotFound, "text/plain")
            .Produces<string>(StatusCodes.Status500InternalServerError, "text/plain");
    }

    public static async Task<IResult> Post([FromBody] RequestBookingDto bookingDto, ScheduleClient scheduleClient, BookingDbContext dbContext)
    {
        try
        {
            var newBooking = await BookingService.SaveBooking(bookingDto, scheduleClient, dbContext);

            var responseBookingDto = BookingService.MapToResponseBookingDto(newBooking);

            return TypedResults.Created($"/bookings/{newBooking.Id}", responseBookingDto);
        }
        catch (ArgumentException ex)
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
        catch (ArgumentException ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
        catch (Exception)
        {
            return TypedResults.InternalServerError("An error occurred!");
        }
    }
    private static async Task<IResult> Get([FromQuery] int showId, BookingDbContext dbContext)
    {
        try
        {
            var result = await BookingService.FindBookingByShow(showId, dbContext);
            var response = result.Select(BookingService.MapToResponseBookingDto).ToList();
            return TypedResults.Ok(response);
        }
        catch (Exception)
        {
            return TypedResults.InternalServerError("An error occurred!");
        }
    }

}
