namespace MyCinebook.BookingApiService.Dtos;

public class RequestBookingDto
{
    public int ShowId { get; set; }

    public bool IsValid()
    {
        return ShowId > 0;
    }
}