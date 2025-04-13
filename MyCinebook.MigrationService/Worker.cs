using System.Diagnostics;
using MyCinebook.ScheduleData;
using Microsoft.EntityFrameworkCore;
using MyCinebook.BookingData;

namespace MyCinebook.MigrationService;

public class Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
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
            // Run migration in a transaction to avoid partial migration if it fails.
            await dbContext.Database.MigrateAsync(cancellationToken);
        });

    }

    private static async Task SeedDataAsync(ScheduleDbContext dbContext, CancellationToken cancellationToken)
    {
        // Check if there are any existing shows in the database
        if (!await dbContext.Shows.AnyAsync(cancellationToken))
        {
            // Create example data for shows
            var shows = new List<ScheduleShowModel>
            {
                new() {
                    Title = "The Matrix",
                    Seats =
                    [
                        new ScheduleSeatModel { Line = 'A', Number = 1 },
                        new ScheduleSeatModel { Line = 'A', Number = 2 },
                        new ScheduleSeatModel { Line = 'A', Number = 3 },
                        new ScheduleSeatModel { Line = 'B', Number = 1 },
                        new ScheduleSeatModel { Line = 'B', Number = 2 },
                        new ScheduleSeatModel { Line = 'B', Number = 3 }
                    ]
                },
                new() {
                    Title = "Inception",
                    Seats =
                    [
                        new ScheduleSeatModel { Line = 'A', Number = 1 },
                        new ScheduleSeatModel { Line = 'A', Number = 2 },
                        new ScheduleSeatModel { Line = 'A', Number = 3 },
                        new ScheduleSeatModel { Line = 'B', Number = 1 },
                        new ScheduleSeatModel { Line = 'B', Number = 2 },
                        new ScheduleSeatModel { Line = 'B', Number = 3 }
                    ]
                }
            };

            // Add the shows to the database
            await dbContext.Shows.AddRangeAsync(shows, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
    private async Task RunMigrationAsync(BookingDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Run migration in a transaction to avoid partial migration if it fails.
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }
}
