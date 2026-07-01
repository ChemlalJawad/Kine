# Kine - SaaS Platform

Plateforme SaaS pour kinésithérapeutes en Belgique.

## Architecture

**Style:** Modular monolith (Clean Architecture légère).

Voir [SPEC/03-architecture.md](SPEC/03-architecture.md) pour détails complets.

## Modules

- `Kine.Api` — HTTP API entry point
- `Kine.SharedKernel` — Base classes et interfaces partagées
- `Kine.Modules.Identity` — Authentification, users, roles
- `Kine.Modules.Patients` — Gestion patients
- `Kine.Modules.Clinical` — Dossier clinique
- `Kine.Modules.Scheduling` — Agenda et rendez-vous
- `Kine.Modules.Billing` — Facturation
- `Kine.Modules.Reimbursement` — Remboursement (INAMI)
- `Kine.Modules.Reporting` — Reporting et analytics
- `Kine.Modules.Audit` — Audit trail (append-only)

## Documentation

- [SPEC/01-vision.md](SPEC/01-vision.md) — Vision produit
- [SPEC/02-requirements.md](SPEC/02-requirements.md) — Exigences
- [SPEC/03-architecture.md](SPEC/03-architecture.md) — Architecture
- [SPEC/04-data-model.md](SPEC/04-data-model.md) — Modèle données
- [SPEC/06-decisions.md](SPEC/06-decisions.md) — Décisions architecturales
- [SPEC/10-risk-register.md](SPEC/10-risk-register.md) — Registre risques
- [SPEC/11-open-questions.md](SPEC/11-open-questions.md) — Questions bloquantes
- [SPEC/13-legal-sources.md](SPEC/13-legal-sources.md) — Sources officielles
- [SPEC/14-reimbursement-rules.md](SPEC/14-reimbursement-rules.md) — Règles remboursement
- [SPEC/15-dev-standards.md](SPEC/15-dev-standards.md) — Standards développement
- [SPEC/00-developer-roadmap.md](SPEC/00-developer-roadmap.md) — Feuille de route développeurs

## Setup

```bash
# Install .NET 8 SDK
# https://dotnet.microsoft.com/

# Restore packages
dotnet restore

# Build
dotnet build

# Run tests
dotnet test
```

## Development

Follow [SPEC/15-dev-standards.md](SPEC/15-dev-standards.md) for:
- Code conventions
- Multi-tenant discipline
- Audit trail requirements
- Testing standards
- Security gates

## License

© Kine 2026
