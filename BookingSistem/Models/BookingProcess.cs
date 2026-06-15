public enum ProcessStatus { New, RequestAccepted, ResourceReserved, AccessGranted, Completed, CompensationExecuted, Error }


namespace BookingSistem.Models
{
    public class BookingProcess
    {
        public string ProcessKey { get; set; }
        public ProcessStatus Status { get; set; } = ProcessStatus.New;
        public HashSet<string> HandledIdempotencyKeys { get; } = new();
    }
}