# ADR-0001: Record important decisions as ADRs in a repo knowledge base

- Status: Accepted
- Date: 2026-09-24
- Deciders: Jonathan Trefz

## Context

The project is being redesigned (WPF desktop app to API + React) and extended towards ERP features, with Claude
sessions in the cloud and on the owner's computer doing much of the work. Decisions and the owner's preferences need
to survive between sessions, and each session should check them before deciding something new.

## Options considered

1. **Nothing written down**: no overhead, but every session re-derives or contradicts earlier decisions.
2. **Wiki or external docs**: outside the repo, so local sessions and reviews do not see it.
3. **In-repo knowledge base**: `CLAUDE.md` for coding rules and review lessons, ADRs for important decisions,
   `docs/decisions.md` as the index to consult first.

## Decision

Option 3. Important decisions are grilled first (the owner's *grill me* skill) and recorded as ADRs in `docs/adr/`
(MADR-style, see [README.md](README.md)). `docs/decisions.md` indexes ADRs, standing decisions and open questions.
`CLAUDE.md` holds coding guidelines and grows with conventions learned from the owner's review comments.

## Consequences

- Every session reads `CLAUDE.md` and `docs/decisions.md` before important changes.
- Decision PRs carry an ADR; the index must be kept in sync in the same PR.
- Some overhead per decision, kept small by the one-page template.
