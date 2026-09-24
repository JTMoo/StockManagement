# ADR-0001: Decisions live in the repo as ADRs

- Status: Accepted
- Date: 2026-09-24

## Context

- Cloud and local Claude sessions lose decisions between sessions

## Options

- Nothing written down: sessions contradict each other
- External wiki: invisible to sessions and reviews
- In-repo: `CLAUDE.md` + `docs/adr/` + `docs/decisions.md`

## Decision

- In-repo
- Important decisions: grill, then ADR
- Owner review comments: line in `CLAUDE.md`

## Consequences

- Decision PRs carry an ADR and an index update
