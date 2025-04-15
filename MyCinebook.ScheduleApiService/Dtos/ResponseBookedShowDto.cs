namespace MyCinebook.ScheduleApiService.Dtos;

public class ResponseBookedShowDto
{
    public int ShowId { get; set; }
    public required string ShowTitle { get; set; }
    public required ResponseBookedShowSeatDto Seat { get; set; }
}
