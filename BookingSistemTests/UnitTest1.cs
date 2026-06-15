using Moq;
using Microsoft.Extensions.Logging;
using BookingSistem.Services;
using BookingSistem.Models;

public class BookingSystemTests
{
    [Fact]
    public async Task Should_Compensate_When_AccessGrant_Fails()
    {
        var metrics = new MetricsService();
        var logger = new Mock<ILogger<BookingService>>().Object;
        var service = new BookingService(logger, metrics);

        await service.HandleEventAsync(new("p1", "i1", "ПринятьЗаявку"), "c1");
        await service.HandleEventAsync(new("p1", "i2", "Забронировать"), "c2");
        var result = await service.HandleEventAsync(new("p1", "i3", "ВыдатьДоступ"), "fail-id");

        Assert.Equal(ProcessStatus.CompensationExecuted, result);
        Assert.Equal(1, metrics.GetCurrentMetrics()["compensation"]);
    }

    [Fact]
    public async Task Should_Not_Change_State_On_Duplicate_Event()
    {
        var metrics = new MetricsService();
        var logger = new Mock<ILogger<BookingService>>().Object;
        var service = new BookingService(logger, metrics);

        var evt = new ProcessEvent("p1", "idemp1", "ПринятьЗаявку");
        var result1 = await service.HandleEventAsync(evt, "corr1");
        var result2 = await service.HandleEventAsync(evt, "corr1");

        Assert.Equal(result1, result2);
        Assert.Equal(1, metrics.GetCurrentMetrics()["idempotency"]);
    }

    [Fact]
    public async Task Should_Complete_Process_On_Successful_Events()
    {
        var metrics = new MetricsService();
        var logger = new Mock<ILogger<BookingService>>().Object;
        var service = new BookingService(logger, metrics);

        await service.HandleEventAsync(new("p2", "i1", "ПринятьЗаявку"), "c1");
        await service.HandleEventAsync(new("p2", "i2", "Забронировать"), "c2");
        await service.HandleEventAsync(new("p2", "i3", "ВыдатьДоступ"), "c3");
        var result = await service.HandleEventAsync(new("p2", "i4", "Завершить"), "c4");

        Assert.Equal(ProcessStatus.Completed, result);

        Assert.Equal(4, metrics.GetCurrentMetrics()["success"]);
    }

    [Fact]
    public async Task Should_Handle_Different_Events_With_Same_ProcessKey()
    {
        var metrics = new MetricsService();
        var logger = new Mock<ILogger<BookingService>>().Object;
        var service = new BookingService(logger, metrics);

        var result1 = await service.HandleEventAsync(new("p3", "i1", "ПринятьЗаявку"), "c1");
        var result2 = await service.HandleEventAsync(new("p3", "i2", "Забронировать"), "c2");
        var result3 = await service.HandleEventAsync(new("p3", "i2", "Забронировать"), "c2");

        Assert.Equal(ProcessStatus.ResourceReserved, result2);
        Assert.Equal(result2, result3);
        Assert.Equal(1, metrics.GetCurrentMetrics()["idempotency"]);
    }

    [Fact]
    public async Task Should_Log_Transition()
    {
        var metrics = new MetricsService();
        var loggerMock = new Mock<ILogger<BookingService>>();
        var service = new BookingService(loggerMock.Object, metrics);

        await service.HandleEventAsync(new("p4", "i1", "ПринятьЗаявку"), "c1");

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Переход")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
}
