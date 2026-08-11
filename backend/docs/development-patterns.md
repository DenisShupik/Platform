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

## Local type aliases

Use file-local `using` aliases to give a concise contextual name to an existing type. This is a compile-time readability tool: it does not introduce a proxy type, change serialization, or create another mapping boundary.

Name every Minimal API response type `Response`, including endpoints with a single result type:

```csharp
using Response = Ok<IReadOnlyList<UserDto>>;

public static async Task<Response> GetUsersPagedAsync(...)
```

For multiple outcomes, alias `Results<T1, T2, ...>`. Include only outcomes that the handler can actually return; do not add speculative `BadRequest`, `NotFound`, or other branches solely for documentation. Shared Problem Details responses are added by the OpenAPI contract transformer.

Use the same pattern for closed application result types that are repeated in a command/query declaration and its handler:

```csharp
using CommandResult = Result<PostId, ThreadNotFoundError, PermissionDeniedError>;

public sealed record CreatePostCommand : ICommand<CommandResult>;
public sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, CommandResult>;
```

Aliases also resolve genuine name collisions or ambiguity, for example `Thread = CoreService.Domain.Entities.Thread`, `ThreadState = CoreService.Domain.Enums.ThreadState`, and `Index = Shared.Domain.ValueObjects.Index`.

Alias naming conventions:

- Use PascalCase.
- Use `Response` only for the complete HTTP result type of a REST endpoint.
- Use `CommandResult` and `QueryResult` for the complete result type of an application command or query.
- Use `<UseCaseName>Result` only when a scope contains multiple result aliases that would otherwise be ambiguous.
- For a CLR name collision, preserve the domain type's natural short name (`Thread`, `ThreadState`, `Index`) instead of adding `Alias`, `Type`, or an infrastructure-oriented suffix.
- Keep aliases file-local; do not introduce `global using` aliases for feature contracts.

Do not create a wrapper class or DTO merely to shorten a generic type. Keep a real named type when it owns contract fields, validation, behavior, serialization identity, or domain meaning; an alias is not a replacement for such a type.

## Bulk REST lookups

Use the literal `bulk` route segment for an operation that accepts a collection of resource identifiers at the same route depth as a singular resource operation.

- Canonical routes: `api/forums/bulk/{forumIds}`, `api/categories/bulk/{categoryIds}`, `api/threads/bulk/{threadIds}`, and `api/users/bulk/{userIds}`.
- Keep singular operations on `api/{resources}/{resourceId}` and collection lookups on `api/{resources}/bulk/{resourceIds}`.
- Do not try to distinguish two path templates only by changing a parameter name, such as `{postId}` versus `{postIds}`. OpenAPI treats templates with the same hierarchy as identical.

For example, bookmark membership lookup uses `GET api/posts/bookmarks/bulk/{postIds}`, while creation and deletion use `POST` and `DELETE api/posts/bookmarks/{postId}`.

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

Use TickerQ when a service actually has recurring application jobs. `NotificationService` currently has no ticker
functions, so it must not register or host TickerQ merely for future use.

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

Register repositories with `AddRepository<TRepository, TImplementation>()`. When repository-call diagnostics are enabled in `RegisterDbContexts`, the shared repository proxy establishes an async-local `Repository.Method` scope and the shared command interceptor adds it to command text for both EF Core and LinqToDB. This keeps diagnostics out of query composition and covers every SQL command executed during the repository call.

Services pass `enableRepositoryCallDiagnostics: !builder.Environment.IsProduction()` to `RegisterDbContexts`. `AddRepository` resolves that choice while building the service collection: when diagnostics are disabled it emits the ordinary direct `AddScoped<TRepository, TImplementation>()` registration, so production has no proxy, factory lookup, async-local scope, or command interceptor. For a deliberate non-production exception, pass `enableCallDiagnostics: false` to the individual `AddRepository` registration. Methods that only mutate the change tracker emit SQL later in the unit of work and are not attributed to the earlier repository call.

The normal service startup project is the EF tooling entry point. Do not add an `IDesignTimeDbContextFactory` merely to work around an invocation or configuration problem.

## OpenAPI aggregation and caching

The gateway aggregates downstream schemas in `ApiGateway/Infrastructure/Services/OpenApiAggregatorService.cs`. The merged document is cached through the named FusionCache configured in `ApiGateway/Infrastructure/DependencyInjection.cs`.

The cache key includes the gateway module version and a deterministic fingerprint of the configured downstream OpenAPI sources. A new gateway build or proxy topology change therefore cannot reuse a stale merged schema. Fix cache invalidation by versioning or explicit eviction; do not remove aggregation caching.

Load one deterministic OpenAPI source per reverse-proxy cluster. Replicas within a cluster must expose the same contract. Path collisions are configuration errors and must fail aggregation. Shared component names may be reused only when their serialized definitions are identical; never resolve path or component collisions with last-write-wins behavior.

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
