using MyCinebook.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var postgresServer = builder.AddPostgres("postgreSQLServer")
    .WithPgAdmin();

var scheduleDatabase = postgresServer.AddDatabase("schedule");

builder.AddProject<Projects.MyCinebook_ScheduleApiService>("scheduleapiservice")
    .WithHttpsHealthCheck("/health")
    .WithScalar()
    .WithReference(scheduleDatabase);

builder.AddProject<Projects.MyCinebook_MigrationService>("migrationservice")
    .WithReference(postgresServer)
    .WithReference(scheduleDatabase)
    .WaitFor(postgresServer);

builder.AddProject<Projects.MyCinebook_BookingApiService>("bookapiservice")
    .WithHttpsHealthCheck("/health")
    .WithScalar();

builder.Build().Run();
