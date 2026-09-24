# Architecture Decision Records

An ADR records one important decision: the context, what was decided and what follows from it. ADRs are the
long-form part of the knowledge base; the index lives in [../decisions.md](../decisions.md).

## Conventions

- File name: `NNNN-short-title-in-kebab-case.md`, numbered sequentially from `0001`. Numbers are never reused.
- Start from [template.md](template.md).
- Status is one of `Proposed`, `Accepted`, `Deprecated`, `Superseded by ADR-NNNN`.
- An accepted ADR is not rewritten. To change a decision, write a new ADR and set the old one to
  `Superseded by ADR-NNNN` (fixing typos or adding links is fine).
- Keep it short: one decision per ADR, a page at most.
- Every new or changed ADR gets its row updated in [../decisions.md](../decisions.md) in the same PR.

## Process

1. Check [../decisions.md](../decisions.md) for related decisions and open questions.
2. Grill the decision (see *Before making an important decision* in [../../CLAUDE.md](../../CLAUDE.md)).
3. Write the ADR as `Proposed`, open a PR; the owner accepts it by merging (set status to `Accepted` before merge).
