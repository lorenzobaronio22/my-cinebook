namespace MyCinebook.BookingData;

public class BookingModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public List<BookingShowModel> Shows { get; set; } = [];

}
