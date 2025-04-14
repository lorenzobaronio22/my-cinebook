namespace MyCinebook.BookingData.Models;

public class BookedShowSeat
{
    public int ID { get; set; }
    public required string Line { get; set; }
    public int Number { get; set; }
    public required BookedShow BookedShow { get; set; }
}
