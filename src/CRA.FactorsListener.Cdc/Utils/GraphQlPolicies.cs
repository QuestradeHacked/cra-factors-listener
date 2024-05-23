using System;
using System.Linq;
using System.Threading.Tasks;
using CRA.FactorsListener.Cdc.Configs;
using GraphQL;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace CRA.FactorsListener.Cdc.Utils;

public abstract class GraphQlPolicies<T> where T : class, IGraphQLResponse, new()
{
    public static IAsyncPolicy<T> GetAllResiliencePolicies(ILogger logger, Resilience resilience)
    {
        return Policy.WrapAsync(
            GetRetryPolicy(logger, resilience),
            GetFallbackPolicy(),
            GetCircuitBreakerPolicy(logger, resilience));
    }

    private static IAsyncPolicy<T> GetCircuitBreakerPolicy(ILogger logger, Resilience resilience)
    {
        return Policy<T>
            .Handle<Exception>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: resilience.ConsecutiveExceptionsAllowedBeforeBreaking,
                durationOfBreak: TimeSpan.FromSeconds(resilience.DurationOfBreakInSeconds),
                onBreak: (exception, breakDelay) =>
                    logger.LogError(exception.Exception,
                        "GraphQl integration will break in the next {totalSeconds} seconds.",
                        breakDelay.TotalSeconds),
                onReset: () => logger.LogInformation("GraphQl integration reset."),
                onHalfOpen: () => logger.LogWarning("GraphQl integration half open."));
    }

    private static IAsyncPolicy<T> GetFallbackPolicy()
    {
        var defaultReturn = new T
        {
            Errors = new[] { new GraphQLError { Message = "Fallback activated." } }
        };
        
        return Policy<T>
            .Handle<BrokenCircuitException>()
            .FallbackAsync(fallbackAction: _ => Task.FromResult(defaultReturn));
    }

    private static IAsyncPolicy<T> GetRetryPolicy(ILogger logger, Resilience resilience)
    {
        return Policy<T>
            .Handle<Exception>()
            .OrResult(result => result.Errors?.Any() == true)
            .WaitAndRetryAsync(resilience.RetryCount,
                sleepDurationProvider: sleepDuration => TimeSpan.FromSeconds(Math.Pow(2, sleepDuration)),
                onRetry: (outcome, timeSpan, retryAttempt, context) =>
                {
                    logger.LogWarning(outcome.Exception,
                        "{RetryAttempt}/{RetryCount} attempt failed, next retry in {Delay} ms.", retryAttempt, resilience.RetryCount,
                        timeSpan.TotalMilliseconds);
                });
    }
}
