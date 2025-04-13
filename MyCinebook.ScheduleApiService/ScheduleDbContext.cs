using System;
using Microsoft.EntityFrameworkCore;

namespace MyCinebook.ScheduleApiService;

public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{
    public DbSet<ShowModel> Shows { get; set; }
    public DbSet<SeatModel> Seats { get; set; }

}
