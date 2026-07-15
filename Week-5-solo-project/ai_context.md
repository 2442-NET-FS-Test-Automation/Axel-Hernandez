# AI Context — AutoSupply Week-5 Solo Project

Use this file when continuing work in a new chat. Prefer this over rediscovering from scratch.

---

## Who & how to collaborate

- **Learner:** Axel (SWE internship / Revature). Building this to understand every line — not to have code dumped wholesale.
- **Mode preference:** Ask/coach by default when learning; Agent only when explicitly asked (e.g. README, this file).
- **Coaching style (required):**
  1. One small step at a time (file → tiny snippet → explain why → wait for “done”).
  2. Explain **why / for what**, not only what to type.
  3. Do **not** paste whole finished files unless asked.
  4. Mirror patterns from the **trainer demo** and the **project criteria** — avoid inventing new techniques, libraries, or architectures not seen there.
  5. Stick to Week 1–4 taught stack: C#, OOP, EF Core, Minimal API, Serilog, multithreading, basic DSA.

---

## Project identity

| Item | Value |
|------|--------|
| **App** | AutoSupply — Order Fulfillment Service (auto-parts domain) |
| **Path** | `Week-5-solo-project/` under Axel’s training repo |
| **Stack** | .NET Minimal API · EF Core code-first · SQL Server Docker · Serilog |
| **Solution** | `AutoSupply.slnx` → `AutoSupply.Api` + `AutoSupply.Data` |
| **SQL** | `localhost,1434` · DB `AutoSupplyDb` · sa / `TestPass1!` (see `Program.cs`) |
| **Criteria** | [`order-fulfillment-engine-v2.md`](order-fulfillment-engine-v2.md) |
| **Demo to mirror** | `/Users/macbookpro/Desktop/revature_training/trainer-code/Demos/EF-REST-SOAP-ASP/library-api-minimal` especially `Library.Api` |

Domain translation: Library “title/copies” → AutoSupply **Product / InventoryItem.QuantityOnHand**; orders compete for limited stock.

---

## Architecture (current)

```
POST /orders/burst
  → create Pending orders (OrderFactory)
  → Task.Run + new DI scope
  → FulfillBurstAsync (BurstPlanner PriorityQueue → Task.WhenAll)
  → each FulfillOrderAsync: own DbContext, stock check, RowVersion retry

DB entities: Customer, Product, Category, InventoryItem (+ RowVersion),
             Order, OrderLine, FulfillmentEvent
```

**Not used (and generally shouldn’t invent):** `InventoryRepository` from ControllerApi — that is inventory CRUD for `Library.ControllerApi`, not Minimal API fulfillment. Demo fulfillment uses `IDbContextFactory` inside services.

---

## Key files

| Path | Role |
|------|------|
| `AutoSupply.Api/Program.cs` | DI, Serilog setup, all Minimal API endpoints |
| `AutoSupply.Api/Fulfillment/FulfilmentService.cs` | `FulfillOrderAsync`, `FulfillBurstAsync`, `SaveWithRetryAsync`, SKU cache |
| `AutoSupply.Api/Fulfillment/IFulfillmentService.cs` | Interface + `FulfillmentResult` / `BurstResult` |
| `AutoSupply.Api/Fulfillment/OrderFactory.cs` | Builds Order + OrderLine |
| `AutoSupply.Api/Fulfillment/BurstPlanner.cs` | `PriorityQueue` — Expedited before Standard |
| `AutoSupply.Api/Seed/Seeder.cs` | Catalog seed + `ResetAndCreateOrderAsync` for benchmark |
| `AutoSupply.Api/Exceptions/UnknownSkuException.cs` | Custom exception carrying `Sku` |
| `AutoSupply.Data/AutoSupplyDbContext.cs` | Fluent API, indexes, RowVersion |
| `README.md` | Technique → file map + Big-O notes |

Note: filename is often `FulfilmentService.cs` (British spelling) while type is `FulfillmentService`.

---

## Seed catalog (starting stock)

Used by seed and benchmark reset (`ResetAndCreateOrderAsync`):

| SKU | Start qty |
|-----|-----------|
| BRK-001 | 20 |
| FLU-001 | 15 |
| ENG-001 | 4 |
| TIR-001 | 10 |

---

## Endpoints (as of last session)

