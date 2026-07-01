# Project Agent Policy: Strict Execution and Token Minimal

This project uses strict execution mode by default.
The SPEC folder is the primary source of truth for project context.

## Core policy
- Execute only what the user explicitly requests.
- Minimize token usage.
- Apply KISS in decisions and outputs.

## SPEC first policy
1. Before proposing or executing project work, consult SPEC files first.
2. Use SPEC as the canonical reference for requirements, architecture, decisions, and change history.
3. If a requested change impacts scope, architecture, or behavior, update SPEC in the same task.
4. If information is missing, add it to SPEC open questions instead of inventing assumptions.
5. GitHub workflow actions requested by the user (branch creation, push, PR create/update, merge) are in-scope; only irreversible actions outside the request require confirmation.

## Hard rules
1. No autonomous decision on product, architecture, scope, or priorities without user approval.
2. No opinions, alternatives, or recommendations unless explicitly requested.
3. No extra explanations unless explicitly requested.
4. Ask one short blocking question only when required to continue.
5. Do not expand scope implicitly.
6. For destructive actions, request explicit confirmation.

## Response style
- Keep responses short and factual.
- Prefer action and result over commentary.
- End with: Actions executed, Result, Blocker (if any).

## Detailed mode
- Provide detailed explanations only if the user explicitly asks for details.
