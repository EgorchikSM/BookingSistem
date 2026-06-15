using BookingSistem.Models;
using BookingSistem.Services;
using System.Collections.Concurrent;

public class BookingService : IBookingService
{
    private static readonly ConcurrentDictionary<string, BookingProcess> _db = new();
    private readonly ILogger<BookingService> _logger;
    private readonly IMetricsService _metrics;

    public BookingService(ILogger<BookingService> logger, IMetricsService metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<ProcessStatus> HandleEventAsync(ProcessEvent request, string correlationId) {
        var process = _db.GetOrAdd(request.ProcessKey, _ => new BookingProcess { ProcessKey = request.ProcessKey });

        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId });

        if (process.HandledIdempotencyKeys.Contains(request.IdempotencyKey)) {
            _logger.LogInformation("Повтор события {IdempotencyKey}. Состояние не изменено.", request.IdempotencyKey);
            _metrics.IncrementIdempotency();
            return process.Status;
        }

        try {
            var oldStatus = process.Status;
            process.Status = request.EventType switch {
                "ПринятьЗаявку" when process.Status == ProcessStatus.New => ProcessStatus.RequestAccepted,
                "Забронировать" when process.Status == ProcessStatus.RequestAccepted => ProcessStatus.ResourceReserved,
                "ВыдатьДоступ" when process.Status == ProcessStatus.ResourceReserved => PerformAccessGrant(correlationId),
                "Завершить" when process.Status == ProcessStatus.AccessGranted => ProcessStatus.Completed,
                _ => process.Status
            };

            if (process.Status != oldStatus) {
                _logger.LogInformation("Переход: {Old} -> {New}", oldStatus, process.Status);
                _metrics.IncrementSuccess();
            }

            process.HandledIdempotencyKeys.Add(request.IdempotencyKey);
            return process.Status;
        }
        catch (Exception ex) {
            _metrics.IncrementError();
            if (process.Status == ProcessStatus.ResourceReserved && request.EventType == "ВыдатьДоступ") {
                _logger.LogWarning("Сбой на шаге ВыдатьДоступ. Запуск компенсации.");
                process.Status = ProcessStatus.CompensationExecuted;
                _metrics.IncrementCompensation();
                return process.Status;
            }
            process.Status = ProcessStatus.Error;
            return process.Status;
        }
    }

    private ProcessStatus PerformAccessGrant(string correlationId) {
        if (correlationId.Contains("fail")) throw new Exception("Критический сбой системы доступа");
        return ProcessStatus.AccessGranted;
    }
}