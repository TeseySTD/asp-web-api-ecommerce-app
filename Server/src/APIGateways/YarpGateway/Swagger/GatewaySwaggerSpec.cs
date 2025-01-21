namespace YarpGateway.Swagger;

public class GatewaySwaggerSpec
{
    public string Endpoint { get; set; } = null!;
    public string Spec { get; set; } = null!;
    public string OriginPath { get; set; } = null!;
    public string TargetPath { get; set; } = null!;
}