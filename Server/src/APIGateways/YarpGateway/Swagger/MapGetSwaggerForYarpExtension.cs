using Yarp.ReverseProxy.Configuration;

namespace YarpGateway.Swagger;

public static class MapGetSwaggerForYarpExtension
{
    public static void MapGetSwaggerForYarp(this IEndpointRouteBuilder endpoints,IConfiguration configuration)
    {
        var clusters = configuration.GetSection("ReverseProxy:Clusters");
        var routes = configuration.GetSection("ReverseProxy:Routes").Get<List<RouteConfig>>();

        if (clusters.Exists())
        {
            foreach (var child in clusters.GetChildren())
            {
                if (child.GetSection("Swagger").Exists())
                {
                    var cluster = child.Get<ClusterConfig>();
                    var swagger = child.GetSection("Swagger").Get<GatewaySwaggerSpec>();
                    // Map swagger endpoint if we find a cluster with swagger configuration
                    endpoints.MapSwaggerSpecs(routes!, cluster!, swagger!);
                }
            }
        }
    }
}