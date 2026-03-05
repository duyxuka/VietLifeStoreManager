using System;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace VietlifeStore.Web.HealthChecks;

public static class HealthChecksBuilderExtensions
{
    public static void AddVietlifeStoreHealthChecks(this IServiceCollection services)
    {
        var configuration = services.GetConfiguration();

        // ===== LẤY SelfUrl =====
        var appSelfUrl = configuration["App:SelfUrl"] ?? "http://localhost:8099";

        // ===== LẤY HealthCheckUrl =====
        var healthCheckPath = configuration["App:HealthCheckUrl"] ?? "/health-status";

        // Nếu cấu hình là full URL thì tách lấy path
        if (healthCheckPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(healthCheckPath);
            healthCheckPath = uri.AbsolutePath;
        }

        // ===== REGISTER HEALTH CHECK =====
        var healthChecksBuilder = services.AddHealthChecks();
        healthChecksBuilder.AddCheck<VietlifeStoreDatabaseCheck>(
            "VietlifeStore DbContext Check",
            tags: new[] { "database" }
        );

        // ===== MAP HEALTH CHECK ENDPOINT =====
        services.ConfigureHealthCheckEndpoint(healthCheckPath);

        // ===== BUILD FULL URL CHO UI =====
        var fullHealthCheckUrl = $"{appSelfUrl.TrimEnd('/')}{healthCheckPath}";

        // Nếu có cấu hình riêng cho UI thì ưu tiên
        var uiCheckUrl = configuration["App:HealthUiCheckUrl"] ?? fullHealthCheckUrl;

        var healthChecksUiBuilder = services.AddHealthChecksUI(settings =>
        {
            settings.AddHealthCheckEndpoint(
                "VietlifeStore Health Status",
                uiCheckUrl
            );
        });

        healthChecksUiBuilder.AddInMemoryStorage();

        // ===== MAP UI ENDPOINT =====
        services.MapHealthChecksUiEndpoints(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-ui-api";
        });
    }

    private static IServiceCollection ConfigureHealthCheckEndpoint(this IServiceCollection services, string path)
    {
        services.Configure<AbpEndpointRouterOptions>(options =>
        {
            options.EndpointConfigureActions.Add(endpointContext =>
            {
                endpointContext.Endpoints.MapHealthChecks(
                    new PathString(path.EnsureStartsWith('/')),
                    new HealthCheckOptions
                    {
                        Predicate = _ => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                        AllowCachingResponses = false,
                    });
            });
        });

        return services;
    }

    private static IServiceCollection MapHealthChecksUiEndpoints(
        this IServiceCollection services,
        Action<global::HealthChecks.UI.Configuration.Options>? setupOption = null)
    {
        services.Configure<AbpEndpointRouterOptions>(routerOptions =>
        {
            routerOptions.EndpointConfigureActions.Add(endpointContext =>
            {
                endpointContext.Endpoints.MapHealthChecksUI(setupOption);
            });
        });

        return services;
    }
}