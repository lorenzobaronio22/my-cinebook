namespace MyCinebook.BookingApiService;

public class BookingDto
{
    public int ShowId { get; set; }

    public bool isValid()
    {
        return ShowId > 0;
    }
}