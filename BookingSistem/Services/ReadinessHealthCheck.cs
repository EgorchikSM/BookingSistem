using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BookingSistem.Services
{
    public class ReadinessHealthCheck : IHealthCheck
    {
        private readonly IMetricsService _metrics;

        public ReadinessHealthCheck(IMetricsService metrics)
        {
            _metrics = metrics;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var metrics = _metrics.GetCurrentMetrics();
            if (metrics.TryGetValue("compensation", out var compensations) && compensations > 5)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Слишком много компенсаций"));
            }
            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}