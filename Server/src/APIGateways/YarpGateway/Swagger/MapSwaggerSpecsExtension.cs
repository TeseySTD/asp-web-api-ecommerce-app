using System.Text.RegularExpressions;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Yarp.ReverseProxy.Configuration;

namespace YarpGateway.Swagger;

public static class MapSwaggerSpecsExtension
{
    private static readonly HttpClient Client = new();

    public static void MapSwaggerSpecs(
        this IEndpointRouteBuilder endpoints,
        List<RouteConfig> config,
        ClusterConfig cluster,
        GatewaySwaggerSpec swagger)
    {
        endpoints.Map(swagger.Endpoint, async context =>
        {
            var stream = await Client.GetStreamAsync(swagger.Spec);
            var document = new OpenApiStreamReader().Read(stream, out _);

            ProcessOpenApiPaths(document, config, cluster, swagger);

            var result = document.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
            await context.Response.WriteAsync(result);
        });
    }

    private static void ProcessOpenApiPaths(
        OpenApiDocument document,
        List<RouteConfig> config,
        ClusterConfig cluster,
        GatewaySwaggerSpec swagger)
    {
        var rewrite = new OpenApiPaths();

        var clusterRoutes = config.Where(p => p.ClusterId == cluster.ClusterId).ToList();
        var hasCatchAll = clusterRoutes.Any(p => p.Match.Path != null && p.Match.Path.Contains("**catch-all"));

        foreach (var path in document.Paths)
        {
            var rewritedPath = Regex.Replace(path.Key, swagger.TargetPath, swagger.OriginPath);

            if (ShouldIncludeEntirePath(rewritedPath, clusterRoutes, hasCatchAll))
            {
                rewrite.Add(rewritedPath, path.Value);
                continue;
            }

            var matchingRoutes = clusterRoutes.Where(p => p.Match.Path != null && p.Match.Path.Equals(rewritedPath))
                .ToList();

            if (matchingRoutes.Any())
            {
                FilterPathOperations(path.Value, matchingRoutes);

                // Add path if there are any operations left
                if (path.Value.Operations.Any())
                {
                    rewrite.Add(rewritedPath, path.Value);
                }
            }
        }

        document.Paths = rewrite;
    }

    private static bool ShouldIncludeEntirePath(string rewritedPath, List<RouteConfig> clusterRoutes, bool hasCatchAll)
    {
        if (hasCatchAll) return true;

        return clusterRoutes.Any(p =>
            p.Match.Path != null &&
            p.Match.Path.Equals(rewritedPath) &&
            p.Match.Methods == null);
    }

    private static void FilterPathOperations(OpenApiPathItem pathItem, List<RouteConfig> matchingRoutes)
    {
        var operationsToRemove = new List<OperationType>();

        foreach (var operation in pathItem.Operations)
        {
            var methodStr = operation.Key.ToString().ToUpperInvariant();

            var hasRoute = matchingRoutes.Any(p =>
                p.Match.Methods != null &&
                p.Match.Methods.Contains(methodStr));

            if (!hasRoute)
            {
                operationsToRemove.Add(operation.Key);
            }
        }

        foreach (var operationToRemove in operationsToRemove)
        {
            pathItem.Operations.Remove(operationToRemove);
        }
    }
}