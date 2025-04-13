using System;
using Microsoft.EntityFrameworkCore;
using MyCinebook.ScheduleApiService;
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

app.MapGet("/shows", (ScheduleDbContext context) =>
{
    return context.Shows.Include(s => s.Seats).ToListAsync();
})
.WithName("GetShows");

app.Run();
