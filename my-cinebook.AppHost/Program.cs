var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.my_cinebook_ApiService>("apiservice")
    .WithHttpsHealthCheck("/health")
    .WithExternalHttpEndpoints();

builder.Build().Run();
