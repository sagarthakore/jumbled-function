# Jumbled - Wordle Assistant Azure Function

A .NET 10.0 Azure Functions application that provides assistance for Wordle players through a serverless API

## 🚀 Features

- Wordle assistance functionality through HTTP-triggered Azure Function
- Built with .NET 10.0 and Azure Functions v4
- Application Insights integration for telemetry
- Dependency injection pattern using `IWordleAssistService`

## 🛠️ Project Structure

- `Program.cs` - Application entry point and DI configuration
- `WordleAssistService` - Core service implementation
- `Jumbled.Tests` - xUnit test project

## 📋 Prerequisites

- .NET 10.0 SDK
- Azure Functions Core Tools
- Visual Studio 2022 or Visual Studio Code

## 🏃‍♂️ Getting Started

1. Clone the repository
2. Copy `local.settings.example.json` to `local.settings.json`
3. Run the project:

```bash
func host start --script-root ./Jumbled
```

## 🧪 Testing

```bash
dotnet test Jumbled.Tests/Jumbled.Tests.csproj
```

## 🐳 Docker Support

Build and run using Docker:

```bash
docker build -t jumbled -f Jumbled/Dockerfile .
docker run -p 8080:80 jumbled
```

Build and run using Docker Compose:

```bash
docker compose up -d
```

## 📦 Deployment

The application automatically deploys to Azure Functions using GitHub Actions when changes are pushed to the main branch. Configuration can be found in build-deploy.yml.