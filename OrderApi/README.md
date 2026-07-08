# OrderApi

OrderApi is a .NET 8 ASP.NET Core Web API that handles PayPal payment orders and publishes them to Kafka for asynchronous processing. This is a sample implementation and does not process real payments.

## Overview

OrderApi follows a layered architecture: Controller → Provider → Service, utilizing dependency injection and Kafka for event-driven payment processing.

## API Contract

### Endpoint

`POST /api/orders/paypal`

### Request Model

````````

### Response Model (202 Accepted)

````````

## Error Responses

- **400 Bad Request** — Missing or invalid fields (OrderId, Amount, PayerEmail, Currency)
- **500 Internal Server Error** — Unexpected failures

## Architecture

- **Controller**: Receives HTTP requests, validates input, and calls the provider.
- **Provider**: Implements business logic and coordinates with Kafka service.
- **Service**: Publishes payment events to Kafka for processing.

## Kafka Integration

Payment orders are published to the `payment-events` topic with the following message format:

````````

## Configuration

Update `appsettings.json` with your Kafka broker details:

````````

## Running the API

1. Ensure Kafka is running locally or update `appsettings.json` with your broker address.
2. Restore and run:

````````
3. Swagger is available at `/swagger` in Development mode.

## Testing

Run unit tests:
````````

## Dependencies

- Swashbuckle.AspNetCore (6.6.2) — Swagger integration
- Confluent.Kafka (2.3.0) — Kafka producer client

## Coding Standards

- Use async/await for all I/O operations
- Constructor injection for dependencies
- Nullable reference types enabled
- XML comments on public methods
- xUnit + Moq for unit tests

## License

Sample code provided as-is.
