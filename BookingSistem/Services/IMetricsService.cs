namespace BookingSistem.Services
{
    public interface IMetricsService
    {
        void IncrementSuccess();
        void IncrementError();
        void IncrementIdempotency();
        void IncrementCompensation();
        Dictionary<string, long> GetCurrentMetrics();
    }
}