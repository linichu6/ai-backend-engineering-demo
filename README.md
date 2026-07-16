# StockApi

StockApi is a .NET 8 ASP.NET Core Web API that returns a stock's closing price for a specified date. If no date is provided, the API uses the current UTC date. The project follows a layered architecture: Controller -> Provider -> Repository and uses dependency injection and `IHttpClientFactory` for external calls.

```text
Client
   │
   ▼
ASP.NET Core Controller
   │
   ▼
Stock Provider
   │
   ├── Cache Hit ───────────────► Return Result
   │
   └── Cache Miss
           │
           ▼
    Stock Repository
           │
           ▼
    Alpha Vantage API
```

## API Contract

### Endpoint
| Endpoint  | Description               |
| --------- | ------------------------- |
| `/health` | Returns API health status |


GET `/api/stocks/price`

Query parameters:
- `ticker` (string, required) — Stock ticker symbol (e.g., `AAPL`, `MSFT`).
- `date` (ISO 8601 date, optional) — Date to return the closing price for (e.g., `2026-06-18`). If omitted, the server uses `DateTime.UtcNow.Date`.

Examples:
GET /api/stocks/price?ticker=AAPL
GET /api/stocks/price?ticker=AAPL&date=2026-06-18

### Request model

public class StockPriceRequest
{
    public string Ticker { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
}

### Response model

public class StockPriceResponse
{
    public string Ticker { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal ClosePrice { get; set; }
}

### Success response (200)

Example:

{
  "ticker": "AAPL",
  "date": "2026-06-18",
  "closePrice": 248.16
}

### Error responses

- **400 Bad Request**
  - Condition: Missing or empty `ticker`.
  - Body: `{ "message": "Ticker is required." }`

- **404 Not Found**
  - Condition: Ticker does not exist or no data for requested date.
  - Body: `{ "message": "Ticker not found." }`

- **500 Internal Server Error**
  - Condition: Unexpected failures.
  - Body: `{ "message": "An unexpected error occurred." }`

## Architecture and Design

- **Controller**: Receives HTTP requests, validates input, maps to `StockPriceRequest`, and calls the provider. No business logic in the controller.
- **Provider**: `IStockProvider` / `StockProvider` implements business logic (resolving effective date, validation) and calls the repository.
- **Repository**: `IStockRepository` / `AVStockRepository` uses `HttpClient` through `IHttpClientFactory` to call the Alpha Vantage API, parse the JSON response, and return the closing price for the requested date.

## Dependency Injection

Registered in `Program.cs`:

builder.Services.AddHttpClient<IStockRepository, StockRepository>();
builder.Services.AddScoped<IStockProvider, StockProvider>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

## Running the API

1. Restore and run:
   dotnet restore
   dotnet run --project StockApi
2. When running in `Development`, Swagger is available at `/swagger`.

## Tests

A test project `StockApi.Tests` (xUnit + Moq) is included. It contains unit tests for the Controller, Provider, and Repository layers. Run tests:

dotnet test ./StockApi.Tests

**Notes**:
- The repository test uses a mocked `HttpMessageHandler` to simulate Yahoo API responses.
- Ensure `StockApi.Tests.csproj` includes `Microsoft.NET.Test.Sdk` to enable test discovery and execution.

## External API

This project uses the Alpha Vantage API to retrieve historical daily stock prices.

The repository calls the `TIME_SERIES_DAILY` endpoint and searches the returned time-series data for the requested date.

If no date is provided, the API uses the current UTC date. If the requested date does not contain trading data, such as a weekend or market holiday, the service returns the latest available closing price on or before the requested date.

The Alpha Vantage API key is loaded from configuration and must not be committed to source control.

### Local configuration

Use .NET User Secrets:

```bash
dotnet user-secrets init --project StockApi/MyWebService.csproj

dotnet user-secrets set \
  "AlphaVantage:ApiKey" \
  "YOUR_API_KEY" \
  --project StockApi/MyWebService.csproj
## AWS ECS Deployment (Fargate)

Add the following task definition snippet and CloudWatch Logs configuration when deploying this service to AWS ECS (Fargate).

- Launch type: Fargate
- OS: Linux
- Container port: 8080
- Health endpoint: `/health`
- Logs: CloudWatch Logs (awslogs driver)

### Usage notes:
- Set `ASPNETCORE_ENVIRONMENT=Production` in the task definition environment variables.
- Ensure the ALB target group health check is set to `HTTP:8080/health`.
- Configure the task IAM role and security groups according to your environment.

Add the JSON task definition snippet below to your deployment repository or use it as a starting point in the ECS Console.

```json
{
  "family": "StockApi",
  "containerDefinitions": [
    {
      "name": "StockApi",
      "image": "your-docker-image",
      "memory": 512,
      "cpu": 256,
      "essential": true,
      "portMappings": [
        {
          "containerPort": 8080,
          "hostPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/StockApi",
          "awslogs-region": "your-region",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

## Contributing

Follow the existing coding standards: use async/await, constructor injection, nullable reference types, and XML comments on public methods. Unit tests should use xUnit and Moq.

## License

Project is provided as sample code.

This version maintains the original structure while enhancing clarity and coherence, ensuring that all necessary information is presented in a logical flow. The new AWS ECS Deployment section has been seamlessly integrated into the document.
