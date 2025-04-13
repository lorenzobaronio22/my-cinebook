using Microsoft.EntityFrameworkCore;

namespace MyCinebook.BookingData;

public class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public DbSet<BookingModel> BookingModel { get; set; }
}
