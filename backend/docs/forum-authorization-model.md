# Forum authorization model

Status: implemented baseline for the current forum use cases.

## Decisions

1. A public principal is a user. Forum code does not classify it as a human, bot, or service.
2. Keycloak authenticates the user. The JWT supplies identity, not forum or platform privileges.
3. CoreService owns authorization decisions and evaluates them from its current domain data.
4. Internal workloads use a separate internal authentication policy. When a workload performs an action on behalf of a user, internal gRPC carries only that user's ID; the receiving service evaluates its own policies.
5. Policies name business decisions, capabilities are stable atomic rights, and grants assign capabilities to a user at a resource scope.
6. Reviewed capability bundles are created and revoked by explicit UseCases. There is no generic role editor and no caller-supplied permission list.

## Actor context

`ActorContext(UserId)` contains only the identity needed by application policies. The HTTP adapter builds it from the trusted subject claim. The internal gRPC adapter builds it from `RequestedByActor.UserId` after the workload itself has passed the internal authentication policy.

CoreService does not call UserService to resolve a role while constructing an actor. UserService is consulted only by appointment workflows that must reject an unknown target user. A workload without a delegated actor cannot invoke a user-authorized UseCase by fabricating forum privileges.

Changing an appointment takes effect on the next policy evaluation. It does not require issuing another access token.

## Policies and capabilities

`ForumPolicy` names the action a UseCase authorizes:

| Policy | Stored capability |
| --- | --- |
| `ManageStructure` | `ManageStructure` |
| `ViewUnpublishedThreads` | `ViewUnpublishedThreads` |
| `ApproveThread` | `ApproveThreads` |
| `RejectThread` | `RejectThreads` |
| `EditAnyPost` | `EditAnyPost` |
| `DeleteAnyPost` | `DeleteAnyPost` |
| `ManageAuthorization` | `ManageAuthorization` |

The evaluator returns the project's `SuccessOr<PermissionDeniedError>`. It does not introduce a parallel authorization-result abstraction.

`CapabilityCode` is a closed `short` enum with explicit append-only numeric values. It has no independent invariant that would justify a value object. `AuthorizationAssignmentId` and `CapabilityGrantId` are Vogen identifier value objects. `AuthorizationScope` is an immutable composite value object with factories for valid platform, forum, and category scopes.

## Grants and scope inheritance

`CapabilityGrant` is an auditable entity containing the assignment and grant IDs, target user, capability, scope, source, issuer, issue and optional expiry times, and optional revocation audit.

Scope inheritance is additive and downward:

```text
Platform
└── Forum
    └── Category
        └── Thread / Post resource predicates
```

Ownership and aggregate state remain domain predicates rather than ACL rows. For example, an author may edit their own eligible post, while editing another user's post requires `EditAnyPost`. The aggregate receives the semantic decision, not a role or an infrastructure policy object.

The database enforces valid scope combinations, enum values, validity intervals, issuer rules, and complete revocation audit. Filtered unique indexes prevent duplicate active appointment capabilities while allowing a new appointment after revocation.

## Implemented appointments

### Platform administration

“Platform administrator” is the name of an operational appointment, not a JWT role and not a policy bypass. `AppointPlatformAdministrator` creates an explicitly reviewed bundle of the seven currently supported platform-scoped capabilities. A future capability is not added automatically merely because it appears in the enum.

The first platform capability bundle is granted through an internal gRPC operation authenticated with a service token. The service identity authorizes the internal call; it is not modelled as a domain user and is not written into the user issuer field. Bootstrap grants record their dedicated source. Once any active platform appointment exists, the operation is an idempotent no-op. Normal appointments require `ManageAuthorization`, verify that the target user exists, and record the issuing user. The last active platform administrator cannot be revoked.

`DevEnv: WithSeeding` creates `admin` through the normal identity-management flow, receives its generated `UserId`, and passes that identifier to CoreService through the internal service-authenticated gRPC operation. Keycloak contains neither a fixed administrator identity nor forum roles.

REST operations:

- `GET /api/authorization/platform/allowed-actions`;
- `GET /api/authorization/platform/administrators`;
- `POST /api/authorization/platform/administrators/{userId}`;
- `DELETE /api/authorization/platform/administrators/{userId}`.

### Category moderation

`AppointCategoryModerator` creates a category-scoped bundle containing `ViewUnpublishedThreads`, `ApproveThreads`, `RejectThreads`, `EditAnyPost`, and `DeleteAnyPost`. It deliberately does not grant structure or authorization management. The appointment may expire and is revoked as one lifecycle.

REST operations:

- `GET /api/categories/{categoryId}/allowed-actions`;
- `GET /api/categories/{categoryId}/moderators`;
- `POST /api/categories/{categoryId}/moderators/{userId}`;
- `DELETE /api/categories/{categoryId}/moderators/{userId}`.

`GET /api/forums/{forumId}/allowed-actions` exposes current forum-level decisions for server-rendered UI. Frontend visibility is derived from these `allowed-actions` responses, but every command still authorizes again inside its UseCase.

## Boundaries

For a capability check, CoreService loads active grants for the actor at platform, parent forum, or exact category scope, maps the requested policy to a stable capability code, and then lets the aggregate enforce ownership and state invariants.

Unpublished-thread read specifications apply the same active scoped grants in SQL, so REST, search, bookmarks, and internal notification lookups do not diverge.

`CoreService.Infrastructure.Grpc.Client` remains the low-level internal transport SDK. A consuming bounded context owns its application port and maps gRPC responses in its infrastructure adapter. Internal workload credentials never become forum users and never receive forum grants.

Fresh Keycloak configuration has no platform or forum application roles. Its remaining role-related protocol scopes and the `realm-management/manage-users` service-account mapping are Keycloak infrastructure concerns, not application authorization.

## Persistence

Migration `AddCapabilityGrants` creates `core_service.capability_grants`, foreign keys, generated enum checks, composite invariant checks, lookup indexes, and active-appointment uniqueness constraints. Authorization discriminators are numeric enums; no capability or scope is persisted as free-form text.

`EFCore.CheckConstraints` generates enum checks. Composite row invariants use column identifiers obtained from typed property expressions and the configured PostgreSQL name translator, so a CLR property rename cannot silently leave a stale identifier in these constraints.

## Deliberate next extensions

The kernel supports direct grants and additional scopes, but a generic grant-management API is intentionally not exposed. New administrative workflows should first be named and reviewed as domain UseCases with explicit capability bundles.

Groups, typed disciplinary restrictions, an authorization-explain endpoint, and a versioned authorization cache are separate increments. They must not reintroduce ordered roles or arbitrary client-supplied permission lists.

## References

- [ASP.NET Core policy-based authorization](https://learn.microsoft.com/aspnet/core/security/authorization/policies)
- [ASP.NET Core resource-based authorization](https://learn.microsoft.com/aspnet/core/security/authorization/resourcebased)
- [Discourse category moderators](https://meta.discourse.org/t/understanding-user-statuses-roles-and-permissions/35171)
- [XenForo groups and scoped permissions](https://docs.xenforo.com/manual/access-privileges/groups-permissions)
- [phpBB permission roles](https://www.phpbb.com/support/docs/en/3.3/ug/adminguide/permissions_roles/)
- [Reddit moderator permissions](https://support.reddithelp.com/hc/en-us/articles/15484498369428-User-Management-moderators-and-permissions)
