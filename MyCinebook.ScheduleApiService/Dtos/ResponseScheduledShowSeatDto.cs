namespace MyCinebook.ScheduleApiService.Dtos;

public class ResponseScheduledShowSeatDto
{
    public int ID { get; set; }
    public required string Line { get; set; }
    public int Number { get; set; }
}
