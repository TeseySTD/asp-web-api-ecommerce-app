using System.Diagnostics;
using System.Text.Json;
using EcommerceProject.Core.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EcommerceProject.Application.Common.Classes.Behaviour;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
    public const int MaximalPerformanceTimeInMs = 2000;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var responseName = typeof(TResponse).Name;

        _logger.LogInformation("[START] Handle request:{Request} - Response type:{Response}",
            requestName, responseName);

        TResponse response;

        var timer = new Stopwatch();
        timer.Start();

        try
        {
            _logger.LogInformation(
                $"[PROPS] {JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true })}");
        }
        catch (Exception)
        {
            _logger.LogInformation(
                $"[Serialization props ERROR] {typeof(TRequest).Name} Could not serialize the request.");
        }

        response = await next();

        timer.Stop();

        var timeTaken = timer.Elapsed;
        if (timeTaken.Milliseconds > MaximalPerformanceTimeInMs)
        {
            _logger.LogWarning("[PERFORMANCE] The request {Request} took {TimeTaken} ms.",
                requestName, timeTaken.Milliseconds);
        }

        if (response is Result)
        {
            var res = response as Result;
            if (res!.IsFailure)
                _logger.LogWarning("[ERROR] Handler returned Result object with errors: {Errors}",
                    JsonSerializer.Serialize(res.Errors, new JsonSerializerOptions { WriteIndented = true }));
        }

        _logger.LogInformation("[END] Handler {Request} with response type: {Response}; Excecution time: {Time}ms.",
            requestName, responseName, timeTaken.Milliseconds);


        return response;
    }
}