| Method | Route | Notes |
|--------|-------|--------|
| POST | `/seed` | Catalog + customers |
| GET | `/inventory` | On-hand quantities |
| POST | `/orders/burst` | Body `BurstOrderRequest` (`Count`, `Expedited`); 202 + background fulfill |
| POST | `/orders/fulfill/{orderId}` | Single-order debug fulfill |
| GET | `/orders`, `/orders/{id}` | Inspect |
| GET | `/reports/completed-orders` | Floor-style count of fulfilled |
| GET | `/reports/top-products` | Ranked by units sold (DESC) |
| GET | `/reports/rank-of/{units}` | `Array.BinarySearch` on sorted units (demo style) |
| GET | `/products/by-sku/{sku}` | ConcurrentDictionary lookup; catches `UnknownSkuException` |
| POST | `/benchmark` | Body `BenchmarkRequest` (`Number`); seq vs concurrent + timings |

---

## Criteria progress

### Floor — largely done

- [x] EF model, migrations, seed, DI
- [x] `/seed`, `/orders/burst`, `/inventory`, one+ reports
- [x] Concurrent fulfill, per-order DbContext, RowVersion retry, no oversell
- [x] Background `Task` on burst
- [x] Serilog structured logs in fulfillment
- [x] README technique map

### Target — mostly done; one gap

- [x] `PriorityQueue` via `BurstPlanner`
- [x] Benchmark: reset between runs, sequential vs `FulfillBurstAsync`, timings (confirm **speedup** still in JSON if presenting)
- [x] Sorted report + binary search (`top-products` + `rank-of/{units}`)
- [x] `ConcurrentDictionary` SKU → product id + `ResolveProductId`
- [x] Factory (`OrderFactory`)
- [x] Custom exception with data + specific catch
- [ ] **Repository behind an interface** — still outstanding if aiming full Target
- [ ] Optional polish: mixed expedited in `/orders/burst` for PriorityQueue demo; `Log.CloseAndFlush` on shutdown; README Big-O update for reports/cache

### Stretch

- Skip unless Target is green and time permits (worker pool, etc.).

---

## Important design decisions already made

1. **Fulfill one correctly first**, then burst wraps it (`Task.WhenAll`) — never rewrite fulfill logic in the endpoint.
2. **`RowVersion` on `InventoryItem` only** — optimistic concurrency for stock races.
3. **`SaveWithRetryAsync`** on fulfilled path; on retry failure → clear tracker, mark Backordered, **must** `SaveChanges`.
4. Benchmark prep lives in **`Seeder.ResetAndCreateOrderAsync`** (reset stock + create N orders + return ids) — not in `InventoryRepository`.
5. Burst background work uses **`IServiceScopeFactory`** + new scope inside `Task.Run` (request scope dies after 202).
6. **Do not** chase ControllerApi patterns for this Minimal API project.
7. Rank-by-`units` was kept to match demo BinarySearch teaching; productId rank is nicer UX but different API.

---

## Pitfalls already hit (don’t repeat)

- Codex left untracked folders (`Reports/`, `Repositories/`, etc.) — cleaned earlier; avoid re-applying stash blindly.
- `OrderFactory` must stay registered in DI if burst still injects it.
- Migration “RowVersion already exists”: empty `Up()` or fix DB/history sync — column is concurrency token, **not** unique.
- `FindIndex` is on **List**, not array (`ToArrayAsync` → use `Array.FindIndex` or `ToListAsync`).
- Constructor name must match class name (`UnknownSkuException`).
- ConcurrentDictionary cache is filled at **service construction** — restart API after seed if cache was empty at startup.
- Don’t put inventory truth only in memory; SQL + RowVersion remains source of truth.

---

## How to continue in a new chat

1. Read this file + skim `order-fulfillment-engine-v2.md` Target section.
2. Ask Axel what they want next (likely: **repository**, README refresh, or presentation rehearsal).
3. If implementing: **one step + why**, wait for confirmation, then next step.
4. Prefer demo files under `library-api-minimal/Library.Api/` when showing patterns.

### Suggested next work (priority)

1. **`IOrderRepository` / thin repository** for create-burst + reset inventory (Target checkbox) — or argue Seeder already centralizes prep; if implementing, keep it small and demo-aligned.
2. Refresh **README** with ConcurrentDictionary, exception, reports, benchmark speedup.
3. Ensure benchmark response includes **speedup factor**.
4. Presentation dry-run: seed → burst oversell proof → inventory → benchmark → top-products / rank-of.

---

## Voice / product wording for demos

Pitch: *“This is the service that fulfills our orders — correct under concurrency (never oversells) and faster when run concurrently.”*

Prove: on-hand never negative; units fulfilled align with stock drawn down; parallel benchmark usually beats sequential after fair reset.
