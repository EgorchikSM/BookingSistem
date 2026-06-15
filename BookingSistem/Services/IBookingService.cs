using BookingSistem.Models;

namespace BookingSistem.Services
{
    public interface IBookingService 
    {
        Task<ProcessStatus> HandleEventAsync(ProcessEvent request, string correlationId);
    }
}