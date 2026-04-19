# Coverage Improvements — Design

**Date:** 2026-04-19
**Status:** Approved

## Context

Baseline coverage on `jumbled-function` (after excluding auto-generated `**/*.g.cs` SDK files):

- Overall: 60.3% line, 90.6% branch
- `Services/WordleAssistService.cs`: 100% line, 96.7% branch
- `Models/WordleAssistRequest.cs`: 100%
- `Program.cs`: 0%
- `Functions/HealthCheck.cs`: 0%
- `Functions/WordleAssist.cs`: 0%

The low overall number is driven by (a) untested Azure Function entry points and (b) `Program.cs` bootstrap code that doesn't meaningfully benefit from unit tests.

## Goal

Exclude code that isn't meaningfully testable via unit tests, and add unit tests for the Function entry points. Target: ~95%+ line coverage on user code.

## Design

### 1. Exclusions

Apply `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` to:

- **`Program.cs`** — top-level host bootstrap. No branching logic. Since top-level statements compile into a generated `Program` class, convert to an explicit `public class Program` with a `Main` method decorated with `[ExcludeFromCodeCoverage]`. This is unambiguous and makes the exclusion locally visible to readers of `Program.cs`.
- **`WordleAssist.LogRequestReceived`** — the `[LoggerMessage]` partial method. Body is source-generated.

Update `coverlet.runsettings` to honor the attribute:

```xml
<ExcludeByFile>**/*.g.cs</ExcludeByFile>
<ExcludeByAttribute>ExcludeFromCodeCoverageAttribute</ExcludeByAttribute>
```

### 2. Test dependencies

Add to `Jumbled.Tests.csproj`:

```xml
<PackageReference Include="NSubstitute" Version="5.3.0" />
```

Chosen over Moq due to the ongoing SponsorLink controversy and NSubstitute's cleaner syntax.

### 3. New test files

#### `Jumbled.Tests/Functions/HealthCheckTests.cs`

Substitutes `HealthCheckService` (abstract base, substitutable via NSubstitute).

| Case | Arrange | Assert |
|---|---|---|
| Healthy report returns 200 | `CheckHealthAsync` returns `HealthReport` with `HealthStatus.Healthy` | `ObjectResult.StatusCode == 200` |
| Unhealthy report returns 503 | status `Unhealthy` | `StatusCode == 503` |
| Degraded report returns 503 | status `Degraded` | `StatusCode == 503` (anything non-Healthy) |
| Response shape | healthy report with one entry | payload contains `status`, `timestamp`, `checks[]` with `name`/`status`/`description`/`duration` |

#### `Jumbled.Tests/Functions/WordleAssistTests.cs`

Substitutes `IWordleAssistService`, `ILogger<WordleAssist>`. Uses a real `TelemetryClient` with a default `TelemetryConfiguration` (telemetry is dropped silently, no assertions against it — the code path just needs to not throw). `HttpRequest` built via `DefaultHttpContext().Request` with `QueryCollection`.

| Case | Arrange | Assert |
|---|---|---|
| All three query params present | `?word=ABC&exclude=XY&include=_a_` | service called with lowercase values `("abc","xy","_a_")`; result wrapped in `OkObjectResult` |
| Missing params default to empty | `?word=abc` only | service called with `("abc","","")` |
| Service result flows through | service returns `["alpha","beta"]` | `OkObjectResult.Value` equals that list |
| Lowercasing | `?word=ABC&exclude=XY` | service receives lowercase strings |

### 4. Tradeoff noted

Using a real `TelemetryClient` rather than abstracting it behind an interface. Rationale: `TelemetryClient` tolerates a null/default configuration by dropping events, so tests don't need it abstracted; introducing an interface purely for test seams would be premature. If telemetry assertions become necessary later, revisit.

## Expected outcome

| File | Before | After |
|---|---|---|
| `Services/WordleAssistService.cs` | 100% | 100% |
| `Models/WordleAssistRequest.cs` | 100% | 100% |
| `Functions/HealthCheck.cs` | 0% | ~100% |
| `Functions/WordleAssist.cs` | 0% | ~100% (minus excluded `LogRequestReceived`) |
| `Program.cs` | 0% | excluded |
| **Overall (user code)** | **60.3%** | **~95%+** |

## Out of scope

- Integration tests against a running Functions host.
- Build-time coverage gates (user chose "measure only").
- Refactoring `WordleAssistService` to inject the words file path (current file-system read works because resources are copied to output).
