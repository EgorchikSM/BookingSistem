namespace BookingSistem.Models
{
    public record ProcessEvent(string ProcessKey, string IdempotencyKey, string EventType);
}