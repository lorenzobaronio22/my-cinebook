using Microsoft.EntityFrameworkCore;
using Moq;
using MyCinebook.BookingApiService;
using MyCinebook.BookingApiService.Dtos;
using MyCinebook.BookingData;
using MyCinebook.BookingData.Models;

namespace MyCinebook.Tests.Unit;

public class BookingServiceTest
{
    [Fact]
    public async Task SaveBooking_ShouldSaveBooking_WhenShowIsAvailable()
    {
        // Arrange
        var scheduledShow = new ResponseScheduledShowDto
        {
            ID = 1,
            Title = "Test Show",
            Seats = [ new ResponseScheduledShowSeatDto { Line = "A", Number = 1 } ]
        };
        var scheduleClientMock = SetupScheduleClientMock([scheduledShow]);
        BookingDbContext _dbContext = InitDBContext("SaveBooking_ShouldSaveBooking_WhenShowIsAvailable");

        // Act
        var requestBookingDto = new RequestBookingDto { ShowId = 1 };
        var result = await BookingService.SaveBooking(requestBookingDto, scheduleClientMock.Object, _dbContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Shows.First().ShowId);
        Assert.Equal("A", result.Shows.First().Seats.First().Line);
        Assert.Equal(1, result.Shows.First().Seats.First().Number);
        Assert.Equal(1, _dbContext.Booking.Count());
    }

    [Fact]
    public async Task SaveBooking_ShouldThrowBookingError_WhenShowNotFound()
    {
        // Arrange
        var scheduleClientMock = SetupScheduleClientMock([]);
        BookingDbContext _dbContext = InitDBContext("SaveBooking_ShouldThrowBookingError_WhenShowNotFound");

        // Act & Assert
        var requestBookingDto = new RequestBookingDto { ShowId = 99 };
        await Assert.ThrowsAsync<ArgumentException>(() =>
            BookingService.SaveBooking(requestBookingDto, scheduleClientMock.Object, _dbContext));
    }

