using System.ComponentModel.DataAnnotations;

namespace MyCinebook.ScheduleData.Models;

public class ScheduledShow
{
    public int ID { get; set; }
    [StringLength(250)]
    public required string Title { get; set; }
    public ICollection<ScheduledShowSeat> Seats { get; set; } = [];
}
