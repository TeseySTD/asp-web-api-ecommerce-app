using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Persistence;
using Shared.Core.Validation.Result;

namespace Ordering.Tests.Integration.Common;

public class ApiTest : BaseIntegrationTest, IClassFixture<IntegrationTestWebApplicationFactory>, IAsyncLifetime
{
    protected ISender Sender { get; init; }
    protected HttpClient HttpClient { get; init; }
    protected ApplicationDbContext ApplicationDbContext { get; init; }
    
    private DatabaseFixture _databaseFixture;
    private IServiceScope _scope;

    protected ApiTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture)
    {
        HttpClient = factory.CreateClient();

        _scope = factory.Services.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        ApplicationDbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _databaseFixture = databaseFixture;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _databaseFixture.ResetAsync();
        _scope.Dispose();
    }

    public string MakeSystemErrorApiOutput(string message, string description)
    {
        var result =
            """
            {
              "errors":[
                {
                  "systemErrors":[
                    {
                      "message":"
            """ + message + "\"," +
            """
                      "description":"
            """ + description + "\"" +
            """
                    }
                  ]
                }
              ]
            }
            """;
        return result.Replace("\n", "").Replace("\r", "").Trim();
    }

    public string MakeSystemErrorApiOutput(Error error) => MakeSystemErrorApiOutput(error.Message, error.Description);
    
    public string MakePropertyErrorApiOutput(string propertyName, IEnumerable<Error> errors)
    {
        var stringErrors = errors
            .Aggregate("",
                (current, error) => current +
                                    "{" +
                                    $"""
                                     "message":"{propertyName}",
                                     "description":"{error.Message}:{error.Description}"
                                     """ +
                                    "},");
        // Remove last coma
        stringErrors = stringErrors.Remove(stringErrors.Length - 1);

        var result =
            """
            {
              "errors":[
                {
                  "propertyName": "
                  
            """ + propertyName + "\"," +
            """
                      "propertyErrors":[
            """ + stringErrors +
            """
                    
                  ]
                }
              ]
            }
            """;
        return result.Replace("\n", "").Replace("\r", "").Trim();
    }
}