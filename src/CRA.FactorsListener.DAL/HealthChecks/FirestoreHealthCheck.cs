using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace CRA.FactorsListener.DAL.HealthChecks
{
    public class FirestoreHealthCheck : IHealthCheck
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<FirestoreHealthCheck> _logger;

        public FirestoreHealthCheck(FirestoreDb firestoreDb, ILogger<FirestoreHealthCheck> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new ())
        {
            try
            {
                await _firestoreDb.Collection("Users").Limit(0).GetSnapshotAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (OperationCanceledException ex)
            {
                var timeoutInMilliseconds = context.Registration.Timeout.Milliseconds;
                _logger.LogWarning(ex, "Health check failed after reaching timeout limit of {TimeoutInMilliseconds} ms", timeoutInMilliseconds);

                var healthCheckResult = new HealthCheckResult(context.Registration.FailureStatus, "Health check failed after reaching timeout limit.");

                return healthCheckResult;
            }
        }
    }
}
