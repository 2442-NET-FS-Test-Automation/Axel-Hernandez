# AutoSupply — Order Fulfillment Service

Minimal API that seeds an auto-parts catalog, accepts a burst of orders, and fulfills them **concurrently** against limited SQL Server inventory without overselling.

## Stack

- .NET Minimal API
- EF Core (code-first) + SQL Server (Docker)
- Serilog (console + rolling file under `AutoSupply.Api/logs/`)

## Run

1. Start SQL Server on `localhost,1434` (same connection string as in `Program.cs`).
2. Apply migrations if needed:  
   `dotnet ef database update --project AutoSupply.Data --startup-project AutoSupply.Api`
3. Run the API:  
   `dotnet run --project AutoSupply.Api`

## Demo flow

1. `POST /seed` — catalog + customers + starting stock  
2. `GET /inventory` — on-hand quantities  
3. `POST /orders/burst` — body example:

```json
{ "count": 40, "expedited": false }
```

Returns **202 Accepted** while fulfillment continues in the background.

4. `GET /orders` / `GET /inventory` — statuses become Fulfilled or Backordered; stock never goes below zero.

Optional: `POST /orders/fulfill/{orderId}` fulfills a single order (useful for debugging).

## How concurrency stays correct

- `FulfillBurstAsync` runs many `FulfillOrderAsync` calls with `Task.WhenAll`.
- Each order gets its **own** `DbContext` from `IDbContextFactory` (DbContext is not thread-safe).
- Stock decrement is protected by **`InventoryItem.RowVersion`** (EF optimistic concurrency).
- On `DbUpdateConcurrencyException`, `SaveWithRetryAsync` reloads fresh stock, re-checks, and retries (or backorders).

**vs in-memory `lock` / `Interlocked`:** those fit a single-process shared memory model (like the Bank demo). Here inventory lives in SQL shared by many contexts/tasks, so a DB concurrency token + retry is the right fit.

**ACID (one order):** one save path per order keeps the stock change and order status together; isolation/concurrency conflicts are resolved with `RowVersion` instead of overselling.

## Technique → where it lives

| Technique | Where |
|-----------|--------|
| Minimal API + status codes | `AutoSupply.Api/Program.cs` |
| EF Core model, Fluent API, indexes | `AutoSupply.Data/AutoSupplyDbContext.cs` + entities |
| Migrations + seed | `AutoSupply.Data/Migrations/`, `AutoSupply.Api/Seed/Seeder.cs` |
| `RowVersion` concurrency token | `InventoryItem.RowVersion` + `.IsRowVersion()` |
| Factory | `AutoSupply.Api/Fulfillment/OrderFactory.cs` |
| Concurrent burst + per-order context | `FulfillmentService.FulfillBurstAsync` / `FulfillOrderAsync` |
| Background burst (`Task.Run` + new scope) | `POST /orders/burst` in `Program.cs` |
| `PriorityQueue` (expedited first) | `AutoSupply.Api/Fulfillment/BurstPlanner.cs` |
| Hash lookup (`Dictionary`) | product/qty map in `FulfillOrderAsync` (`requested`) |
| Serilog structured templates | `Program.cs` startup + `FulfillmentService` |
| `CancellationToken` on shutdown | `IHostApplicationLifetime.ApplicationStopping` passed into background burst |

## Big-O (structures in use)

| Structure | Cost | Why |
|-----------|------|-----|
| `PriorityQueue` enqueue/dequeue | O(log n) | Orders expedited before standard without a full sort each time |
| `Dictionary` product → qty | O(1) average | Fast re-check during concurrency retry |
| `Task.WhenAll` over n orders | O(n) tasks | Simple correct parallel fan-out; each task is independent |

## Project layout

- `AutoSupply.Api` — endpoints, seed, fulfillment
- `AutoSupply.Data` — entities, DbContext, migrations

## Still to finish (Target / Floor polish)

- Real report endpoint(s) (e.g. top products)
- `POST /benchmark` (sequential vs parallel + inventory reset)
- README updates when repository / custom exception / binary-search report are added
