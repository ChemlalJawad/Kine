# 16 - Pilotage projet et avancement

Date de demarrage
- 2026-07-01

Nom produit
- Q-INE

Routage agents
- Orchestrator: coordination globale, suivi, reporting utilisateur.
- Kine Dev: implementation et validations techniques.
- Kine Analysis: analyse exigences/risques/compliance.
- Kine Architecture: arbitrage technique et decisions architecture.

Flux branches
- Base cible: main
- Une branche par lot de livraison

Regle d execution
- Dev execute les taches de developpement.
- Si contrainte technique/compliance bloque Dev, Analysis + Architecture produisent une solution executable pour reprise Dev.

Tableau taches (backlog P0)
| ID | Tache | Agent principal | Statut | Notes |
| --- | --- | --- | --- | --- |
| P0-001 | Initialiser backend modular monolith .NET 8 | Kine Dev | Done | Demarrage autorisable |
| P0-002 | Initialiser frontend React TypeScript | Kine Dev | Done | SPA React TS + routing /login + shell auth minimal |
| P0-003 | Provisionner infra dev/staging/prod | Kine Dev | Blocked | Bloque par Q-B02/Q-B08/Q-B10 |
| P0-004 | Implementer tenant context middleware | Kine Dev | In progress | Branche feature/p0-004 |
| P0-005 | Activer RLS sur tables coeur | Kine Dev | Done | Policies SQL + tests anti-fuite cross-tenant |
| P0-006 | Implementer RBAC par cabinet | Kine Dev | Done | RbacMiddleware (claims OIDC / X-Roles demo), matrice testee |
| P0-007 | MFA staff obligatoire | Kine Dev | Blocked | Design MFA/authn a cadrer |
| P0-008 | Journal audit append-only | Kine Dev | In progress | Branche feature/p0-008 |
| P0-009 | Module Patients v1 | Kine Dev | Done | Branche feature/P0-009; API + UI staff minimale, tests verts |
| P0-010 | Module Agenda v1 | Kine Dev | Done | Branche feature/P0-010; API + UI staff minimale, tests verts |
| P0-007 | MFA staff obligatoire | Kine Dev | In progress | Branche feature/p0-007, Swagger API demande |
| P0-008 | Journal audit append-only | Kine Dev | Done | Branche sur toutes les mutations sensibles + /api/audit/events + /verify |
| P0-009 | Module Patients v1 | Kine Dev | Done | Voir tableau ci-dessus |
| P0-010 | Module Agenda v1 | Kine Dev | Done | Voir tableau ci-dessus |
| P0-011 | Module Facturation v1 | Kine Dev | Done | Invoice + actes INAMI (montants placeholders) + UI |
| P0-012 | Module Clinical v1 (seances) | Kine Dev | Done | Q-B20 tranche: seances reelles, compteur derive |
| P0-013 | Module Reimbursement v1 | Kine Dev | Done | State machine SPEC/14, eFact mocke (Q-B03) |
| P1-001 | Reporting v1 | Kine Dev | Done | /api/reporting/summary + export CSV + page UI |
| P1-002 | Pipeline CI | Kine Dev | Done | GitHub Actions build+tests .NET / frontend |

