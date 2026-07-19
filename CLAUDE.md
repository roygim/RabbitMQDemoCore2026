# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```powershell
# Build the whole solution
dotnet build RabbitMQDemoCore2026.slnx

# Start RabbitMQ (required before running anything)
docker compose -f RabbitMQDemoCore2026.Api/Docker/docker-compose.yml up -d

# Run the API (http://localhost:5235, Swagger UI at /swagger in Development)
dotnet run --project RabbitMQDemoCore2026.Api

# Run the consumer worker (separate process, run alongside the API)
dotnet run --project RabbitMQDemoCore2026.Worker
```

RabbitMQ Management UI: http://localhost:15672 (admin / admin123 — same credentials as in `appsettings.Development.json` of both runnable projects).

There are no test projects in this solution.

## Architecture

.NET 10 demo of a RabbitMQ producer/consumer pipeline with retry + dead-letter handling. Two runnable processes communicate only through RabbitMQ:

**RabbitMQDemoCore2026.Api** (producer) — `POST /api/products/add` → `ProductService` builds a `ProductCreatedEvent` → `ProductsProducer` → `RabbitMqProducer` publishes JSON to the `products.exchange` topic exchange with routing key `product.created`. The API returns 202 Accepted; nothing is persisted.

**RabbitMQDemoCore2026.Worker** (consumer) — `ProductConsumerWork` (a `BackgroundService`) declares the entire broker topology on startup and consumes from the main queue:

- `products.exchange` (topic) → `products.db.queue`, bound to `product.*`
- On processing failure the message is republished to `products.retry.exchange` → `products_retry_queue`, which has a 10s TTL and dead-letters back to `products.exchange` with routing key `product.retry` (so it re-enters the main queue)
- Retry attempts are counted via the `x-retry-count` message header; after `MaxAttempts` (3 total attempts) the message goes to `products.dlx` → `products_dead_queue`
- All acks are manual; failures are always acked after being rerouted (no nack/requeue)

The consumer currently throws an intentional exception ("DB failed intentionally") to demonstrate the retry/DLQ flow — every message ends up in the DLQ by design. Real DB persistence is a marked TODO at that spot.

**RabbitMQDemoCore2026.Domain** — shared contracts: `ProductCreatedEvent` (extends `EventBase`), `CreateProductRequest`, and routing-key constants in `ProductEventNames`.

**RabbitMQDemoCore2026.Infrastructure** — only `RabbitMqOptions`, bound from the `RabbitMQ` config section in both Api and Worker.

### Things to keep in sync

- Exchange/queue names are duplicated as literals: `"products.exchange"` appears both in the Api's `ProductsProducer` and as constants in the Worker's `ProductConsumerWork`. Renaming topology requires touching both projects.
- The Api and Worker each create their own RabbitMQ connection independently (`RabbitMqProducer` in the Api, `RabbitMqConnection` in the Worker); `Port` and `VirtualHost` from `RabbitMqOptions` are currently commented out in both connection factories, so only HostName/UserName/Password take effect.
- Namespaces in the Api project are inconsistent: controllers/producers live under `RabbitMQDemoCore2026.Controllers` / `RabbitMQDemoCore2026.Repositories`, while interfaces/services use `RabbitMQDemoCore2026.Api.*`. Match the namespace of the folder you're editing rather than assuming the project-name prefix.
