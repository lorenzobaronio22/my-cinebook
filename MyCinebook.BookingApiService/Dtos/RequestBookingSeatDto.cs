namespace MyCinebook.BookingApiService.Dtos;

public class RequestBookingSeatDto
{
    public required string Line {  get; set; }
    public int Number {  get; set; }
}