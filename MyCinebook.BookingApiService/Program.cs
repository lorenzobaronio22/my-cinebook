using Microsoft.EntityFrameworkCore;
using MyCinebook.BookingApiService;
using MyCinebook.BookingData;
using MyCinebook.BookingData.Models;
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
        var bookingDto = await context.Request.ReadFromJsonAsync<RequestBookingDto>();
        if (bookingDto == null || !bookingDto.IsValid())
        {
            return Results.BadRequest("Invalid request.");
        }

        ScheduleClient scheduleClient = context.RequestServices.GetRequiredService<ScheduleClient>();
        var shows = await scheduleClient.GetShowsAsync();
        var matchingShow = shows.FirstOrDefault(show => show.ID == bookingDto.ShowId);
        if (matchingShow == null)
        {
            return Results.BadRequest("Show not found.");
        }

        var dbContext = context.RequestServices.GetRequiredService<BookingDbContext>();
        var bookedSeats = await dbContext.Booking
            .Where(booking => booking.DeletedAt == null)
            .SelectMany(booking => booking.Shows)
            .Where(show => show.ShowId == matchingShow.ID)
            .SelectMany(show => show.Seats)
            .ToListAsync();

        var availableSeat = matchingShow.Seats
            .FirstOrDefault(seat => !bookedSeats.Any(bookedSeat =>
                bookedSeat.Line == seat.Line && bookedSeat.Number == seat.Number));

        if (availableSeat == null)
        {
            return Results.BadRequest($"Show ${matchingShow.Title} is sold-out.");
        }

        var newBooking = new Booking{ CreatedAt = DateTime.UtcNow, Shows = [] };
        await dbContext.Booking.AddAsync(newBooking);
        var newBookedShow = new BookedShow { ShowId = matchingShow.ID, ShowTitle = matchingShow.Title, Booking = newBooking, Seats = [] };
        newBooking.Shows.Add(newBookedShow);
        var newBookednShowSeat = new BookedShowSeat { Line = availableSeat.Line, Number = availableSeat.Number, BookedShow = newBookedShow };
        newBookedShow.Seats.Add(newBookednShowSeat);

        await dbContext.SaveChangesAsync();

        var responseBookingDto = new ResponseBookingDto
        {
            Id = newBooking.Id,
            CreatedAt = newBooking.CreatedAt,
            Shows = [.. newBooking.Shows.Select(show => new ResponseBookedShowDto
            {
                ShowId = show.ShowId,
                ShowTitle = show.ShowTitle,
                Seats = [.. show.Seats.Select(seat => new ResponseBookedShowSeatDto
                {
                    Line = seat.Line,
                    Number = seat.Number
                })]
            })]
        };

        return Results.Created($"/bookings/{newBooking.Id}", responseBookingDto);
    }
    catch (Exception)
    {
        return Results.InternalServerError("An error occurred!");
    }
})
.WithName("PostBooking")
.Produces<ResponseBookingDto>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status500InternalServerError)
.Produces(StatusCodes.Status400BadRequest);

app.Run();
