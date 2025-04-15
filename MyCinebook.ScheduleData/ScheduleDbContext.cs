using Microsoft.EntityFrameworkCore;
using MyCinebook.ScheduleData.Models;

namespace MyCinebook.ScheduleData;

public class ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : DbContext(options)
{
    public virtual DbSet<ScheduledShow> ScheduledShow { get; set; }
    public virtual DbSet<ScheduledShowSeat> ScheduledShowSeat { get; set; }

}
