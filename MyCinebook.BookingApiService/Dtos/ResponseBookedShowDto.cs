namespace MyCinebook.BookingApiService.Dtos;

public class ResponseBookedShowDto
{
    public int ShowId { get; set; }
    public required string ShowTitle { get; set; }
    public required ICollection<ResponseBookedShowSeatDto> Seats { get; set; }
}