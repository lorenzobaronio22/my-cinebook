using Microsoft.EntityFrameworkCore;
using MyCinebook.BookingApiService;
using MyCinebook.BookingData;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<BookingDbContext>(connectionName: "booking");

builder.Services.AddHttpClient<ScheduleClient>(
    static client => client.BaseAddress = new("https+http://scheduleapiservice"));

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Endpoints Mapping
var group = app.MapGroup("bookings");

group.MapPost("", async (HttpContext context) =>
{
    try
    {
        var bookingDto = await context.Request.ReadFromJsonAsync<BookingDto>();
        if (bookingDto == null || !bookingDto.isValid())
        {
            return Results.BadRequest("Invalid request.");
        }

        ScheduleClient scheduleClient = context.RequestServices.GetRequiredService<ScheduleClient>();
        var shows = await scheduleClient.GetShowsAsync();
        var matchingShow = shows.FirstOrDefault(show => show.Id == bookingDto.ShowId);
        if (matchingShow == null)
        {
            return Results.BadRequest("Show not found.");
        }

        // Find the first available seat
        var dbContext = context.RequestServices.GetRequiredService<BookingDbContext>();

        // Update the following block of code to ensure `allBookings` is not null before calling `Any`:
        var allBookings = await dbContext.Set<BookingModel>().ToListAsync();
        SeatDto availableSeat;
        if (allBookings == null || allBookings.Count == 0)
        {
            availableSeat = matchingShow.Seats.First();
        }
        else
        {
            availableSeat = matchingShow.Seats.FirstOrDefault(seat =>
                !allBookings.Any(booking =>
                    booking.Shows.Any(show => show.Id == matchingShow.Id &&
                        show.Seats.Any(takenSeat => takenSeat.Line == seat.Line && takenSeat.Number == seat.Number))
                )
            ) ?? throw new InvalidOperationException("No available seat found.");
        }

        if (availableSeat == null)
        {
            return Results.BadRequest("No available seats.");
        }

        // Create a new booking for the matching show and available seat
        var newBooking = new BookingModel
        {
            CreatedAt = DateTime.UtcNow,
            Shows =
            [
                new BookingShowModel
                {
                    Id = matchingShow.Id,
                    Title = matchingShow.Title,
                    Seats =
                    [
                        new BookingSeatModel
                        {
                            Id = availableSeat.Id,
                            Line = availableSeat.Line,
                            Number = availableSeat.Number
                        }
                    ]
                }
            ]
        };

        dbContext.Set<BookingModel>().Add(newBooking);
        await dbContext.SaveChangesAsync();

        return Results.Created($"/bookings/{newBooking.Id}", newBooking);
    }
    catch (Exception)
    {
        return Results.InternalServerError("An error occurred!");
    }
})
.WithName("PostBooking")
.Produces<BookingModel>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status500InternalServerError)
.Produces(StatusCodes.Status400BadRequest);

app.Run();
