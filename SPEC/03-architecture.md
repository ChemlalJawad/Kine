# 03 - Architecture

Vue d ensemble
- Style cible: modular monolith au demarrage.
- Evolution: extraction de services si necessaire.

Domaines
- Patients
- Dossier clinique
- Agenda
- Facturation
- Remboursement
- Identite et acces
- Audit et conformite
- Reporting

Principes
- KISS en priorite.
- Frontieres claires entre domaines.
- Contrats explicites entre modules.
- Evenements metier pour tracer les transitions.

Multi-tenant
- Tenant obligatoire sur chaque operation.
- Isolation logique stricte des donnees.

Securite
- Authentification forte staff.
- Autorisation par roles.
- Journalisation des acces et modifications.

Observabilite
- Logs structures.
- Metriques techniques et metier.
- Alerting minimal pour incidents critiques.

Architecture v1 MVP (6 mois, 10-30 cabinets)
- Decomposition systeme (modular monolith)
- Module API Gateway/App: endpoints HTTP, validation, contexte tenant, authn/authz.
- Module Identite et acces: utilisateurs, roles, sessions, MFA staff.
- Module Patients: cycle de vie patient, statut, historique metier.
- Module Dossier clinique: notes, seances, documents metadata.
- Module Agenda: disponibilites, rendez-vous, annulation, no-show.
- Module Facturation: actes, facture, lignes, statut paiement.
- Module Remboursement: preparation dossier, statut, rejet, correction.
- Module Audit et conformite: journal immuable applicatif, consultation trace.
- Module Reporting: vues agreges metier et KPI operationnels.
- Frontieres: pas d acces direct DB entre modules; echanges via contrats internes + evenements metier internes.

- Strategie base de donnees (multi-tenant + audit)
- Un PostgreSQL manage en UE, schema unique, colonne tenant_id obligatoire sur tables metier.
- Cle primaire UUID stable; index composes tenant_id + id metier; FK avec tenant_id coherent.
- Row-Level Security activee sur tables metier (policy par tenant courant).
- JournalAudit append-only, hash de chaine (prev_hash, event_hash) pour detecter alteration.
- Historisation transitions critiques (statut rendez-vous, facture, remboursement, dossier clinique) dans tables d historique dediees.

- Blueprint cloud UE
- Region primaire UE unique pour MVP (ex: West Europe), 1 VNet prive.
- Environnements: dev, staging, prod separes (ressources et secrets separes).
- Sous-reseaux: app, data, ops; DB privee sans acces public.
- Secrets dans coffre gere (rotation cle API et credentials techniques).
- Sauvegardes DB quotidiennes + PITR 30 jours; test restauration mensuel sur staging.
- Stockage documents chiffre, replication zone locale, retention alignee legal Belgique.

- Modele securite par cabinet
- Authn: staff via OIDC + MFA obligatoire.
- Authz: RBAC par tenant (roles cabinet) + controles de permissions par module.
- Isolation: scoping tenant obligatoire au niveau requete + RLS DB + verifications service.
- Chiffrement: TLS 1.2+ en transit, AES-256 at-rest (DB, backups, objets).
- Logging: acces dossier patient, modifications sensibles, exports, echecs authn; horodatage + user_id + tenant_id.

- Evolution modular monolith vers services
- Etape 1 (MVP): monolithe modulaire avec contrats explicites et evenements internes.
- Etape 2: extraction Reporting en service lecture seul si charge analytique degrade la prod.
- Etape 3: extraction Facturation/Remboursement si integration externe ou scalabilite differenciee requise.
- Regle d extraction: uniquement si SLO, couplage, ou cadence de livraison ne tiennent plus.

Sequence implementation 12 semaines (dependances)
- S1-S2: fondations plateforme (CI/CD, environnements, secrets, observabilite, authn de base).
- S3-S4: Identite et acces + multi-tenant transverse (tenant context, RLS, audit minimal).
- S5-S6: Patients + Dossier clinique (depend de identite/multi-tenant/audit).
- S7-S8: Agenda (depend de patients + identite).
- S9-S10: Facturation + Remboursement v1 (depend agenda + dossier + audit).
- S11: Reporting v1 (depend donnees stabilisees des modules coeur).
- S12: hardening prod (perf, backup/restore drill, test securite, UAT, go-live).

Architecture technique V1 (executable)
- Stack application
- Backend: .NET 8 Web API (modular monolith) + Clean Architecture legere.
- Frontend: React + TypeScript (SPA) pour staff cabinet.
- DB: PostgreSQL 16 manage (UE).
- Cache/queues MVP: pas de bus distribue obligatoire; queue interne SQL + job worker.
- Object storage: bucket prive UE pour documents.

- Decomposition repository
- src/Kine.Api (entrypoint HTTP)
- src/Kine.Modules.Identity
- src/Kine.Modules.Patients
- src/Kine.Modules.Clinical
- src/Kine.Modules.Scheduling
- src/Kine.Modules.Billing
- src/Kine.Modules.Reimbursement
- src/Kine.Modules.Reporting
- src/Kine.Modules.Audit
- src/Kine.SharedKernel
- tests/Kine.UnitTests
- tests/Kine.IntegrationTests
- infra/terraform (network, db, storage, secrets, monitoring)

- Environnements cloud
- Dev: integration continue, donnees non reelles.
- Staging: pre-prod, jeux de test proches prod, tests restoration.
- Prod: exploitation cabinets.
- Chaque environnement a ses propres ressources et secrets.

- Reseau et acces
- VNet prive avec sous-reseaux app/data/ops.
- API exposee via reverse proxy manag
- DB privee uniquement accessible depuis app subnet.
- Acces ops via bastion ou VPN prive.

- Design base de donnees
- Convention: toutes tables metier ont tenant_id, created_at, updated_at, created_by.
- Tables coeur
- identity_users, identity_roles, identity_user_roles
- patients, patient_contacts, patient_consents
- clinical_records, clinical_sessions, clinical_documents
- appointments, practitioner_slots
- invoices, invoice_lines, payments
- reimbursement_cases, reimbursement_events
- audit_logs_append_only
- Index minimum
- (tenant_id, id)
- (tenant_id, status)
- (tenant_id, updated_at)
- (tenant_id, patient_id)
- RLS activee sur toutes tables tenant-scoped.

- Audit et conformite
- audit_logs_append_only: id, tenant_id, actor_id, action, entity, entity_id, ts_utc, payload_hash, prev_hash.
- Journal incident separe: incidents_log.
- Redaction PII/PHI dans logs techniques non-audit.

- Securite par cabinet
- Authn: OIDC + MFA obligatoire pour staff.
- Authz: RBAC par tenant (AdminCabinet, Kine, Assistant, Billing).
- Isolation: token tenant + guard service + RLS DB.
- Chiffrement: TLS 1.2+ transit, AES-256 repos DB/objets/backups.
- Secrets: vault/KMS, rotation periodique.

- Observabilite et resilience
- Logs structures centralises, metriques API/DB/jobs.
- Alerting minimal: auth failures, erreurs 5xx, echec backup, echec jobs remboursement.
- Backups: PITR 30 jours + restore drill mensuel.

- Regles d evolution
- Extraction en service uniquement si:
- SLO p95 API > 1.5s sur 2 sprints consecutifs,
- Couplage module bloquant la livraison,
- Ou exigence de scalabilite differenciee.
