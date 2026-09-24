# ADR-0006: MongoDB as single-node replica set, multi-step writes in transactions

- Status: Proposed
- Date: 2026-09-24

## Context

- Sale = stock change per line + invoice insert; must be all or nothing (#41)
- Multi-document transactions need a replica set; installs ran standalone `mongod`
- Step 1 (#66): conditional `$inc` per line + undo; still broken by a crash mid-sale

## Options

- Single-node replica set + session transactions
- Conditional `$inc` + undo (#66): no oversell, not atomic
- One document per sale (stock embedded): breaks the stock item model

## Decision

- Every MongoDB (desktop, API, tests) runs as replica set `rs0`, one node
- Multi-step writes in one session transaction, in Kernel `*ServiceProvider` (`IDatabase.StartSessionAsync`)
- Default connection string: `mongodb://127.0.0.1:27017/?directConnection=true`
- No standalone fallback

## Consequences

- Existing installs: one-time switch (README), else sales fail
- Tests: Testcontainers `.WithReplicaSet("rs0")`; CI jobs with a plain MongoDB service need `--replSet`
- Transactions available for later multi-step writes (import commit, counters)
