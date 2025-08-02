using Basket.API.Application;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Validation.Result;

namespace Basket.Tests.Integration.Common;

public class ApiTest : BaseIntegrationTest, IClassFixture<IntegrationTestWebApplicationFactory>, IAsyncLifetime
{
    protected HttpClient HttpClient { get; private set; }
    protected IDocumentSession Session { get; private set; }

    private DatabaseFixture _databaseFixture;
    private IServiceScope _scope;

    public ApiTest(DatabaseFixture databaseFixture, IntegrationTestWebApplicationFactory factory)
    {
        _databaseFixture = databaseFixture;
        HttpClient = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        Session = _scope.ServiceProvider.GetRequiredService<IDocumentSession>();
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