using Microsoft.EntityFrameworkCore;
using Moq;
using MyCinebook.ScheduleApiService;
using MyCinebook.ScheduleApiService.Dtos;
using MyCinebook.ScheduleData;
using MyCinebook.ScheduleData.Models;

namespace MyCinebook.Tests.Unit;

public class ScheduleServiceTest
{
    public static ScheduleDbContext InitScheduleDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName);

        return new ScheduleDbContext(options.Options);
    }

    [Fact]
    public async Task ListShows_ShouldListScheduledShows_WhenThereAreShowsInTheSchedule()
    {
        // Arrange
        var scheduledShows = new List<ScheduledShow>
        {
            new() { ID = 1, Title = "Test Show 1", Seats = [] },
            new() { ID = 2, Title = "Test Show 2", Seats = [] },
        };
        var seatA1AShow1 = new ScheduledShowSeat { ID = 1, Line = "A", Number = 1, ScheduledShow = scheduledShows[0] };
        scheduledShows[0].Seats.Add(seatA1AShow1);
        var seatA1AShow2 = new ScheduledShowSeat { ID = 2, Line = "A", Number = 1, ScheduledShow = scheduledShows[1] };
        scheduledShows[1].Seats.Add(seatA1AShow2);
        var seatA2AShow2 = new ScheduledShowSeat { ID = 3, Line = "A", Number = 2, ScheduledShow = scheduledShows[1] };
        scheduledShows[1].Seats.Add(seatA2AShow2);

        var _dbContext = InitScheduleDbContext("ListShows_ShouldListScheduledShows_WhenThereAreShowsInTheSchedule");
        _dbContext.ScheduledShow.AddRange(scheduledShows);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await ScheduleService.ListScheduledShows(_dbContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        // First show
        var firstShow = result.First();
        Assert.Equal(1, firstShow.ID);
        Assert.Equal("Test Show 1", firstShow.Title);
        Assert.NotNull(firstShow.Seats);
        Assert.Single(firstShow.Seats);

        var firstShowSeat = firstShow.Seats.First();
        Assert.Equal(1, firstShowSeat.ID);
        Assert.Equal("A", firstShowSeat.Line);
        Assert.Equal(1, firstShowSeat.Number);

        // Second show
        var secondShow = result.Last();
        Assert.Equal(2, secondShow.ID);
        Assert.Equal("Test Show 2", secondShow.Title);
        Assert.NotNull(secondShow.Seats);
        Assert.Equal(2, secondShow.Seats.Count);

        var secondShowSeat1 = secondShow.Seats.First();
        Assert.Equal(2, secondShowSeat1.ID);
        Assert.Equal("A", secondShowSeat1.Line);
        Assert.Equal(1, secondShowSeat1.Number);

        var secondShowSeat2 = secondShow.Seats.Last();
        Assert.Equal(3, secondShowSeat2.ID);
        Assert.Equal("A", secondShowSeat2.Line);
        Assert.Equal(2, secondShowSeat2.Number);
    }

    [Fact]
    public async Task FindShowAvailability_ShouldListShowSeatsWithAvailabilityStatus_WhenShowIsInSchedule()
    {
        // Arrange
        var scheduledShow = new ScheduledShow() { ID = 1, Title = "Test Show 1", Seats = [] };
        var seatA1 = new ScheduledShowSeat { ID = 1, Line = "A", Number = 1, ScheduledShow = scheduledShow };
        scheduledShow.Seats.Add(seatA1);
        var seatA2 = new ScheduledShowSeat { ID = 2, Line = "A", Number = 2, ScheduledShow = scheduledShow };
        scheduledShow.Seats.Add(seatA2);

        var _dbContext = InitScheduleDbContext("FindShowAvailability_ShouldListShowSeatsWithAvailabilityStatus_WhenShowIsInSchedule");
        _dbContext.ScheduledShow.AddRange(scheduledShow);
        await _dbContext.SaveChangesAsync();

        var booking = new ResponseBookingDto
        {
            Shows = [
                new ResponseBookedShowDto {
                    ShowId = 1,
                    ShowTitle = "Test Show 1",
                    Seat = new ResponseBookedShowSeatDto { Line = "A", Number = 1 }
                }
            ]
        };
        var _mockCLient = SetupBookingClientMock([booking]);
        // Act

        var result = await ScheduleService.FindShowAvailability(1, _dbContext, _mockCLient.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.False(result.First().Available);
        Assert.True(result.Last().Available);

    }

    private static Mock<IBookingClient> SetupBookingClientMock(ICollection<ResponseBookingDto> response)
    {
        var clientMock = new Mock<IBookingClient>();
        clientMock
            .Setup(client => client.GetBookingsFilteredByShowAsync(It.IsAny<int>()))
            .ReturnsAsync(response);
        return clientMock;
    }
}
