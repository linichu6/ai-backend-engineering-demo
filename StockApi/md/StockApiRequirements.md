# Stock Price API - Implementation Requirements

## Objective

Create a .NET 8 ASP.NET Core Web API project named **StockApi**.

The API should return a stock's closing price for a specified date.

If the date is not provided, use the current UTC date.

Use a layered architecture:

```text
Controller
    ↓
Provider
    ↓
Repository
```

Dependency injection must be used throughout the application.

---

# Technical Requirements

## Framework

* .NET 8
* ASP.NET Core Web API
* Swagger enabled
* Dependency Injection
* IHttpClientFactory

---

# Project Structure

Create the following folders:

```text
Controllers
Models
Providers
Repositories
```

Structure:

```text
StockApi
│
├── Controllers
│   └── StockController.cs
│
├── Models
│   ├── StockPriceRequest.cs
│   └── StockPriceResponse.cs
│
├── Providers
│   ├── IStockProvider.cs
│   └── StockProvider.cs
│
├── Repositories
│   ├── IStockRepository.cs
│   └── StockRepository.cs
│
└── Program.cs
```

---

# API Endpoint

## Get Stock Price

Route:

```http
GET /api/stocks/price
```

Query Parameters:

| Name   | Required | Example    |
| ------ | -------- | ---------- |
| ticker | Yes      | AAPL       |
| date   | No       | 2026-06-18 |

Examples:

```http
GET /api/stocks/price?ticker=AAPL
```

```http
GET /api/stocks/price?ticker=AAPL&date=2026-06-18
```

Behavior:

* ticker is required
* date is optional
* if date is null, use DateTime.UtcNow.Date

---

# Request Model

Create:

```csharp
public class StockPriceRequest
{
    public string Ticker { get; set; } = string.Empty;

    public DateTime? Date { get; set; }
}
```

---

# Response Model

Create:

```csharp
public class StockPriceResponse
{
    public string Ticker { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public decimal ClosePrice { get; set; }
}
```

---

# Controller Layer

Create:

```csharp
StockController
```

Responsibilities:

* Accept HTTP request
* Validate ticker
* Call provider
* Return IActionResult

Route:

```csharp
[ApiController]
[Route("api/stocks")]
```

Endpoint:

```csharp
[HttpGet("price")]
```

The controller must not contain business logic.

---

# Provider Layer

Create:

```csharp
IStockProvider
StockProvider
```

Responsibilities:

* Business logic
* Determine effective date
* Call repository
* Return StockPriceResponse

Interface:

```csharp
Task<StockPriceResponse> GetStockPriceAsync(
    StockPriceRequest request);
```

---

# Repository Layer

Create:

```csharp
IStockRepository
StockRepository
```

Responsibilities:

* External API communication
* Parse response
* Return StockPriceResponse

Use HttpClient injected through constructor.

---

# Stock Data Source

Use Yahoo Finance Chart API.

Pattern:

```text
https://query1.finance.yahoo.com/v8/finance/chart/{ticker}
```

Parameters:

```text
period1={unixStart}
period2={unixEnd}
interval=1d
```

Example:

```text
https://query1.finance.yahoo.com/v8/finance/chart/AAPL?period1=1718668800&period2=1718755200&interval=1d
```

Repository should:

1. Convert requested date to Unix timestamps.
2. Call Yahoo Finance.
3. Parse JSON.
4. Extract closing price.
5. Populate StockPriceResponse.

---

# Error Handling

Return:

## 400 Bad Request

If ticker is empty.

Example:

```json
{
  "message": "Ticker is required."
}
```

## 404 Not Found

If ticker does not exist.

Example:

```json
{
  "message": "Ticker not found."
}
```

## 500 Internal Server Error

For unexpected failures.

---

# Dependency Injection

Register services in Program.cs.

Repository:

```csharp
builder.Services.AddHttpClient<IStockRepository, StockRepository>();
```

Provider:

```csharp
builder.Services.AddScoped<IStockProvider, StockProvider>();
```

Controllers:

```csharp
builder.Services.AddControllers();
```

Swagger:

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
```

---

# Coding Standards

Requirements:

* Use async/await everywhere.
* Use interfaces for Provider and Repository.
* Use constructor injection.
* No business logic in Controller.
* No direct HttpClient usage in Controller.
* Use nullable reference types.
* Use XML comments on public methods.
* Use meaningful exception messages.
* Follow SOLID principles.

---

# Expected Swagger Example

Request:

```http
GET /api/stocks/price?ticker=MSFT
```

Response:

```json
{
  "ticker": "MSFT",
  "date": "2026-06-19",
  "closePrice": 612.41
}
```

Request:

```http
GET /api/stocks/price?ticker=AAPL&date=2026-06-18
```

Response:

```json
{
  "ticker": "AAPL",
  "date": "2026-06-18",
  "closePrice": 248.16
}
```

---

# Unit Testing

Create a separate xUnit test project.

Test:

* Controller
* Provider
* Repository

Use:

* xUnit
* Moq

Verify:

* Valid ticker returns result.
* Missing ticker returns BadRequest.
* Invalid ticker returns NotFound.
* Null date uses current date.
* Repository parses Yahoo response correctly.

```
```
