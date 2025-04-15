using MyCinebook.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var postgresServer = builder.AddPostgres("postgreSQLServer")
    //.WithPgAdmin()
;

var scheduleDatabase = postgresServer.AddDatabase("schedule");
var bookingDatabase = postgresServer.AddDatabase("booking");

builder.AddProject<Projects.MyCinebook_MigrationService>("migrationservice")
    .WithReference(postgresServer)
    .WithReference(scheduleDatabase)
    .WithReference(bookingDatabase)
    .WaitFor(postgresServer)
    .WaitFor(scheduleDatabase)
    .WaitFor(bookingDatabase);

var scheduleApiService = builder.AddProject<Projects.MyCinebook_ScheduleApiService>("scheduleapiservice")
    .WithHttpsHealthCheck("/health")
    .WithScalar();

var bookingApiService = builder.AddProject<Projects.MyCinebook_BookingApiService>("bookapiservice")
    .WithHttpsHealthCheck("/health")
    .WithScalar();

scheduleApiService
    .WithReference(scheduleDatabase)
    .WithReference(bookingApiService);

bookingApiService
    .WithReference(bookingDatabase)
    .WithReference(scheduleApiService);


builder.Build().Run();