    [Fact]
    public async Task SaveBooking_ShouldThrowBookingError_WhenShowIsSoldOut()
    {
        // Arrange
        var scheduledShow = new ResponseScheduledShowDto
        {
            ID = 1,
            Title = "Test Show",
            Seats = [ new ResponseScheduledShowSeatDto { Line = "A", Number = 1 } ]
        };
        var scheduleClientMock = SetupScheduleClientMock([scheduledShow]);

        var booking = new Booking { Shows = [] };
        var bookedShow = new BookedShow
        {
            ShowId = 1,
            ShowTitle = "Test Show",
            Seats = [],
            Booking = booking
        };
        booking.Shows.Add(bookedShow);
        var bookedShowSeat = new BookedShowSeat {
            Line = "A",
            Number = 1,
            BookedShow = bookedShow 
        };
        bookedShow.Seats.Add(bookedShowSeat);

        BookingDbContext _dbContext = InitDBContext("SaveBooking_ShouldThrowBookingError_WhenShowIsSoldOut");
        _dbContext.Booking.Add(booking);
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        var requestBookingDto = new RequestBookingDto { ShowId = 1 };
        await Assert.ThrowsAsync<ArgumentException>(() =>
            BookingService.SaveBooking(requestBookingDto, scheduleClientMock.Object, _dbContext));
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
                new ResponseScheduledShowSeatDto { Line = "B", Number = 1 }
            ]
        };
        var scheduleClientMock = SetupScheduleClientMock([scheduledShow]);

        var booking = new Booking { Shows = [] };
        var bookedShow = new BookedShow
        {
            ShowId = 1,
            ShowTitle = "Test Show",
            Seats = [],
            Booking = booking
        };
        booking.Shows.Add(bookedShow);
        var bookedShowSeat = new BookedShowSeat
        {
            Line = "A",
            Number = 1,
            BookedShow = bookedShow
        };
        bookedShow.Seats.Add(bookedShowSeat);

        BookingDbContext _dbContext = InitDBContext("SaveBooking_ShouldSaveBooking_WhenShowAndSeatAreAvailable");
        _dbContext.Booking.Add(booking);
        await _dbContext.SaveChangesAsync();

        // Act
        var requestBookingDto = new RequestBookingDto
        {
            ShowId = 1,
            Seat = new RequestBookingSeatDto { Line = "B", Number = 1 }
        };
        var result = await BookingService.SaveBooking(requestBookingDto, scheduleClientMock.Object, _dbContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Shows.First().ShowId);
        Assert.Equal("B", result.Shows.First().Seats.First().Line);
        Assert.Equal(1, result.Shows.First().Seats.First().Number);
        Assert.Equal(2, _dbContext.Booking.Count());
    }

    [Fact]
    public async Task DeleteBooking_ShouldDeleteBooking_WhenBookingExists()
    {
        // Arrange
        var booking = new Booking { Id = 1, CreatedAt = DateTime.UtcNow, Shows = [] };

        BookingDbContext _dbContext = InitDBContext("DeleteBooking_ShouldDeleteBooking_WhenBookingExists");
        _dbContext.Booking.Add(booking);
        await _dbContext.SaveChangesAsync();

        // Act
        await BookingService.DeleteBooking(1, _dbContext);

        // Assert
        var deletedBooking = await _dbContext.Booking.FindAsync(1);
        Assert.NotNull(deletedBooking);
        Assert.NotNull(deletedBooking.DeletedAt);
    }

    [Fact]
    public async Task DeleteBooking_ShouldThrowBookingError_WhenBookingDoesNotExist()
    {
        // Arrange
        BookingDbContext _dbContext = InitDBContext("SaveBooking_ShouldSaveBooking_WhenShowIsAvailable");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            BookingService.DeleteBooking(99, _dbContext));
    }

    [Fact]
    public async Task FindBookingByShow_ShouldReturnOnlyBookingForThatShow_WhenShowItsBeenBooked()
    {
        var booking1 = new Booking { Shows = [] };
        var bookedShow1 = new BookedShow
        {
            ShowId = 1,
            ShowTitle = "Test Show 1",
            Seats = [],
            Booking = booking1
        };
        booking1.Shows.Add(bookedShow1);
        var bookedShowSeat1 = new BookedShowSeat
        {
            Line = "A",
            Number = 1,
            BookedShow = bookedShow1
        };
        bookedShow1.Seats.Add(bookedShowSeat1);
        var bookedShow2 = new BookedShow
        {
            ShowId = 2,
            ShowTitle = "Test Show 2",
            Seats = [],
            Booking = booking1
        };
        booking1.Shows.Add(bookedShow2);
        var bookedShowSeat2 = new BookedShowSeat
        {
            Line = "A",
            Number = 1,
            BookedShow = bookedShow2
        };
        bookedShow2.Seats.Add(bookedShowSeat2);

        var booking2 = new Booking { Shows = [] };
        var bookedShow1booking2 = new BookedShow
        {
            ShowId = 1,
            ShowTitle = "Test Show 1",
            Seats = [],
            Booking = booking2
        };
        booking2.Shows.Add(bookedShow1booking2);
        var bookedShowSeat2booking2 = new BookedShowSeat
        {
            Line = "A",
            Number = 2,
            BookedShow = bookedShow1booking2
        };
        bookedShow1booking2.Seats.Add(bookedShowSeat2booking2);

        var booking3 = new Booking { Shows = [] };
        var bookedShow2booking3 = new BookedShow
        {
            ShowId = 2,
            ShowTitle = "Test Show 2",
            Seats = [],
            Booking = booking3
        };
        booking3.Shows.Add(bookedShow2booking3);
        var bookedShowSeat2booking3 = new BookedShowSeat
        {
            Line = "A",
            Number = 2,
            BookedShow = bookedShow2booking3
        };
        bookedShow2booking3.Seats.Add(bookedShowSeat2booking3);

        BookingDbContext _dbContext = InitDBContext("FindBookingByShow_ShouldReturnOnlyBookingForThatShow_WhenShowItsBeenBooked");
        _dbContext.Booking.Add(booking1);
        _dbContext.Booking.Add(booking2);
        _dbContext.Booking.Add(booking3);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await BookingService.FindBookingByShow(1, _dbContext);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, booking => booking.Id == booking1.Id);
        Assert.Contains(result, booking => booking.Id == booking2.Id);
        Assert.DoesNotContain(result, booking => booking.Id == booking3.Id);
    }

    private static BookingDbContext InitDBContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new BookingDbContext(options);
    }

    private static Mock<IScheduleClient> SetupScheduleClientMock(ICollection<ResponseScheduledShowDto> response)
    {
        var clientMock = new Mock<IScheduleClient>();
        clientMock
            .Setup(client => client.GetShowsAsync())
            .ReturnsAsync(response);
        return clientMock;
    }

}
