# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Q-INE: SaaS platform for physiotherapists in Belgium. Backend is a .NET 8 modular monolith (Clean Architecture légère); frontend is a React + TypeScript SPA. Currently early MVP stage — most modules are empty scaffolds, only `Patients`, `Scheduling`, and `Audit` have real logic, all backed by in-memory stores (no DB/EF Core wired up yet).

## Agent policy (mandatory — read before acting)

This repo enforces a strict, token-minimal execution policy for AI agents (`.github/copilot-instructions.md`, `.github/instructions/*.md`):

- Execute only what is explicitly requested; do not expand scope implicitly.
- **SPEC-first**: `SPEC/` is the canonical source of truth for requirements, architecture, decisions, and history. Consult it before project-context work.
- Sync SPEC when a change has impact: behavior → `SPEC/07-change-log.md`, decisions → `SPEC/06-decisions.md`, architecture → `SPEC/03-architecture.md`, unknowns → `SPEC/11-open-questions.md`, risks → `SPEC/10-risk-register.md`.
- No autonomous product/architecture/scope decisions without user approval. No unsolicited opinions/alternatives.
- If information is missing, ask one short blocking question instead of assuming.
- Responses: compact, action-oriented. Default output ends with Actions executed / Result / Blocker.
- `.github/agents/` defines specialized sub-agents (kine-orchestrator, kine-dev, kine-analysis, kine-architecture) — single-purpose, least-privilege tools, no circular delegation.

## Commands

Backend (.NET 8, from repo root):
```bash
dotnet restore
dotnet build
dotnet test
dotnet test --filter "FullyQualifiedName~PatientServiceTests"   # single test class
dotnet run --project src/Kine.Api                                # run API (Swagger at /swagger)
```
Test projects use xunit. `Kine.UnitTests` covers domain/application logic; `Kine.IntegrationTests` uses `Microsoft.AspNetCore.Mvc.Testing` against `Kine.Api`.

Frontend (from `src/Kine.Web`):
```bash
npm install
npm run dev       # Vite dev server, http://localhost:5173
npm run build
npm run preview
```
API CORS is currently hardcoded to `http://localhost:5173` / `127.0.0.1:5173` in `src/Kine.Api/Program.cs`.

No CI workflow is currently configured in `.github/` (no `.github/workflows/`); `SPEC/15-dev-standards.md` documents the intended gates (60%+ unit coverage, anti-tenant-leak tests, lint, architecture review) but they are not yet automated.

## Architecture

**Modular monolith**: `src/Kine.Api` is the single HTTP entry point; each business domain is a separate class library project referenced by the API, with no direct DB access between modules (contracts + internal domain events only, per `SPEC/03-architecture.md`). Modules implement `Kine.SharedKernel.IModule` and are registered in `src/Kine.Api/Modules/ModuleCatalog.cs`.

Modules: `Kine.Modules.Identity`, `Patients`, `Clinical`, `Scheduling`, `Billing`, `Reimbursement`, `Reporting`, `Audit`. Of these, only **Patients**, **Scheduling**, and **Audit** currently have real code (`Domain/`, `Application/`, `Infrastructure/` folders with entities, services, and in-memory stores); the rest are empty `IModule` marker classes awaiting implementation.

**Multi-tenancy is central and non-negotiable**: every business table/query must carry `tenant_id`. `TenantContextMiddleware` (`src/Kine.Api/Middleware/`) extracts tenant from the `tenant_id` OIDC claim or the `X-Tenant-Id` header and stores it in `HttpContext.Items`; requests without a tenant (except `/`, `/health`, `/swagger`) get a 400. Target DB enforcement is PostgreSQL Row-Level Security — see `infra/scripts/p0-005-rls-core-tables.sql` for the per-table `USING`/`WITH CHECK` policy pattern (tested for coverage in `tests/Kine.UnitTests/RlsPolicyScriptTests.cs`), though no DB is wired into the app yet.

**Auth**: staff authenticate via external OIDC; MFA is enforced only via OIDC claims (`amr`/`acr`), never stored locally. `StaffMfaEnforcementMiddleware` rejects any authenticated request lacking MFA claim evidence with 403. No local MFA secrets/state exist by design (MVP decision, see `SPEC/06-decisions.md`).

**Audit trail**: `Kine.Modules.Audit` implements an append-only, hash-chained journal (`AuditEvent` with `PrevHash`/`EventHash`). `AuditChainVerifier.IsValid` recomputes each event's hash and checks chain linkage — any break indicates tampering. This is the pattern to follow for any future audit-relevant writes.

**Language convention** (`SPEC/15-dev-standards.md`): business/domain classes and comments in French (e.g. `RendezVous`, `FactureRemboursement`); technical classes (`Repository`, `Handler`, `Middleware`) and their comments in English.

**Target module internal structure** (aspirational, not fully realized yet — see Patients/Scheduling for closest examples): `Domain/` (aggregates, value objects, domain events, repository interfaces) → `Application/` (commands, queries, DTOs, service interfaces) → `Infrastructure/` (EF Core persistence, service impls) → `Presentation/` (controllers/endpoints, currently done via minimal-API extension methods in `Kine.Api/Modules/*Endpoints.cs`).

**Data model target** (not yet implemented — no EF Core/DbContext in repo): single PostgreSQL 16 schema, UUID PKs, `(tenant_id, id)` composite indexing convention, RLS on all tenant-scoped tables. Full intended schema and reasoning are in `SPEC/04-data-model.md` and `SPEC/03-architecture.md`.

## Key docs

`SPEC/` is the authoritative project reference — read the relevant file before making decisions in that area: `01-vision.md`, `02-requirements.md`, `03-architecture.md`, `04-data-model.md`, `05-compliance-be.md`, `06-decisions.md`, `07-change-log.md`, `08-roadmap.md`, `09-backlog.md`, `10-risk-register.md`, `11-open-questions.md`, `14-reimbursement-rules.md` (INAMI reimbursement rules), `15-dev-standards.md`.
