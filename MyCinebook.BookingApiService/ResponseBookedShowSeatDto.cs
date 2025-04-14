namespace MyCinebook.BookingApiService;

public class ResponseBookedShowSeatDto
{
    public int ID { get; set; }
    public required string Line { get; set; }
    public int Number { get; set; }
}