namespace MyCinebook.BookingApiService;

public static class BookingEndpoints
{
    public static void MapBookEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("bookings");
        group.MapPost("", CreateBooking);
    }

    private static async Task<IResult> CreateBooking()
    {
        var bookingId = Guid.NewGuid();

        return Results.Created($"/bookings/{bookingId}", new { Id = bookingId });
    }
}
