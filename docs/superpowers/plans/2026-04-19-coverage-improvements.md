# Coverage Improvements Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reach ~95%+ line coverage on user code by excluding non-testable bootstrap from coverage and adding unit tests for the two Azure Function entry points.

**Architecture:** Apply `[ExcludeFromCodeCoverage]` to `Program.cs` (host bootstrap) and the `[LoggerMessage]` partial in `WordleAssist`. Add NSubstitute-based unit tests for `HealthCheck` and `WordleAssist` that substitute `HealthCheckService`, `ILogger<T>`, `IWordleAssistService`, and use a real `TelemetryClient` with an empty `TelemetryConfiguration` (events dropped silently).

**Tech Stack:** .NET 10, xUnit 2.9.2, coverlet.collector 6.0.4, NSubstitute 5.3.0, Azure Functions Worker, Application Insights.

**Spec:** `docs/superpowers/specs/2026-04-19-coverage-improvements-design.md`

---

## File Structure

**Modify:**
- `Jumbled/Program.cs` — convert top-level statements to explicit `Program` class decorated with `[ExcludeFromCodeCoverage]`.
- `Jumbled/Functions/WordleAssist.cs` — add `[ExcludeFromCodeCoverage]` to `LogRequestReceived` partial method.
- `Jumbled.Tests/Jumbled.Tests.csproj` — add NSubstitute package reference.
- `coverlet.runsettings` — add `ExcludeByAttribute` entry.

**Create:**
- `Jumbled.Tests/Functions/HealthCheckTests.cs` — 4 tests covering healthy/unhealthy/degraded status codes and response shape.
- `Jumbled.Tests/Functions/WordleAssistTests.cs` — 4 tests covering param parsing, lowercasing, defaults, and result pass-through.

No existing files are deleted. No shared helpers — each test file is self-contained.

---

### Task 1: Add NSubstitute package

**Files:**
- Modify: `Jumbled.Tests/Jumbled.Tests.csproj`

- [ ] **Step 1: Add the package reference**

Edit `Jumbled.Tests/Jumbled.Tests.csproj`. In the existing `<ItemGroup>` that holds `<PackageReference>` entries, add NSubstitute:

```xml
<ItemGroup>
  <PackageReference Include="coverlet.collector" Version="6.0.4" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  <PackageReference Include="NSubstitute" Version="5.3.0" />
  <PackageReference Include="xunit" Version="2.9.2" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
</ItemGroup>
```

- [ ] **Step 2: Restore and verify build still compiles**

Run from `C:/Development/jumbled-function`:
```bash
dotnet restore && dotnet build
```
Expected: `Build succeeded.` with zero errors.

- [ ] **Step 3: Commit**

```bash
git add Jumbled.Tests/Jumbled.Tests.csproj
git commit -m "test: add NSubstitute for Function-level unit tests"
```

---

### Task 2: Update coverlet.runsettings to honor `[ExcludeFromCodeCoverage]`

**Files:**
- Modify: `coverlet.runsettings`

- [ ] **Step 1: Add ExcludeByAttribute entry**

Replace the `<Configuration>` block in `coverlet.runsettings` with:

```xml
<Configuration>
  <Format>cobertura</Format>
  <ExcludeByFile>**/*.g.cs</ExcludeByFile>
  <ExcludeByAttribute>ExcludeFromCodeCoverageAttribute</ExcludeByAttribute>
</Configuration>
```

Keep everything else (RunSettings root, DataCollector element) identical.

- [ ] **Step 2: Verify tests still pass and coverage collects**

```bash
rm -rf TestResults && dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings --results-directory ./TestResults
```
Expected: 7 passed, 0 failed. A `coverage.cobertura.xml` file is produced in `TestResults/<guid>/`.

- [ ] **Step 3: Commit**

```bash
git add coverlet.runsettings
git commit -m "test: honor ExcludeFromCodeCoverageAttribute in coverage runs"
```

---

### Task 3: Convert `Program.cs` to explicit class with `[ExcludeFromCodeCoverage]`

**Files:**
- Modify: `Jumbled/Program.cs`

- [ ] **Step 1: Replace file contents**

Overwrite `Jumbled/Program.cs` with:

