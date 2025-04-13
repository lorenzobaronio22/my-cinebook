namespace MyCinebook.BookingData;

public class BookingShowModel
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public List<BookingSeatModel> Seats { get; set; } = [];
}
