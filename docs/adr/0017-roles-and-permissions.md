# ADR-0017: Roles and permissions

- Status: Proposed
- Date: 2026-09-26

## Context

- ADR-0010 added JWT login but explicitly deferred roles/permissions to #14/#30
- #14: role-based access, e.g. simple users can't change client data; UI should reflect what a user can do
- #30: user data (login, password, full name, email, phone, admin flag, position) plus rights (read, write, module-specific)
- FastEndpoints' JWT bearer already supports `Roles`/`Permissions`/`Claims` on the token and matching `Roles()`/`Permissions()` checks on `Configure()` - no new package needed

## Options

- Authorization model: coarse admin/non-admin flag only / free-form per-module Read+Write permission strings (matches #30's "Rights: Read, Write, Module specific") / full custom policy engine
- Where permissions live: computed from role only / stored per user and combined with role
- Admin bypass: admin role exempt from permission checks in code / admin is simply granted every permission string at login, so every endpoint only ever checks permissions

## Decision

- `UserRole` enum (`Standard`, `Admin`) plus a `Permission` string-constant catalog, one Read/Write pair per module (`Customers`, `StockItems`, `Sales`, `Settings`) plus `UsersManage`
- `User` gains `FullName`, `Email`, `Phone`, `Position`, `Role`, `Permissions` (`string[]`, ignored for Admins)
- Login computes the *effective* permission set (`Permission.CatalogAll` for Admins, the stored list for Standard users) and puts it on the JWT alongside the role string; every endpoint (except `/auth/login`) declares `Permissions(...)` for its module - no endpoint checks `Roles()` directly, so admin bypass is just "has every permission", not a second code path
- User management (`/users` CRUD) itself requires `Permission.UsersManage`, granted to Admins automatically and grantable to a Standard user like any other permission
- React: login response also carries role + effective permissions, stored the same way as the username, used to hide nav items/actions the user can't use (#14 item 3); the server remains the actual gate

## Consequences

- Permissions are additive strings, not a bitmask; adding a module later means adding one constant and tagging its endpoints, no migration of existing rows needed
- A Standard user's rights only take effect on their next login (stateless JWT, same tradeoff already accepted in ADR-0010) - no immediate revoke
- No password change/reset endpoint yet for self-service; still covered by user management (admin can set a new password for any user), self-service still open
- WPF GUI's `LoginViewModel` stays a non-functional stub (ADR-0010); not rewired here
