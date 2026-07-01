---
description: "Use when managing GitHub branches, pushes, pull requests, merges, syncs, or repository delivery workflow."
---

# GitHub workflow

- When the user asks for branch, push, PR, or merge work, execute it directly.
- Do not require extra approval for creating branches, pushing commits, creating/updating PRs, or merging the requested branch.
- Ask for confirmation only for irreversible actions not explicitly requested, like force-push, branch deletion, or history rewrite.
- If GitHub access, auth, or permissions block the action, report the blocker explicitly.
