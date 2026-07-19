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

The user (Roei) typically builds and runs from Visual Studio himself — prefer explaining changes and reviewing over running builds.

## Architecture

.NET 10 demo of a RabbitMQ producer/consumer pipeline with retry + dead-letter handling, built as a learning project with deliberate architectural layering. Two runnable processes communicate only through RabbitMQ:

**RabbitMQDemoCore2026.Api** (producer) — REST endpoints for product Create/Update/Delete (`POST /api/products/add`, `PUT /api/products/update/{id}`, `DELETE /api/products/delete/{id}`). Flow: `ProductsController` → `ProductService` (translates HTTP requests into immutable events) → `ProductsProducer` → `RabbitMqProducer` publishes JSON to the `products.exchange` topic exchange with routing keys `product.created` / `product.updated` / `product.deleted`. Endpoints return 202 Accepted; nothing is persisted in the API.

**RabbitMQDemoCore2026.Worker** (consumer) — `Consumers/ProductsDbConsumer` (a `BackgroundService`) declares the entire broker topology on startup, consumes `products.db.queue`, and dispatches by routing key to per-event handlers in `Handlers/` (`ProductCreatedHandler` / `ProductUpdatedHandler` / `ProductDeletedHandler`). Handlers hold the business logic and know nothing about RabbitMQ; the consumer owns transport concerns only (ack, retry, DLQ). Handlers are registered **Scoped** and resolved via `IServiceScopeFactory` with one scope per message — this is deliberate prep for a scoped `DbContext`; don't inject handlers directly into the consumer (captive dependency).

Retry/DLQ flow:
- `products.exchange` (topic) → `products.db.queue`, bound to `product.*`
- On handler failure the message is republished to `products.retry.exchange` → `products_retry_queue` (10s TTL), which dead-letters back into `products.exchange` with routing key `product.retry`
- Because retried messages arrive with routing key `product.retry`, the original key is preserved in the `x-original-routing-key` header (set on first failure only) and `GetOriginalRoutingKey` reads it back for dispatch — string headers come back as `byte[]` and must be UTF8-decoded
- Attempts are counted via the `x-retry-count` header; after `MaxAttempts` (3 total) the message goes to `products.dlx` → `products_dead_queue`, with all headers copied so the DLQ retains diagnostic history
- All acks are manual; failures are always acked after being rerouted (no nack/requeue)
- Unknown routing keys are logged and acked (not retried — retry is for transient failures only)

All handlers currently throw an intentional exception ("DB failed intentionally") to demonstrate the retry/DLQ flow — every message ends up in the DLQ by design. Real DB persistence is a marked TODO in each handler; `ProductId` for Create is a random placeholder until then.

**RabbitMQDemoCore2026.Domain** — shared contracts: events (`ProductCreatedEvent` / `ProductUpdatedEvent` / `ProductDeletedEvent`, all inheriting `EventBase` and immutable via `init`), request models, and routing-key constants in `ProductEventNames`. Events are full snapshots, not deltas; keep new events `init`-only.

**RabbitMQDemoCore2026.Infrastructure** — `RabbitMqOptions` (bound from the `RabbitMQ` config section) and `Messaging/RabbitMqTopology` — the single source of truth for all exchange/queue/routing-key names, used by both Api and Worker. Never hardcode topology names; add new ones here.

### Conventions and gotchas

- The Api and Worker each create their own RabbitMQ connection (`RabbitMqProducer` in the Api, `RabbitMqConnection` in the Worker); `Port` and `VirtualHost` from `RabbitMqOptions` are commented out in both connection factories, so only HostName/UserName/Password take effect.
- Namespaces in the Api project are inconsistent: controllers/producers live under `RabbitMQDemoCore2026.Controllers` / `RabbitMQDemoCore2026.Repositories`, while interfaces/services use `RabbitMQDemoCore2026.Api.*`. This compiles because of `Api/GlobalUsings.cs` (global usings for `RabbitMQDemoCore2026.Interfaces` and `.Repositories`). Match the namespace of the folder you're editing.
- Adding a new event type requires touching: `Domain/Events`, `ProductEventNames`, `IProductsProducer` + `ProductsProducer`, `IProductService` + `ProductService`, `ProductsController`, a new handler in `Worker/Handlers`, an `AddScoped` registration in Worker `Program.cs`, and a `case` in the `ProductsDbConsumer` switch. Missing the DI registration compiles fine but sends every such message to the DLQ at runtime via `GetRequiredService` failure.
- Renaming queues in `RabbitMqTopology` creates new queues on the broker; old ones (and their messages) linger until deleted via the Management UI. Changing arguments of an existing queue (e.g. the retry queue's TTL) causes a PRECONDITION_FAILED on declare — delete the queue first.
