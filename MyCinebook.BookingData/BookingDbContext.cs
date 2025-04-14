using Microsoft.EntityFrameworkCore;
using MyCinebook.BookingData.Models;

namespace MyCinebook.BookingData;

public class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public virtual DbSet<Booking> Booking { get; set; }
    public DbSet<BookedShow> BookedShow { get; set; }
    public DbSet<BookedShowSeat> BookedShowSeat { get; set; }
}
