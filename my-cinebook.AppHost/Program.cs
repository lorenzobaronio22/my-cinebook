using MyCinebook.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.MyCinebook_ApiService>("apiservice")
    .WithHttpsHealthCheck("/health")
    .WithScalar();

builder.Build().Run();
