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
app.MapBookingEndpoint();

app.Run();
