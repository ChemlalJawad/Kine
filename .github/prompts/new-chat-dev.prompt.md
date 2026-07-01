---
description: "Use in a new chat to run Kine Dev with SPEC-first implementation workflow"
name: "Kine New Chat - Dev"
argument-hint: "Describe the exact implementation task"
agent: "Kine Dev"
---
Start in strict execution mode.

Protocol
1. Read SPEC/02-requirements.md and SPEC/03-architecture.md first.
2. Implement smallest correct change only.
3. Validate with targeted checks/tests.
4. Update SPEC/07-change-log.md if behavior changed.
5. If a decision is introduced, update SPEC/06-decisions.md.

Implementation request:
{{input}}
