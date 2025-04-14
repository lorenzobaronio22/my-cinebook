using System.Diagnostics;
using MyCinebook.ScheduleData;
using Microsoft.EntityFrameworkCore;
using MyCinebook.BookingData;
using MyCinebook.ScheduleData.Models;
using MyCinebook.BookingData.Models;

namespace MyCinebook.MigrationService;

public class Worker(IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    private readonly IServiceProvider serviceProvider = serviceProvider;
    private readonly IHostApplicationLifetime hostApplicationLifetime = hostApplicationLifetime;

    public const string ActivitySourceName = "Migrations Schedule";
    private static readonly ActivitySource s_activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = s_activitySource.StartActivity("Migrating database", ActivityKind.Client);
        try
        {
            using var scope = serviceProvider.CreateScope();

            var scheduleDbContext = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
            await RunMigrationAsync(scheduleDbContext, cancellationToken);
            await SeedDataAsync(scheduleDbContext, cancellationToken);

            var bookingDbContext = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
            await RunMigrationAsync(bookingDbContext, cancellationToken);
            await SeedDataAsync(bookingDbContext, cancellationToken);

        }
        catch (Exception ex)
        {
            activity?.AddEvent(new ActivityEvent("Exception", default, new ActivityTagsCollection
           {
               { "exception.type", ex.GetType().FullName },
               { "exception.message", ex.Message },
               { "exception.stacktrace", ex.StackTrace }
           }));
            throw;
        }

        hostApplicationLifetime.StopApplication();
    }

    private static async Task RunMigrationAsync(ScheduleDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        });

    }

    private static async Task SeedDataAsync(ScheduleDbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await dbContext.ScheduledShow.AnyAsync(cancellationToken))
        {
            var show1 = new ScheduledShow { Title = "The Matrix", Seats = [] };
            show1.Seats =
            [
                new ScheduledShowSeat { Line = "A", Number = 1, ScheduledShow = show1 },
                new ScheduledShowSeat { Line = "A", Number = 2, ScheduledShow = show1 },
                new ScheduledShowSeat { Line = "A", Number = 3, ScheduledShow = show1 },
                new ScheduledShowSeat { Line = "B", Number = 1, ScheduledShow = show1 },
                new ScheduledShowSeat { Line = "B", Number = 2, ScheduledShow = show1 },
                new ScheduledShowSeat { Line = "B", Number = 3, ScheduledShow = show1 }
            ];

            var show2 = new ScheduledShow { Title = "Private show", Seats = [] };
            show2.Seats.Add(new ScheduledShowSeat { Line = "A", Number = 1, ScheduledShow = show2 });

            var shows = new List<ScheduledShow> { show1, show2 };

            await dbContext.ScheduledShow.AddRangeAsync(shows, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task RunMigrationAsync(BookingDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    private static async Task SeedDataAsync(BookingDbContext dbContext, CancellationToken cancellationToken)
    {
        if (!await dbContext.Booking.AnyAsync(cancellationToken))
        {
            var booking = new Booking { CreatedAt = DateTime.UtcNow, Shows = [] };

            var bookedShow = new BookedShow { ShowId = 1, ShowTitle = "The Matrix", Booking = booking, Seats = [] };
            booking.Shows.Add(bookedShow);
            var bookedShowSeat = new BookedShowSeat { Line = "A", Number = 1, BookedShow = bookedShow };
            bookedShow.Seats.Add(bookedShowSeat);

            var soldOutShow = new BookedShow { ShowId = 2, ShowTitle = "Private show", Booking = booking, Seats = [] };
            booking.Shows.Add(soldOutShow);
            var soldOutShowSeat = new BookedShowSeat { Line = "A", Number = 1, BookedShow = soldOutShow };
            soldOutShow.Seats.Add(soldOutShowSeat);

            await dbContext.Booking.AddAsync(booking, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
