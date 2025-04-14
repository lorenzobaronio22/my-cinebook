namespace MyCinebook.BookingApiService.Dtos;

public class RequestBookingDto
{
    public int ShowId { get; set; }

    public RequestBookingSeatDto? Seat { get; set; }

}