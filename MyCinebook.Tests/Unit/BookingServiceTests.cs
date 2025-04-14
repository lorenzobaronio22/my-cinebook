using Microsoft.EntityFrameworkCore;
using Moq;
using MyCinebook.BookingApiService;
using MyCinebook.BookingApiService.Dtos;
using MyCinebook.BookingApiService.Exceptions;
using MyCinebook.BookingData;
using MyCinebook.BookingData.Models;

namespace MyCinebook.Tests.Unit;

public class BookingServiceTests
{
    private readonly Mock<IScheduleClient> _mockScheduleClient;
    private readonly Mock<DbSet<Booking>> _mockBookingDbSet;
    private readonly Mock<BookingDbContext> _mockDbContext;

    public BookingServiceTests()
    {
        _mockScheduleClient = new Mock<IScheduleClient>();
        _mockBookingDbSet = new Mock<DbSet<Booking>>();
        _mockDbContext = new Mock<BookingDbContext>(new DbContextOptions<BookingDbContext>());
    }

    [Fact]
    public async Task SaveBooking_ShouldSaveBooking_WhenShowIsAvailable()
    {
        // Arrange
        var scheduledShow = new ResponseScheduledShowDto
        {
            ID = 1,
            Title = "Test Show",
            Seats =
            [
                new ResponseScheduledShowSeatDto { Line = "A", Number = 1 }
            ]
        };

        _mockScheduleClient
            .Setup(client => client.GetShowsAsync())
            .ReturnsAsync([scheduledShow]);

        var data = new List<Booking>().AsQueryable();

        SetupMockBookingDbSet(data);

        // Act
        var requestBookingDto = new RequestBookingDto { ShowId = 1 };
        var result = await BookingService.SaveBooking(requestBookingDto, _mockScheduleClient.Object, _mockDbContext.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Shows.First().ShowId);
        Assert.Equal("A", result.Shows.First().Seats.First().Line);
        Assert.Equal(1, result.Shows.First().Seats.First().Number);
        _mockDbContext.Verify(db => db.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task SaveBooking_ShouldThrowBookingError_WhenShowNotFound()
    {
        // Arrange
        _mockScheduleClient
            .Setup(client => client.GetShowsAsync())
            .ReturnsAsync([]);

        _mockDbContext.Setup(db => db.Booking).Returns(_mockBookingDbSet.Object);

        // Act & Assert
        var requestBookingDto = new RequestBookingDto { ShowId = 99 };
        await Assert.ThrowsAsync<BookingError>(() =>
            BookingService.SaveBooking(requestBookingDto, _mockScheduleClient.Object, _mockDbContext.Object));
    }

    [Fact]
    public async Task SaveBooking_ShouldThrowBookingError_WhenShowIsSoldOut()
    {
        // Arrange
        var requestBookingDto = new RequestBookingDto { ShowId = 1 };
        var scheduledShow = new ResponseScheduledShowDto
        {
            ID = 1,
            Title = "Test Show",
            Seats =
            [
                new ResponseScheduledShowSeatDto { Line = "A", Number = 1 }
            ]
        };

        _mockScheduleClient
            .Setup(client => client.GetShowsAsync())
            .ReturnsAsync([scheduledShow]);

        var booking = new Booking { Shows = [] };
        var bookedShow = new BookedShow
        {
            ShowId = 1,
            ShowTitle = "Test Show",
            Booking = booking,
            Seats = []
        };
        var bookedShowSeat = new BookedShowSeat { Line = "A", Number = 1, BookedShow = bookedShow };
        bookedShow.Seats.Add(bookedShowSeat);
        booking.Shows.Add(bookedShow);

        var data = new List<Booking> { booking }.AsQueryable();

        SetupMockBookingDbSet(data);

        // Act & Assert
        await Assert.ThrowsAsync<BookingError>(() =>
            BookingService.SaveBooking(requestBookingDto, _mockScheduleClient.Object, _mockDbContext.Object));
    }

    [Fact]
    public async Task SaveBooking_ShouldSaveBooking_WhenShowAndSeatAreAvailable()
    {
        // Arrange
        var scheduledShow = new ResponseScheduledShowDto
        {
            ID = 1,
            Title = "Test Show",
            Seats =
            [
                new ResponseScheduledShowSeatDto { Line = "A", Number = 1 },
                new ResponseScheduledShowSeatDto { Line = "A", Number = 2 },
                new ResponseScheduledShowSeatDto { Line = "B", Number = 1 },
            ]
        };

        _mockScheduleClient
            .Setup(client => client.GetShowsAsync())
            .ReturnsAsync([scheduledShow]);

        var booking = new Booking { Shows = [] };
        var bookedShow = new BookedShow
        {
            ShowId = 1,
            ShowTitle = "Test Show",
            Booking = booking,
            Seats = []
        };
        var bookedShowSeat = new BookedShowSeat { Line = "A", Number = 1, BookedShow = bookedShow };
        bookedShow.Seats.Add(bookedShowSeat);
        booking.Shows.Add(bookedShow);

        var data = new List<Booking> { booking }.AsQueryable();

        SetupMockBookingDbSet(data);

        // Act
        var requestBookingDto = new RequestBookingDto
        {
            ShowId = 1,
            Seat = new RequestBookingSeatDto { Line = "B", Number = 1 }
        };
        var result = await BookingService.SaveBooking(requestBookingDto, _mockScheduleClient.Object, _mockDbContext.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Shows.First().ShowId);
        Assert.Equal("B", result.Shows.First().Seats.First().Line);
        Assert.Equal(1, result.Shows.First().Seats.First().Number);
        _mockDbContext.Verify(db => db.SaveChanges(), Times.Once);
    }

    private void SetupMockBookingDbSet(IQueryable<Booking> data)
    {
        _mockBookingDbSet.As<IQueryable<Booking>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockBookingDbSet.As<IQueryable<Booking>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockBookingDbSet.As<IQueryable<Booking>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockBookingDbSet.As<IQueryable<Booking>>().Setup(m => m.GetEnumerator()).Returns(() => data.GetEnumerator());
        _mockDbContext.Setup(db => db.Booking).Returns(_mockBookingDbSet.Object);
    }
}
