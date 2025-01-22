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

            var document = new OpenApiStreamReader().Read(stream, out var diagnostic);
            var rewrite = new OpenApiPaths();

            // map existing path
            var routes = config.Where(p => p.ClusterId == cluster.ClusterId);
            var hasCatchAll = routes != null && routes.Any(p => p.Match.Path.Contains("**catch-all"));

            foreach (var path in document.Paths)
            {
                var rewritedPath = Regex.Replace(path.Key, swagger.TargetPath, swagger.OriginPath);

                // there is a catch all, all route are elligible 
                // or there is a route that match without any operation methods filtering
                if (hasCatchAll || routes.Any(p => p.Match.Path.Equals(rewritedPath) && p.Match.Methods == null))
                {
                    rewrite.Add(rewritedPath, path.Value);
                }
                else
                {
                    // there is a route that match
                    var routeThatMatchPath = routes.Any(p => p.Match.Path.Equals(rewritedPath));
                    if (routeThatMatchPath)
                    {
                        // filter operation based on yarp config
                        var operationToRemoves = new List<OperationType>();
                        foreach (var operation in path.Value.Operations)
                        {
                            // match route and method
                            var hasRoute = routes.Any(
                                p => p.Match.Path.Equals(rewritedPath) &&
                                     p.Match.Methods.Contains(operation.Key.ToString().ToUpperInvariant())
                            );

                            if (!hasRoute)
                            {
                                operationToRemoves.Add(operation.Key);
                            }
                        }

                        // remove operation routes
                        foreach (var operationToRemove in operationToRemoves)
                        {
                            path.Value.Operations.Remove(operationToRemove);
                        }

                        // add path if there is any operation
                        if (path.Value.Operations.Any())
                        {
                            rewrite.Add(rewritedPath, path.Value);
                        }
                    }
                }
            }

            document.Paths = rewrite;

            var result = document.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);
            await context.Response.WriteAsync(
                result
            );
        });
    }
}