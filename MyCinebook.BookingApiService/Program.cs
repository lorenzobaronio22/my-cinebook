using MyCinebook.BookingData;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddNpgsqlDbContext<BookingDbContext>(connectionName: "booking");

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

group.MapPost("", (HttpContext context) =>
{
    try
    {
        BookingDbContext dbContext = context.RequestServices.GetRequiredService<BookingDbContext>();
        var response = new BookingModel()
        {
            Id = 1,
        };

        return Results.Created($"/bookings/{response.Id}", response);
    }
    catch (Exception)
    {
        return Results.InternalServerError("An error occurred!");
    }
})
.WithName("PostBooking")
.Produces<BookingModel>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status500InternalServerError);

app.Run();
