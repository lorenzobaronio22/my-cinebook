namespace MyCinebook.BookingApiService.Exceptions;

public class BookingError : Exception
{
    public BookingError(string message) : base(message) { }
}
