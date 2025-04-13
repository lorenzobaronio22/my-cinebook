namespace MyCinebook.ScheduleData;

public class ScheduleShowModel
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public List<ScheduleSeatModel> Seats { get; set; } = [];
}
