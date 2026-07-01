---
description: "Use when: working with custom Kine agents (orchestrator, dev, analysis, architecture) and enforcing SPEC-first workflow"
applyTo: ".github/agents/**/*.agent.md"
---

Agent governance for Kine project

Mandatory
1. Keep each agent single-purpose and role-specific.
2. Use minimal tools per agent.
3. Keep outputs compact and execution-focused.
4. Enforce SPEC-first reading before project-context work.
5. Do not take product/architecture/scope decisions without user approval.

Required SPEC synchronization
- If implementation changed behavior: update SPEC/07-change-log.md.
- If decision was made: update SPEC/06-decisions.md.
- If architecture changed: update SPEC/03-architecture.md.
- If unknown blocks progress: update SPEC/11-open-questions.md.
- If risk discovered: update SPEC/10-risk-register.md.

Agent naming convention
- Prefix with "kine-" and use clear role suffix.

Quality checks
1. Description includes trigger keywords for discovery.
2. Frontmatter YAML is valid.
3. Tools are least-privilege.
4. No circular agent delegation.
