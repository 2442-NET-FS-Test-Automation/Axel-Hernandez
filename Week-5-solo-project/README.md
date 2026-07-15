# AutoSupply — Order Fulfillment Service

Minimal API that seeds an auto-parts catalog, accepts a burst of orders, and fulfills them **concurrently** against limited SQL Server inventory without overselling.

Pitch: *this is the service that fulfills our orders — correct under concurrency (never oversells) and faster when run concurrently.*

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

## Endpoints

| Method | Route | Purpose |
|--------|-------|---------|
| POST | `/seed` | Catalog + customers + starting stock |
| GET | `/inventory` | On-hand quantities |
| POST | `/orders/burst` | Create N orders + fulfill in background (202) |
| POST | `/orders/fulfill/{orderId}` | Single-order fulfill (debug) |
| GET | `/orders`, `/orders/{id}` | Inspect orders |
| GET | `/reports/completed-orders` | Count of fulfilled orders |
| GET | `/reports/top-products` | Products ranked by units sold (DESC) |
| GET | `/reports/rank-of/{units}` | Binary search for a unit-total's rank |
| GET | `/products/by-sku/{sku}` | `ConcurrentDictionary` SKU → product id |
| POST | `/benchmark` | Sequential vs concurrent timings + speedup |

## Demo flow (presentation)

1. `POST /seed` — catalog + customers + starting stock
2. `GET /inventory` — note on-hand before the burst
3. `POST /orders/burst` with demand **exceeding** stock, e.g.:

```json
{ "count": 40, "expedited": false }
```

   Returns **202 Accepted** while fulfillment continues in the background.

4. `GET /inventory` / `GET /orders` — prove **no overselling**: on-hand never negative; some orders Fulfilled, rest Backordered.
5. `POST /benchmark` — body `{ "number": 20 }`; reset between runs; read sequential vs concurrent time and **speedup**.
6. Optional: `GET /reports/top-products`, `GET /reports/rank-of/{units}`, `GET /products/by-sku/BRK-001`.

Starting stock after seed: BRK-001=20, FLU-001=15, ENG-001=4, TIR-001=10.

## How concurrency stays correct

- `FulfillBurstAsync` plans expedited-first (`BurstPlanner` / `PriorityQueue`), then runs many `FulfillOrderAsync` calls with `Task.WhenAll`.
- Each order gets its **own** `DbContext` from `IDbContextFactory` (DbContext is not thread-safe).
- Stock decrement is protected by **`InventoryItem.RowVersion`** (EF optimistic concurrency).
- On `DbUpdateConcurrencyException`, `SaveWithRetryAsync` reloads fresh stock, re-checks, and retries (or backorders).

**vs in-memory `lock` / `Interlocked`:** those fit a single-process shared memory model (like the Bank demo). Here inventory lives in SQL shared by many contexts/tasks, so a DB concurrency token + retry is the right fit.

**ACID (one order):** one save path per order keeps the stock change and order status together; isolation/concurrency conflicts are resolved with `RowVersion` instead of overselling.

**Parallelism vs concurrency (one line):** the benchmark fans out independent order tasks so cores can make progress at the same time; correctness still comes from DB concurrency control, not from “running faster.”

## Technique → where it lives

| Technique | Where |
|-----------|--------|
| Minimal API + status codes | `AutoSupply.Api/Program.cs` |
| EF Core model, Fluent API, indexes | `AutoSupply.Data/AutoSupplyDbContext.cs` + entities |
| Migrations + seed | `AutoSupply.Data/Migrations/`, `AutoSupply.Api/Seed/Seeder.cs` |
| `RowVersion` concurrency token | `InventoryItem.RowVersion` + `.IsRowVersion()` |
| Repository behind an interface | `IOrderRepository` / `OrderRepository` in `AutoSupply.Data/Repositories/` |
| Factory | `AutoSupply.Api/Fulfillment/OrderFactory.cs` |
| Concurrent burst + per-order context | `FulfillmentService.FulfillBurstAsync` / `FulfillOrderAsync` |
| Background burst (`Task.Run` + new scope) | `POST /orders/burst` in `Program.cs` |
| `PriorityQueue` (expedited first) | `AutoSupply.Api/Fulfillment/BurstPlanner.cs` |
| `ConcurrentDictionary` SKU → product id | `FulfillmentService` + `GET /products/by-sku/{sku}` |
| Hash lookup (`Dictionary`) | product/qty map in `FulfillOrderAsync` (`requested`) |
| Sorted report + `BinarySearch` | `GET /reports/top-products`, `GET /reports/rank-of/{units}` |
| Custom exception (carries SKU) | `UnknownSkuException` + specific catch in `/products/by-sku` |
| Sequential vs concurrent benchmark | `POST /benchmark` + `Seeder.ResetAndCreateOrderAsync` |
| Serilog structured templates | `Program.cs` startup + `FulfillmentService` |
| `CancellationToken` on shutdown | `IHostApplicationLifetime.ApplicationStopping` passed into background burst |

**Repository usage:** `GET /inventory` calls `GetInventoryAsync`; `POST /orders/burst` calls `GetCustomerAndProductIdsAsync` so those callers depend on `IOrderRepository`, not EF types. Fulfillment still uses `IDbContextFactory` per order for the concurrency / `RowVersion` path.

## Big-O (structures in use)

| Structure | Cost | Why |
|-----------|------|-----|
| `PriorityQueue` enqueue/dequeue | O(log n) | Orders expedited before standard without a full sort each time |
| `ConcurrentDictionary` / `Dictionary` lookup | O(1) average | Fast SKU → id and product → qty re-checks; no linear scan |
| Report sort (`OrderByDescending`) | O(n log n) | Rank products by units sold for the sorted report |
| `Array.BinarySearch` on sorted units | O(log n) | Find a unit-total's rank without scanning the list |
| `Task.WhenAll` over n orders | O(n) tasks | Simple correct parallel fan-out; each task is independent |

## Project layout

- `AutoSupply.Api` — endpoints, seed, fulfillment, exceptions
- `AutoSupply.Data` — entities, DbContext, migrations, repositories (`IOrderRepository` / `OrderRepository`)

## Optional polish

- `Log.CloseAndFlush` on shutdown
- Mixed expedited flag in a single burst for a clearer PriorityQueue live demo
- Optional `AddOrdersAsync` on the repository so burst can drop its remaining `DbContext` inject for `SaveChanges`