```csharp
using System.Diagnostics.CodeAnalysis;
using Jumbled.Services;
using Jumbled.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jumbled;

[ExcludeFromCodeCoverage(Justification = "Host bootstrap has no branching logic to assert")]
public static class Program
{
    public static async Task Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWebApplication()
            .ConfigureServices(services =>
            {
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();
                services.AddSingleton<IWordleAssistService, WordleAssistService>();
                services.AddHealthChecks();
            })
            .Build();

        await host.RunAsync();
    }
}
```

- [ ] **Step 2: Build and run existing tests**

```bash
dotnet build && dotnet test
```
Expected: Build succeeds, 7 tests pass. Azure Functions still starts from `Program.Main` — no functional change.

- [ ] **Step 3: Commit**

```bash
git add Jumbled/Program.cs
git commit -m "refactor: convert Program.cs to explicit class excluded from coverage"
```

---

### Task 4: Exclude `LogRequestReceived` partial from coverage

**Files:**
- Modify: `Jumbled/Functions/WordleAssist.cs`

- [ ] **Step 1: Add using and attribute**

Edit `Jumbled/Functions/WordleAssist.cs`. Add this using near the top with the others:
```csharp
using System.Diagnostics.CodeAnalysis;
```

Then decorate the `LogRequestReceived` method at the bottom of the class. Replace:
```csharp
    [LoggerMessage(Level = LogLevel.Information, Message = "Request Received - {@Request}")]
    private static partial void LogRequestReceived(ILogger logger, WordleAssistRequest request);
```
with:
```csharp
    [ExcludeFromCodeCoverage]
    [LoggerMessage(Level = LogLevel.Information, Message = "Request Received - {@Request}")]
    private static partial void LogRequestReceived(ILogger logger, WordleAssistRequest request);
```

- [ ] **Step 2: Build**

```bash
dotnet build
```
Expected: Build succeeds.

- [ ] **Step 3: Commit**

```bash
git add Jumbled/Functions/WordleAssist.cs
git commit -m "test: exclude source-generated LogRequestReceived from coverage"
```

---

### Task 5: Add `HealthCheck` tests

**Files:**
- Create: `Jumbled.Tests/Functions/HealthCheckTests.cs`

- [ ] **Step 1: Create the test file with all four tests**

Create `Jumbled.Tests/Functions/HealthCheckTests.cs`:

```csharp
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
```

**Note on reflection:** the response object in `HealthCheck.Run` is an anonymous type, so tests access fields by reflection. This is deliberate — asserting behavior without forcing a public response DTO.

**Note on NSubstitute:** `HealthCheckService.CheckHealthAsync()` (no-arg) is a concrete wrapper around the abstract `CheckHealthAsync(predicate, cancellationToken)`. NSubstitute substitutes the abstract method; the concrete overload forwards to it at runtime.

- [ ] **Step 2: Run the new tests**

```bash
dotnet test --filter "FullyQualifiedName~HealthCheckTests"
```
Expected: 4 passed, 0 failed.

- [ ] **Step 3: Run the full suite**

```bash
dotnet test
```
Expected: 11 passed (7 existing + 4 new), 0 failed.

- [ ] **Step 4: Commit**

```bash
git add Jumbled.Tests/Functions/HealthCheckTests.cs
git commit -m "test: add unit tests for HealthCheck function"
```

---

### Task 6: Add `WordleAssist` function tests

**Files:**
- Create: `Jumbled.Tests/Functions/WordleAssistTests.cs`

- [ ] **Step 1: Create the test file with all four tests**

Create `Jumbled.Tests/Functions/WordleAssistTests.cs`:

