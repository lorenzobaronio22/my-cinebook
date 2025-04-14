namespace MyCinebook.BookingData.Models;

public class Booking
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public required ICollection<BookedShow> Shows { get; set; }

}
