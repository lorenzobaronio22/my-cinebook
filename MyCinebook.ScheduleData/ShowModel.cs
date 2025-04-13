namespace MyCinebook.ScheduleData;

public class ShowModel
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public List<SeatModel> Seats { get; set; } = [];
}
