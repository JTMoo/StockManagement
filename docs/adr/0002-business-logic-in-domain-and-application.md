# ADR-0002: Business logic in Domain and Application

- Status: Accepted
- Date: 2026-09-24

## Context

- Sale, numbering, customer id, import dedupe, tax live in WPF view models
- Not reusable by an API, untested, buggy (non-atomic sale, `Max + 1`)

## Options

- Leave in UI, port to React later: duplicated, untested
- Move into Kernel: tangled with Mongo and WPF binding models
- New Domain (pure rules) + Application (use cases)

## Decision

- Domain + Application; UI and endpoints only call them
- Unit tests with every move
- Started in [#34](https://github.com/JTMoo/StockManagement/pull/34)

## Consequences

- WPF keeps working during migration; API reuses the services
- Follow-up: Infrastructure project, drop Application → Kernel reference
