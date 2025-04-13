namespace MyCinebook.BookingApiService;

public class ShowDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public List<SeatDto> Seats { get; set; } = [];
}
