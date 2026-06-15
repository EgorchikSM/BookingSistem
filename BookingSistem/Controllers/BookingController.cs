using Microsoft.AspNetCore.Mvc;
using BookingSistem.Models;
using BookingSistem.Services;

namespace BookingSistem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _service;
        private readonly IMetricsService _metrics;

        public BookingController(IBookingService service, IMetricsService metrics)
        {
            _service = service;
            _metrics = metrics;
        }

        [HttpPost("event")]
        public async Task<IActionResult> PostEvent([FromBody] ProcessEvent request)
        {
            var correlationId = Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
            var status = await _service.HandleEventAsync(request, correlationId);
            return Ok(new { Status = status.ToString(), CorrelationId = correlationId });
        }

        [HttpGet("metrics")]
        public IActionResult GetMetrics() => Ok(_metrics.GetCurrentMetrics());
    }
}