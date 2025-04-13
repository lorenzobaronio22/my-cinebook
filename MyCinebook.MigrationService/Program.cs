using MyCinebook.MigrationService;
using MyCinebook.ScheduleApiService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<ScheduleDbContext>(connectionName: "schedule");

builder.Services.AddHostedService<Worker>();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));


var host = builder.Build();
host.Run();
