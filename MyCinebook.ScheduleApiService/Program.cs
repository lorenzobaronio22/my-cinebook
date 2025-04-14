using Microsoft.EntityFrameworkCore;
using MyCinebook.ScheduleApiService;
using MyCinebook.ScheduleData;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<ScheduleDbContext>(connectionName: "schedule");

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
app.MapGet("/shows", async (ScheduleDbContext context) =>
{
    var shows = await context.ScheduledShow.Include(s => s.Seats).ToListAsync();
    var response = shows.Select(show => new ResponseScheduledShowDto
    {
        ID = show.ID,
        Title = show.Title,
        Seats = [.. show.Seats.Select(seat => new ResponseScheduledShowSeatDto
        {
            ID = seat.ID,
            Line = seat.Line,
            Number = seat.Number
        })]
    }).ToList();

    return Results.Ok(response);
})
.WithName("GetShows")
.Produces<ICollection<ResponseScheduledShowDto>>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status500InternalServerError);

app.Run();
