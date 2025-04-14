using Microsoft.EntityFrameworkCore;
using MyCinebook.ScheduleData.Models;

namespace MyCinebook.ScheduleData;

public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{
    public DbSet<ScheduledShow> ScheduledShow { get; set; }
    public DbSet<ScheduledShowSeat> ScheduledShowSeat { get; set; }

}
