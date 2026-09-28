# ADR-0026: Unattended PR merging: required checks + Claude review, no owner click

- Status: Proposed
- Date: 2026-09-28

## Context

- Owner no longer wants to approve every PR by hand; wants to be pulled in only for important technical decisions or when the next step is unclear (owner, 2026-09-28)
- `JTMoo/StockManagement` is a personal-account repo: GitHub merge queues need an org
- Parallel threads on separate branches cause merge conflicts that pile up before anyone merges

## Options

- Merge gate: owner click / required status checks only / required checks + a Claude review check
- Conflict handling: wait until merge time / merge `main` into open PR branches after every push to `main`
- ADR number bookkeeping: hand-maintained table / generated from `docs/adr/*.md`

## Decision

- Branch protection on `main`: required status checks (`api-tests`, `web-tests`, `unit-tests`, `adr-index`, `claude-review`), require branches up to date, no owner review required. Repo setting, not API-settable — owner does this once (see PR body)
- `.github/workflows/claude-review.yml`: `anthropics/claude-code-action` runs `/code-review` on every PR; a blocking finding fails the check, same as a failing test
- Threads enable GitHub's native per-PR auto-merge on their own PR once pushed (`enable_pr_auto_merge`); it merges once every required check is green
- `.github/workflows/sync-main-into-prs.yml`: after each push to `main`, merge `main` into every open PR branch; a real conflict gets a PR comment instead of a silent merge
- `docs/decisions.md`'s ADR table is generated from `docs/adr/*.md` (`scripts/generate-adr-index.mjs`, checked in CI) — removes the table as a conflict hotspot; ADR numbers still assigned by whoever starts a thread

## Consequences

- Owner is no longer on the critical path for a green PR; still asked before anything the review/CI gate can't judge (architecture, scope)
- A red Claude review or CI check blocks auto-merge the same way; no separate "soft" gate
- Needs `ANTHROPIC_API_KEY` (or Claude Code OAuth token) as a repo secret — owner adds it, see PR body
- `sync-main-into-prs` pushes directly to other threads' branches; a thread mid-push could race it (accepted; rare, and a conflict comment is visible either way)
