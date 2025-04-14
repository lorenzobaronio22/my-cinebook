namespace MyCinebook.BookingApiService;

public class RequestBookingDto
{
    public int ShowId { get; set; }

    public bool IsValid()
    {
        return ShowId > 0;
    }
}