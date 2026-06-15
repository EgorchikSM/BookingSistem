using System.Collections.Concurrent;

namespace BookingSistem.Services
{
    public class MetricsService : IMetricsService
    {
        private readonly ConcurrentDictionary<string, long> _counters = new();

        public MetricsService()
        {
            _counters["success"] = 0; _counters["error"] = 0;
            _counters["idempotency"] = 0; _counters["compensation"] = 0;
        }

        public void IncrementSuccess() => _counters.AddOrUpdate("success", 1, (_, v) => v + 1);
        public void IncrementError() => _counters.AddOrUpdate("error", 1, (_, v) => v + 1);
        public void IncrementIdempotency() => _counters.AddOrUpdate("idempotency", 1, (_, v) => v + 1);
        public void IncrementCompensation() => _counters.AddOrUpdate("compensation", 1, (_, v) => v + 1);
        public Dictionary<string, long> GetCurrentMetrics() => _counters.ToDictionary(k => k.Key, v => v.Value);
    }
}