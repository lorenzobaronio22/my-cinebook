using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MyCinebook.AppHost;

internal static class ResourceBuilderExtensions
{
    internal static IResourceBuilder<T> WithScalar<T>(this IResourceBuilder<T> builder) where T : IResourceWithEndpoints
    {
        var name = "scalar";
        var displayName = "Scalar";
        return builder.WithCommand(
            name: name,
            displayName: displayName,
            executeCommand: _ =>
            {
                try
                {
                    var endpoint = builder.GetEndpoint("https");
                    var url = $"{endpoint.Url}/scalar/v1";
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    ;
                    return Task.FromResult(new ExecuteCommandResult { Success = true });
                }
                catch (Exception ex)
                {
                    return Task.FromResult(new ExecuteCommandResult { Success = false, ErrorMessage = ex.Message });
                }
            },
            commandOptions: new CommandOptions
            {
                UpdateState = context => context.ResourceSnapshot.HealthStatus == HealthStatus.Healthy ? ResourceCommandState.Enabled : ResourceCommandState.Disabled,
                IconName = "Bookmark",
                IconVariant = IconVariant.Filled
            }
        );
    }
}
