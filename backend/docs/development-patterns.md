# Backend development patterns

This is the index of canonical implementations for backend work. Inspect the linked source before adding an analogous feature. If an example and a generic framework recommendation differ, follow the example unless the task explicitly changes the architecture.

## REST request binding and authorization

Use the presentation source generator for route, query, body, and authenticated-user binding.

- Generated JSON body and authenticated request: `CoreService/Presentation/Rest/Dtos/CreateForumRequestBody.cs`
- Generated route/query request: `CoreService/Presentation/Rest/Dtos/GetForumRequest.cs`
- Endpoint consuming the generated request: `CoreService/Presentation/Rest/CreateForumAsync.cs`
- Route registration without duplicate authorization metadata: `CoreService/Presentation/Rest/Api.cs`
- Generator implementation and diagnostics: `Shared.Presentation.Generator/Generator.cs` and `Shared.Presentation.Generator/Diagnostics.cs`

Required shape:

```csharp
[Omit(typeof(CreateCommand), PropertyGenerationMode.AsRequired,
    nameof(CreateCommand.CreatedBy))]
public sealed partial class CreateRequestBody;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class CreateRequest
{
    [FromBody] public required CreateRequestBody Body { get; init; }
}
```

The endpoint receives `CreateRequest`; it does not receive `HttpContext`, repeat `[FromBody]`, extract claims manually, or add `.RequireAuthorization()` at mapping time.

The multipart avatar upload in `FileService/Presentation/Rest/UploadAvatarAsync.cs` is the current explicit exception because the generator does not model `IFormFile` input.

## Generic read projection

Database reads expose a caller-selected projection type and project it before materialization.

- Generic query and pass-through handler: `UserService.Application/UseCases/GetUserQuery.cs`
- Generic repository contract: `UserService.Application/Interfaces/IUserReadRepository.cs`
- SQL-side Mapster projection: `UserService/Infrastructure/Persistence/Repositories/UserReadRepository.cs`
- Generated DTO: `UserService.Application/Dtos/UserDto.cs`
- Error-aware projection with access checks: `CoreService/Infrastructure/Persistence/Repositories/ThreadReadRepository.cs`

Required shape:

```csharp
public sealed partial class GetEntityQuery<T> : IQuery<Result<T, EntityNotFoundError>>
    where T : notnull;

public Task<Result<T, EntityNotFoundError>> HandleAsync(
    GetEntityQuery<T> query,
    CancellationToken cancellationToken) =>
    _repository.GetOneAsync<T>(query.EntityId, cancellationToken);
```

The repository applies `ProjectToType<T>()` to `IQueryable` before `FirstOrDefault`, `SingleOrDefault`, or list materialization. Do not replace this with an entity fetch followed by `new SomeDto(...)` in a handler.

Handwritten DTOs remain appropriate for genuinely composed results. Current examples are:

- `CoreService.Application/Dtos/SearchResultsDto.cs`, which represents heterogeneous search results.
- `NotificationService/Application/UseCases/GetInternalNotificationsPagedQuery.cs`, which joins a local notification projection with thread and user DTOs from other bounded contexts.
- `NotificationService/Application/UseCases/GetThreadSubscriptionsPagedQuery.cs`, which joins local subscription identifiers with thread DTOs from CoreService.

Because these exceptions can hide accidental mapping, add new composed DTOs only when a query really joins multiple projections or bounded contexts, and document the reason during review.

## Transactional messaging

Use Wolverine's EF Core transactional outbox through the service unit of work.

- Unit of work: `CoreService/Infrastructure/Persistence/UnitOfWork.cs`
- Wolverine PostgreSQL and EF transaction setup: `CoreService/Program.cs`

The write flow is:

1. Change tracked domain state.
2. Publish the domain event through `IUnitOfWork.PublishEventAsync`.
3. Call `IUnitOfWork.CommitAsync`.
4. Let `IDbContextOutbox<TDbContext>.SaveChangesAndFlushMessagesAsync` atomically persist state and messages.

Do not add a parallel outbox entity/table, dispatcher, or retry worker. Use Wolverine routing, durability, scheduling, and error policies. Introduce a Wolverine saga only when the business process itself has durable state across multiple messages and compensating transitions.

## Scheduled work

Use TickerQ for recurring application jobs.

- Registration: `NotificationService/Infrastructure/DependencyInjection.cs`
- Host integration: `NotificationService/Program.cs`

Do not implement recurrence with `BackgroundService`, `PeriodicTimer`, `System.Threading.Timer`, or a `Task.Delay` loop. `DevEnv.Seeder` is an application-lifetime worker, not a recurring production scheduler, and is the current exception.

## Identity-owned user settings

Keycloak is the source of truth for settings that also affect identity-provider behavior. The current example is the authenticated user's locale:

- Generated REST input: `UserService/Presentation/Rest/Dtos/ChangeCurrentUserLocaleRequestBody.cs`
- Pass-through application command: `UserService.Application/UseCases/ChangeCurrentUserLocaleCommand.cs`
- Typed Keycloak HTTP client: `UserService/Infrastructure/Services/KeycloakUserLocaleClient.cs`

Update these settings synchronously and return the identity-provider failure to the caller. Do not mirror them in a service table or add an outbox, reconciliation job, saga, or fallback value solely to synchronize two copies. Browser-local UI state may cache the exact value for immediate rendering, but it is not an authorization source.

## Persistence and SQL

Use, in order:

1. EF Core LINQ for aggregate writes and ordinary queries.
2. LinqToDB integration for advanced SQL translation, bulk operations, window functions, and table-value constructors.
3. Shared persistence helpers such as `Shared.Infrastructure/Extensions/QueryableExtensions.cs`.
4. Generated EF migrations for schema SQL.

Direct `NpgsqlCommand`, `FromSqlRaw`, `ExecuteSqlRaw`, and equivalent provider-specific SQL do not belong in service application code. If existing abstractions cannot express a required operation, stop and get an explicit architectural decision before adding a shared, tested primitive.

The normal service startup project is the EF tooling entry point. Do not add an `IDesignTimeDbContextFactory` merely to work around an invocation or configuration problem.

## OpenAPI aggregation and caching

The gateway aggregates downstream schemas in `ApiGateway/Infrastructure/Services/OpenApiAggregatorService.cs`. The merged document is cached through the named FusionCache configured in `ApiGateway/Infrastructure/DependencyInjection.cs`.

The cache key includes the gateway module version so a new build cannot reuse a stale merged schema. Fix cache invalidation by versioning or explicit eviction; do not remove aggregation caching.

## Compatibility policy

There is currently no production deployment or legacy contract to preserve. Implement only the current target architecture. Do not add old unprefixed routes, compatibility DTOs, mixed-binary rolling-deployment triggers, fallback locale behavior, or historical-data repair unless the user explicitly introduces that requirement.

## Change checklist

Before completing backend work:

1. Compare the implementation with at least two entries above.
2. Search the diff for manual binding, DTO construction, direct HTTP calls, raw SQL, custom retries/outboxes/schedulers, cache removal, and legacy branches.
3. Run `dotnet format` on changed projects.
4. Run focused tests, then `dotnet build Backend.slnx`.
5. For API changes, run integration contract tests and regenerate the frontend SDK only from a verified fresh OpenAPI document.

Do not turn this checklist into a custom source-code scanner. Enforce rules at their natural boundary: source-generator diagnostics for generated contracts, compiler analyzers for forbidden APIs, focused tests for behavior, and integration tests for HTTP/OpenAPI contracts. Introduce an architecture-testing library only when the repository has stable assembly or namespace dependency rules that justify it.