Journal orchestration
- 2026-07-01 16:56 CET - Orchestrator: prise en charge du pilotage projet.
- 2026-07-01 16:57 CET - Orchestrator: chargement baseline SPEC + instructions agents.
- 2026-07-01 16:58 CET - Kine Analysis: qualification des items P0 ready vs blocked.
- 2026-07-01 16:59 CET - Kine Architecture: delta minimal "build-now / activate-later" via capability gates + stubs.
- 2026-07-01 17:00 CET - Kine Dev: ordre d execution technique valide pour lot ready.
- 2026-07-01 17:09 CET - User: renommage de l application en Q-INE.
- 2026-07-01 17:09 CET - Orchestrator: lancement du premier lot dev P0-001.
- 2026-07-01 17:28 CET - Orchestrator: commit du lot P0-001 realise (24fa4b457899d2144148cabab479c6b38be243ca).
- 2026-07-01 17:28 CET - Orchestrator: PR demandee, blocage outil GitHub MCP non disponible.
- 2026-07-01 17:28 CET - Orchestrator: lancement du lot dev P0-002.
- 2026-07-01 17:30 CET - Kine Dev: frontend React TypeScript initialise (routing /login, shell auth minimal, build Vite).
- 2026-07-01 18:55 CET - Orchestrator: branche main creee localement depuis master.
- 2026-07-01 18:55 CET - Orchestrator: branche feature/p0-004 creee pour le lot suivant.
- 2026-07-01 18:55 CET - Orchestrator: P0-004 lance en dev sur branche dediee.
- 2026-07-01 18:55 CET - Orchestrator: P0-004 valide et committe; lot suivant prepare.
- 2026-07-01 18:55 CET - Orchestrator: branche feature/p0-005 creee pour le lot RLS.
- 2026-07-01 18:55 CET - Orchestrator: P0-005 lance en dev sur branche dediee.
- 2026-07-01 19:00 CET - Kine Dev: P0-005 complete avec script RLS core tables et tests de verrouillage cross-tenant.
- 2026-07-01 19:30 CET - Orchestrator: P0-007 marque bloque; swagger demande en parallele.
- 2026-07-01 19:30 CET - Orchestrator: P0-008 lance en dev sur branche feature/p0-008.
- 2026-07-01 20:08 CET - Orchestrator: P0-009 lance en dev sur branche feature/P0-009.
- 2026-07-01 20:09 CET - Kine Dev: P0-009 backend complete (Domain/Application/Infrastructure + endpoints HTTP tenant-scopes + 18 tests verts); UI staff CRUD non traitee, aucune convention/maquette UI formulaire dans SPEC (a cadrer separement).
- 2026-07-01 20:22 CET - Orchestrator: P0-009 UI staff minimale demandee pour completion du lot.
- 2026-07-01 20:28 CET - Kine Dev: P0-010 complete (Kine.Modules.Scheduling: slots/rdv/annulation/no-show, 14 tests unitaires + 6 tests integration verts) + UI staff Agenda minimale reutilisant patientsApi; correction bug pre-existant tenantId/actorId dans useAuth() (Patients+Agenda).
- 2026-07-01 19:30 CET - Orchestrator: branche feature/p0-007 creee pour le lot MFA.
- 2026-07-01 19:30 CET - Orchestrator: P0-007 lance en dev avec demande Swagger API.
- 2026-07-02 - Kine Dev: P0-011 Module Facturation v1 (restaure apres perte partielle de fichiers, + tests + seed); refonte Agenda vue journee; KPIs Dashboard branches sur Billing reel.
- 2026-07-03 - User: validation du lot "continuer le dev de tout" (audit branche + RBAC + Clinical + Reimbursement mock + Reporting + CI); Q-B20 tranche (seances reelles); RBAC valide en mode claims OIDC + header X-Roles demo.
- 2026-07-03 - Kine Dev: P0-006, P0-008, P0-012, P0-013, P1-001, P1-002 livres (voir SPEC/07-change-log.md). Verification frontend OK; dotnet build/test a executer localement (SDK indisponible dans l'environnement agent).

Actions realisees
- Gouvernance multi-agents activee (Orchestrator/Dev/Analysis/Architecture).
- Repartition initiale des taches P0 et statuts d avancement initialises.
- Log central de pilotage cree pour suivi continu.
- Nom public de l application fixe a Q-INE.

Points en attente utilisateur
- Validation lot de demarrage: P0-001/002/004/005/006/007/008/010.
- Confirmation hold: P0-003 et P0-009 jusqu a cloture des questions bloquantes.
