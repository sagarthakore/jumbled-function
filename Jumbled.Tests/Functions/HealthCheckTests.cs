using Jumbled.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;

namespace Jumbled.Tests.Functions;

public class HealthCheckTests
{
    private static HealthReport BuildReport(HealthStatus status, string entryName = "db") =>
        new(
            new Dictionary<string, HealthReportEntry>
            {
                [entryName] = new HealthReportEntry(
                    status,
                    description: "entry description",
                    duration: TimeSpan.FromMilliseconds(5),
                    exception: null,
                    data: null)
            },
            totalDuration: TimeSpan.FromMilliseconds(10));

    private static (HealthCheck sut, HealthCheckService service) CreateSut()
    {
        var service = Substitute.For<HealthCheckService>();
        return (new HealthCheck(service), service);
    }

    [Fact]
    public async Task Run_HealthyReport_Returns200()
    {
        var (sut, service) = CreateSut();
        service.CheckHealthAsync(
                Arg.Any<Func<HealthCheckRegistration, bool>?>(),
                Arg.Any<CancellationToken>())
            .Returns(BuildReport(HealthStatus.Healthy));

        var result = await sut.Run(new DefaultHttpContext().Request);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
    }

    [Fact]
    public async Task Run_UnhealthyReport_Returns503()
    {
        var (sut, service) = CreateSut();
        service.CheckHealthAsync(
                Arg.Any<Func<HealthCheckRegistration, bool>?>(),
                Arg.Any<CancellationToken>())
            .Returns(BuildReport(HealthStatus.Unhealthy));

        var result = await sut.Run(new DefaultHttpContext().Request);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, objectResult.StatusCode);
    }

    [Fact]
    public async Task Run_DegradedReport_Returns503()
    {
        var (sut, service) = CreateSut();
        service.CheckHealthAsync(
                Arg.Any<Func<HealthCheckRegistration, bool>?>(),
                Arg.Any<CancellationToken>())
            .Returns(BuildReport(HealthStatus.Degraded));

        var result = await sut.Run(new DefaultHttpContext().Request);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, objectResult.StatusCode);
    }

    [Fact]
    public async Task Run_HealthyReport_ResponseContainsEntryDetails()
    {
        var (sut, service) = CreateSut();
        service.CheckHealthAsync(
                Arg.Any<Func<HealthCheckRegistration, bool>?>(),
                Arg.Any<CancellationToken>())
            .Returns(BuildReport(HealthStatus.Healthy, entryName: "db"));

        var result = await sut.Run(new DefaultHttpContext().Request);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var value = objectResult.Value!;
        var type = value.GetType();

        var status = (string)type.GetProperty("status")!.GetValue(value)!;
        var timestamp = (DateTime)type.GetProperty("timestamp")!.GetValue(value)!;
        var checksEnumerable = (System.Collections.IEnumerable)type.GetProperty("checks")!.GetValue(value)!;

        Assert.Equal("Healthy", status);
        Assert.True(timestamp <= DateTime.UtcNow);

        var checks = checksEnumerable.Cast<object>().ToList();
        Assert.Single(checks);

        var entry = checks[0];
        var entryType = entry.GetType();
        Assert.Equal("db", entryType.GetProperty("name")!.GetValue(entry));
        Assert.Equal("Healthy", entryType.GetProperty("status")!.GetValue(entry));
        Assert.Equal("entry description", entryType.GetProperty("description")!.GetValue(entry));
        Assert.Equal(TimeSpan.FromMilliseconds(5), entryType.GetProperty("duration")!.GetValue(entry));
    }
}
