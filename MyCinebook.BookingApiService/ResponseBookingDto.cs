using MyCinebook.BookingData.Models;

namespace MyCinebook.BookingApiService;

public class ResponseBookingDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public required ICollection<ResponseBookedShowDto> Shows { get; set; }

}
