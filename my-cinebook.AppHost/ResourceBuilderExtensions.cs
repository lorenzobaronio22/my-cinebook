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
            name,
            displayName,
            async _ =>
            {
                try
                {
                    var endpoint = builder.GetEndpoint("https");
                    var url = $"{endpoint.Url}/scalar/v1";
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    ;
                    return new ExecuteCommandResult { Success = true };
                }
                catch (Exception ex)
                {
                    return new ExecuteCommandResult { Success = false, ErrorMessage = ex.Message };
                }
            },
            new CommandOptions
            {
                UpdateState = context => context.ResourceSnapshot.HealthStatus == HealthStatus.Healthy ? ResourceCommandState.Enabled : ResourceCommandState.Disabled,
                IconName = "Bookmark",
                IconVariant = IconVariant.Filled
            }
        );
    }
}
