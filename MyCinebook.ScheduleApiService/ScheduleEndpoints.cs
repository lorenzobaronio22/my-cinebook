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
    }

    public static async Task<IResult> Get(ScheduleDbContext context)
    {
        var response = await ScheduleService.ListScheduledShows(context);
        return TypedResults.Ok(response);
    }
}
