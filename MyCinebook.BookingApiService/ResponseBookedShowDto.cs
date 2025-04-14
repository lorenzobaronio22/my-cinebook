using System.ComponentModel.DataAnnotations;
using MyCinebook.BookingData.Models;

namespace MyCinebook.BookingApiService;

public class ResponseBookedShowDto
{
    public int ID { get; set; }
    public int ShowId { get; set; }
    public required string ShowTitle { get; set; }
    public required ICollection<ResponseBookedShowSeatDto> Seats { get; set; }
}