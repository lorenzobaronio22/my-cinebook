namespace MyCinebook.ScheduleData.Models;

public class ScheduledShowSeat
{
    public int ID { get; set; }
    public required string Line { get; set; }
    public int Number { get; set; }
    public required ScheduledShow ScheduledShow { get; set; }
}
