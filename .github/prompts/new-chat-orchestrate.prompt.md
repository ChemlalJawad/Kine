---
description: "Use in a new chat to run Kine Orchestrator with SPEC-first context and minimal tokens"
name: "Kine New Chat - Orchestrate"
argument-hint: "Describe the exact request to execute"
agent: "Kine Orchestrator"
---
Start in strict execution mode.

Protocol
1. Read SPEC in this order: README, 06-decisions, 02-requirements, 03-architecture, 07-change-log, 11-open-questions.
2. Execute only the user request.
3. Keep response compact: Actions executed, Result, Blocker.
4. If the request impacts behavior/scope/architecture/decisions, update SPEC files accordingly.

User request:
{{input}}
