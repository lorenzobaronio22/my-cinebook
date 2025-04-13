using Microsoft.EntityFrameworkCore;

namespace MyCinebook.ScheduleData;

public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{
    public DbSet<ScheduleShowModel> Shows { get; set; }
    public DbSet<ScheduleSeatModel> Seats { get; set; }

}
