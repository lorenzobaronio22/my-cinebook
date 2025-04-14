namespace MyCinebook.BookingApiService.Dtos;

public class ResponseScheduledShowDto
{
    public int ID { get; set; }
    public required string Title { get; set; }
    public List<ResponseScheduledShowSeatDto> Seats { get; set; } = [];
}
