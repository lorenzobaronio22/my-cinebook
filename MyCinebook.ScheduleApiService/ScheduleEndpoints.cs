using MyCinebook.ScheduleApiService.Dtos;
using MyCinebook.ScheduleData;

namespace MyCinebook.ScheduleApiService;

public static class ScheduleEndpoints
{
    public static void MapScheduleEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("shows");

        group
            .MapGet("", Get)
            .WithName(nameof(Get))
            .Produces<ICollection<ResponseScheduledShowDto>>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status500InternalServerError, "text/plain");

        group
            .MapGet("/{id}/seats/availability", GetSeatsAvailability)
            .WithName(nameof(GetSeatsAvailability))
            .Produces<ICollection<ResponseScheduledShowSeatAvailabilityDto>>(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status500InternalServerError, "text/plain")
            .Produces<string>(StatusCodes.Status404NotFound, "text/plain");
        ;
    }

    public static async Task<IResult> Get(ScheduleDbContext context)
    {
        try
        {
            var response = await ScheduleService.ListScheduledShows(context);
            return TypedResults.Ok(response);
        }
        catch (Exception)
        {
            return TypedResults.InternalServerError("An error occurred!");
        }
    }
    private static async Task<IResult> GetSeatsAvailability(int id, ScheduleDbContext dbContext, BookingClient bookingClient)
    {
        try
        {
            var result = await ScheduleService.FindShowAvailability(id, dbContext, bookingClient);
            return TypedResults.Ok(result);
        }
        catch (ArgumentException)
        {
            return TypedResults.NotFound($"Show #{id} not found!");
        }
        catch (Exception)
        {
            return TypedResults.InternalServerError("An error occurred!");
        }
    }
}
