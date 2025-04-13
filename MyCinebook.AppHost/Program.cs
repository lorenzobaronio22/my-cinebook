using MyCinebook.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var postgresServer = builder.AddPostgres("postgreSQLServer")
    .WithPgAdmin();

var scheduleDatabase = postgresServer.AddDatabase("schedule");
var bookingDatabase = postgresServer.AddDatabase("booking");

builder.AddProject<Projects.MyCinebook_ScheduleApiService>("scheduleapiservice")
    .WithHttpsHealthCheck("/health")
    .WithScalar()
    .WithReference(scheduleDatabase);

builder.AddProject<Projects.MyCinebook_MigrationService>("migrationservice")
    .WithReference(postgresServer)
    .WithReference(scheduleDatabase)
    .WithReference(bookingDatabase)
    .WaitFor(postgresServer);

builder.AddProject<Projects.MyCinebook_BookingApiService>("bookapiservice")
    .WithHttpsHealthCheck("/health")
    .WithScalar()
    .WithReference(bookingDatabase);

builder.Build().Run();
