using Microsoft.EntityFrameworkCore;

namespace MyCinebook.ScheduleData;

public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{
    public DbSet<ShowModel> Shows { get; set; }
    public DbSet<SeatModel> Seats { get; set; }

}
