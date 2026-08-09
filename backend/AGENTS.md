# Backend engineering contract

These instructions apply to every file under `backend/`.

## Required reconnaissance

- Read `docs/development-patterns.md` before designing or changing backend code.
- Before implementation, find at least two analogous implementations in the repository and follow the established local pattern. Repository conventions take precedence over generic framework examples.
- Check the existing generators and infrastructure before adding code: `Shared.TypeGenerator`, `Shared.Presentation.Generator`, `Shared.Infrastructure.Generator`, Mapster projections, Wolverine, TickerQ, FusionCache, EF Core, and LinqToDB.
- Do not introduce a manual binder, DTO mapper, REST request model, retry loop, scheduler, outbox, cache replacement, raw SQL operation, or design-time factory until the existing project capability has been ruled out.

## REST API

- Model JSON REST input in `Presentation/Rest/Dtos` with a `partial` request type annotated with `[GenerateBind(...)]`.
- Use `AuthorizeMode` on `[GenerateBind]` as the endpoint authorization source. Do not manually extract claims from `HttpContext` or add `.RequireAuthorization()` to ordinary JSON endpoint mappings.
- Generate request bodies with `[Include]` or `[Omit]` when they are a projection of an existing command, query, entity, or value type.
- Endpoint methods receive one generated request object, injected services, and a `CancellationToken`. Binding attributes belong on the generated request properties, not directly on endpoint parameters.
- Keep route registration in the service's `Presentation/Rest/Api.cs` and verify the generated OpenAPI request and response contract with an integration test.
- The current multipart avatar endpoints in `FileService` are the only documented manual-binding exception because `GenerateBind` does not support their form-file contract. New exceptions require an explicit user decision and a documented reason.

## Queries, DTOs, and persistence

- For database-backed reads where the caller selects the response shape, use a generic query/handler/repository pipeline (`Query<T>`) and project in SQL with Mapster's `ProjectToType<T>()`.
- Query handlers must not materialize an entity and manually construct a DTO. They should pass the requested projection type to the repository and return its typed result directly.
- Use `Shared.TypeGenerator` for DTOs that include or omit properties from existing types. Handwritten DTOs are reserved for genuinely composed shapes that do not map to one source type.
- Prefer EF Core, LinqToDB, the shared query extensions, and the repository conventions. Direct provider commands and raw SQL are limited to generated migrations or an established shared persistence primitive.
- Register repositories through `AddRepository<TRepository, TImplementation>()`. In non-production environments the shared DbContext configuration uses this boundary to add the repository method to EF Core and LinqToDB command text automatically; do not add query tags manually. Disable diagnostics for an exceptional repository with `enableCallDiagnostics: false` at registration.
- Generate migrations with EF tooling. Do not hand-edit model snapshots.
- Do not add `IDesignTimeDbContextFactory` implementations while the normal startup project can construct the context. A factory requires a demonstrated tooling failure and explicit approval.

## Messaging and scheduled work

- Use Wolverine's EF Core integration and `IDbContextOutbox<TDbContext>` for transactional messaging. Publish through the unit of work and commit with `SaveChangesAndFlushMessagesAsync`.
- Configure Wolverine persistence with `PersistMessagesWithPostgresql` and `UseEntityFrameworkCoreTransactions`; use Wolverine policies for durable inbox/outbox and retries.
- Do not create custom outbox tables, dispatch loops, retry queues, or polling workers.
- A Wolverine saga is for a genuinely stateful, multi-message business workflow. It is not a replacement for the transactional outbox around a single durable side effect.
- Use TickerQ for recurring or scheduled application work. Do not add a custom `BackgroundService`, timer, or delay loop for scheduling.

## Infrastructure and compatibility

- Preserve existing cross-cutting behavior, including API localization, Problem Details, authentication, observability, and FusionCache-backed OpenAPI aggregation. Do not remove or bypass caching as an incidental fix.
- Treat Keycloak as the sole persistent source for identity-owned settings such as authenticated-user locale. Do not add a service-side mirror, messaging workflow, saga, or reconciliation job for those settings.
- There is no production deployment to preserve. Do not add legacy routes, mixed-version deployment behavior, historical-data fallbacks, compatibility shims, or reconciliation for hypothetical production state unless explicitly requested.
- Prefer deleting superseded code over keeping legacy and fallback paths.

## Required verification

- Run `dotnet format` for changed projects, the relevant focused tests, and `dotnet build Backend.slnx`.
- For API contract changes, run the relevant integration tests and verify the generated OpenAPI document before regenerating the frontend client.
- Run `git diff --check` and audit the final diff for new manual mapping, binding, HTTP, retry, scheduling, outbox, SQL, caching, and compatibility code.
- Do not create a custom source-scanning architecture-test suite to enforce these process conventions. Prefer existing generator diagnostics, compiler analyzers, or focused behavioral tests. Adding a general architecture-testing dependency requires a stable assembly-level invariant and an explicit decision.
- Do not edit generated files under `obj/` or generated frontend SDK output by hand.
- Any exception to this contract must be stated explicitly in the final response and documented in `docs/development-patterns.md` when it becomes an accepted project pattern.