```csharp
using Jumbled.Functions;
using Jumbled.Models;
using Jumbled.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NSubstitute;

namespace Jumbled.Tests.Functions;

public class WordleAssistTests
{
    private static (WordleAssist sut, IWordleAssistService service) CreateSut()
    {
        var logger = Substitute.For<ILogger<WordleAssist>>();
        var service = Substitute.For<IWordleAssistService>();
        var telemetry = new TelemetryClient(new TelemetryConfiguration());
        return (new WordleAssist(logger, service, telemetry), service);
    }

    private static HttpRequest BuildRequest(params (string key, string value)[] query)
    {
        var context = new DefaultHttpContext();
        var dict = query.ToDictionary(q => q.key, q => new StringValues(q.value));
        context.Request.Query = new QueryCollection(dict);
        return context.Request;
    }

    [Fact]
    public void Run_AllParametersPresent_LowercasesAndPassesToService()
    {
        var (sut, service) = CreateSut();
        service.GetWordGuess(Arg.Any<WordleAssistRequest>())
            .Returns(new List<string> { "alpha" });

        var result = sut.Run(BuildRequest(
            ("word", "ABC"),
            ("exclude", "XY"),
            ("include", "_A_")));

        service.Received(1).GetWordGuess(Arg.Is<WordleAssistRequest>(r =>
            r.Word == "abc" && r.Exclude == "xy" && r.Include == "_a_"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(new List<string> { "alpha" }, ok.Value);
    }

    [Fact]
    public void Run_OnlyWordProvided_ExcludeAndIncludeDefaultToEmpty()
    {
        var (sut, service) = CreateSut();
        service.GetWordGuess(Arg.Any<WordleAssistRequest>())
            .Returns(new List<string>());

        sut.Run(BuildRequest(("word", "abc")));

        service.Received(1).GetWordGuess(Arg.Is<WordleAssistRequest>(r =>
            r.Word == "abc" && r.Exclude == "" && r.Include == ""));
    }

    [Fact]
    public void Run_NoQueryParameters_AllFieldsEmpty()
    {
        var (sut, service) = CreateSut();
        service.GetWordGuess(Arg.Any<WordleAssistRequest>())
            .Returns(new List<string>());

        sut.Run(BuildRequest());

        service.Received(1).GetWordGuess(Arg.Is<WordleAssistRequest>(r =>
            r.Word == "" && r.Exclude == "" && r.Include == ""));
    }

    [Fact]
    public void Run_ServiceReturnsList_WrappedInOkObjectResult()
    {
        var (sut, service) = CreateSut();
        var expected = new List<string> { "one", "two", "three" };
        service.GetWordGuess(Arg.Any<WordleAssistRequest>())
            .Returns(expected);

        var result = sut.Run(BuildRequest(("word", "_r___")));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }
}
```

- [ ] **Step 2: Run the new tests**

```bash
dotnet test --filter "FullyQualifiedName~WordleAssistTests&FullyQualifiedName~Functions"
```
Expected: 4 passed, 0 failed.

- [ ] **Step 3: Run the full suite**

```bash
dotnet test
```
Expected: 15 passed (7 existing + 4 HealthCheck + 4 WordleAssist), 0 failed.

- [ ] **Step 4: Commit**

```bash
git add Jumbled.Tests/Functions/WordleAssistTests.cs
git commit -m "test: add unit tests for WordleAssist function"
```

---

### Task 7: Verify final coverage

**Files:**
- None modified.

- [ ] **Step 1: Run coverage collection**

```bash
rm -rf TestResults && dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings --results-directory ./TestResults
```
Expected: 15 tests passed. `coverage.cobertura.xml` produced in `TestResults/<guid>/`.

- [ ] **Step 2: Inspect the coverage report**

Open the generated `coverage.cobertura.xml`. Verify:
- Overall `line-rate` ≥ 0.95 (i.e. 95%+).
- `Jumbled.Services.WordleAssistService` line-rate = 1.
- `Jumbled.Models.WordleAssistRequest` line-rate = 1.
- `Jumbled.Functions.HealthCheck` line-rate = 1 (or very close — the healthy-response-shape test exercises every line).
- `Jumbled.Functions.WordleAssist` line-rate = 1 (minus the excluded `LogRequestReceived` partial, which should NOT appear in the report).
- `Program` class does NOT appear in the report (excluded).

If any target is missed, the plan has a gap — add the missing test before proceeding.

- [ ] **Step 3: Commit any coverage-config cleanup (if needed)**

If no further changes, skip. Otherwise:
```bash
git add <files>
git commit -m "test: tune coverage exclusions"
```

---

## Self-review notes

- **Spec coverage:** Exclusion for `Program.cs` → Task 3. Exclusion for `LogRequestReceived` → Task 4. runsettings attribute → Task 2. NSubstitute → Task 1. `HealthCheck` tests (healthy/unhealthy/degraded/shape) → Task 5. `WordleAssist` tests (all-params/defaults/none/result) → Task 6. Verification → Task 7. No spec requirement is unaddressed.
- **Ambiguity:** `HealthCheckService` is abstract — NSubstitute intercepts the abstract `CheckHealthAsync(predicate, ct)` overload; the no-arg call from production code forwards to it. Tests use `Arg.Any<Func<HealthCheckRegistration, bool>?>()` to match.
- **Type consistency:** Test helper names (`BuildReport`, `CreateSut`, `BuildRequest`) are consistent within each file. No cross-file helpers.
