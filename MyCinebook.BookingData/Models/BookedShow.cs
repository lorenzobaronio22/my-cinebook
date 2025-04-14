using System.ComponentModel.DataAnnotations;

namespace MyCinebook.BookingData.Models;

public class BookedShow
{
    public int ID { get; set; }
    public int ShowId { get; set; }
    [StringLength(250)]
    public required string ShowTitle { get; set; }
    public required Booking Booking { get; set; }
    public required ICollection<BookedShowSeat> Seats { get; set; }
}
