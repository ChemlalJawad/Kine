# 00 - Feuille de route développeurs (Analyse approfondie)

## Synthèse exécutive

Le projet Kine est un **modular monolith MVP** sur 12 semaines pour **10-30 cabinets de kinésithérapie en Belgique**. 
- **Direction:** Plateforme SaaS complète (patient, dossier, agenda, facturation, remboursement, reporting).
- **Risque principal:** Complexité légale/remboursement + isolation multi-tenant critique.
- **Stratégie:** Implémenter les fondations strictes (identité, multi-tenant, audit), puis modules métier en parallèle.

---

## 1. Voie critique (12 semaines)

### S1-S2 : Fondations plateforme (CRITIQUE)
**Objectif:** Socle technique et infra sécurisée operational.

**Livrables:**
- CI/CD pipelines (GitHub Actions ou Azure Pipelines) : build, test, deploy dev/staging/prod
- Environnements séparés (dev/staging/prod) avec ressources et secrets isolés
- Authentification OIDC + MFA staff fonctionnelle
- PostgreSQL UE avec schéma initial (tenant_id, RLS policies, audit_logs append-only)
- Observabilité minimale: logs structurés, métriques API/DB
- API Gateway skeleton (.NET Web API vierge)

**Dépendances:** Aucune (démarrage indépendant).
**Risques:**
- ❌ RLS non activée correctement → fuite inter-tenant (R-003 - CRITIQUE)
- ❌ CI/CD incomplet → déploiement manuel sujet à erreurs
- ❌ MFA non obligatoire staff → faille accès

**Validations requises:**
- [ ] Restore drill DB sur staging (proof of backup integrity)
- [ ] Fuite inter-tenant testée en CI (query sans tenant context deve échouer)
- [ ] Logs audit captures pour création user/role/login

---

### S3-S4 : Identité et multi-tenant transverse
**Objectif:** Gestion d'accès et isolation tenant opérationnelle dans tous les modules.

**Livrables:**
- Module Identité: users, roles (AdminCabinet, Kine, Assistant, Billing), sessions
- Tenant context propagation (JWT token + request middleware)
- RLS policies activées et testées sur toutes tables métier
- Journal audit append-only fonctionnel (hash chain)
- Tests anti-fuite: 100% des queries retestées avec tenant_id != tenant courant

**Dépendances:** Fondations (S1-S2).
**Risques:**
- ❌ Tenant context missing dans une query → données croisées
- ❌ RLS policy trop large → access non restreint

**Validations requises:**
- [ ] Audit trail for all modifications (user X, action Y, entity_id Z, timestamp, tenant_id)
- [ ] Session expiration and MFA re-auth after inactivity (15 min suggested)
- [ ] Role-based access control enforced at API layer (not just DB)

---

### S5-S6 : Patients + Dossier clinique
**Objectif:** Noyau données patient stable.

**Livrables:**
- Patients: CRUD, statut, historique métier
- Dossier clinique: notes, séances, documents (métadata seul MVP)
- Historisation transitions critiques (status changes)
- Consentements patient (pour RGPD)

**Dépendances:** Identité + multi-tenant (S3-S4).
**Risques:**
- ❌ Données cliniques sensibles mal chiffrées en transit/repos
- ❌ Export patient données sans audit

**Validations requises:**
- [ ] All clinical data encrypted at-rest (AES-256)
- [ ] Document upload/download audited (who, when, what)
- [ ] Patient consent rules enforced per jurisdiction

---

### S7-S8 : Agenda
**Objectif:** Disponibilités et rendez-vous.

**Livrables:**
- Disponibilités praticien (slots)
- Rendez-vous (création, annulation, no-show)
- Alertes no-show
- Historique annulations

**Dépendances:** Patients (S5-S6).
**Blocants éventuels:** Aucun identifié.

---

### S9-S10 : Facturation + Remboursement v1
**Objectif:** Facturation de base + chaîne remboursement structurée.

**Livrables (Facturation):**
- Actes (linked agenda)
- Factures (création, édition, annulation)
- Lignes facture avec montants
- Statut paiement

**Livrables (Remboursement v1 - SCOPE RÉDUIT):**
- Dossier remboursement (création, statut)
- Events: dossier créé, soumis, approuvé, rejeté, corrigé
- Trace complète (audit → conformité remboursement)
- **Hors MVP:** Intégrations eFact/eAttest (à confirmer Q-B03)

**Dépendances:** Agenda (S7-S8) + Dossier (S5-S6) + Audit (S3-S4).
**Risques (ÉLEVÉS):**
- ❌ Règles remboursement belges mal comprisses (R-002 - Haute prob/impact)
- ❌ Intégration eFact obligatoire non finalisée avant code
- ❌ Retention données incomprise → violations légales

**Validations requises:**
- [ ] Calcul montants audité (trace avant/après modifications)
- [ ] Reimbursement workflow states immutable (state machine)
- [ ] Proof of reimbursement request sent + response received (logging)

---

### S11 : Reporting v1
**Objectif:** Agrégations métier simples (activité, chiffre d'affaires, encaissements).

**Livrables:**
- Vues agrégées read-only: activité par période, CA, encaissement
- Dashboard cabinet (accès AdminCabinet seul)
- Export CSV de base

**Dépendances:** Tous modules métier stabilisés (S10).

---

### S12 : Hardening prod
**Objectif:** Vérifications finales avant go-live.

**Livrables:**
- [ ] Performance audit (P95 API < 500ms, DB queries < 100ms)
- [ ] Backup/restore drill complet (time proof)
- [ ] Tests sécurité: SQL injection, XSS, CSRF, auth bypass
- [ ] UAT avec 2-3 cabinets pilotes
- [ ] Incident playbook finalisé
- [ ] Monitoring + alertes en place

---

## 2. Blocants critiques à lever **IMMÉDIATEMENT**

| ID | Question | Impact | Owner | Délai | Action dev |
|---|---|---|---|---|---|
| Q-B02 | Key management (KMS cloud vs clé client) | BLOQUANT | Security | 2026-07-08 | Hold: Ne pas coder chiffrement perso tant que non décidé |
| Q-B03 | eHealth/eFact/eAttest MVP requis? | BLOQUANT | Compliance | 2026-07-10 | Hold: Ne pas intégrer tant que non validé; ou intégrer POC seul |
| Q-B04 | Retention par type de document | BLOQUANT | DPO | 2026-07-10 | Need URGENTLY: Matrice retention pour all data classes (clinical, invoice, reimbursement, audit, backup) |
| Q-B08 | Backup/failover intra-UE constraints | BLOQUANT | SRE | 2026-07-10 | Define backup location, replication rules, restore location before S1-S2 closes |
| Q-B10 | RTO/RPO targets | BLOQUANT | SRE | 2026-07-08 | Define: RTO = X hours, RPO = Y minutes for prod |

**Action immédiate:** Escalader Q-B03, Q-B04, Q-B08, Q-B10 auprès de Compliance Lead + DPO + SRE. Bloque S1-S2 infrastructure.

---

## 3. Stratégie de réduction risques

### R-001 : Exigences légales incomplètes ou évolutives (Haute prob, Haut impact)
- **Mitigation:** Gate légal à chaque release + registre sources officielles belges + validation refs.
- **Action dev:** Créer SPEC/13-legal-sources.md = liste sources officielles (INAMI, SPF, Douanes, etc.)
- **Dépendance:** Q-B03 (eHealth scope), Q-B04 (retention rules)

### R-002 : Complexité remboursement (Haute prob, Haut impact)
- **Mitigation:** Scope MVP réduit + rollout progressif (v1 = base, v2/v3 = cas complexes)
- **Action dev:** 
  - Challenger Product Manager sur scope remboursement S9-S10
  - Créer SPEC/14-reimbursement-rules.md = règles métier validées INAMI
  - Tests unitaires exhaustifs pour chaque règle

### R-003 : Fuite inter-tenant (Moyenne prob, CRITIQUE impact)
- **Mitigation:** Tenant context obligatoire + RLS + tests CI
- **Action dev:**
  - S3-S4: Tests anti-fuite en CI (scan toutes queries pour absence tenant_id)
  - S5+: Code review checklist: "Tenant context present?" obligatoire
  - Runbook incident fuite (isolation rapide)

### R-004 : Dette technique (Moyenne prob, Haut impact)
- **Mitigation:** Quality gates
- **Action dev:** 
  - Architecture review tous les 2 weeks
  - Ratio tests: UnitTests 60%+ couverture, IntegrationTests pour flows critiques
  - Static analysis (SonarQube ou Roslyn rules)

### R-006, R-009 : Rétention, backup fiabilité
- **Dépendance:** Q-B04, Q-B08, Q-B10
- **Action dev:** Automatiser retention cleanup + restore drill monthly (preuve documentée)

---

## 4. Plan d'exécution développeurs (étapes concrètes)

### Avant S1 (cette semaine)
1. **Régler Q-B02, Q-B03, Q-B04, Q-B08, Q-B10** via escalade Compliance/DPO/SRE
2. **Créer repo**: Kine-monolith .NET 8
3. **Créer SPEC supplémentaires**:
   - SPEC/13-legal-sources.md (sources officielles belges)
   - SPEC/14-reimbursement-rules.md (règles métier)
   - SPEC/15-dev-standards.md (conventions code, security checks)

### S1-S2 (prochaines 2 semaines)
1. **Infra:**
   - Azure subscription (UE) + environment setup (dev/staging/prod)
   - PostgreSQL managed (UE) + VNet prive
   - GitHub/Azure CI/CD pipelines
   - Secrets vault (KMS ou Azure Key Vault)
   
2. **Code fondations:**
   - Kine.Api skeleton (middleware tenant context, error handling, logging)
   - Kine.SharedKernel (base classes, tenant context, audit interfaces)
   - RLS policies (template pour toutes tables tenant-scoped)
   - Database migrations (initial schema: users, roles, audit_logs, tenants)

3. **Security:**
   - OIDC client setup (OpenID Connect provider)
   - MFA pour staff (TOTP ou WebAuthn)
   - TLS 1.2+ everywhere
   - Log redaction rules (PII/PHI hidden)

4. **Observabilité:**
   - Structured logging (Serilog ou logs enrichis JSON)
   - Health checks endpoints
   - Dashboard minimal (infra readiness)

5. **Tests:**
   - Anti-fuite tests (CI rule: query without tenant context = build fail)
   - Backup restore test (monthly automation)

### S3-S4
- Kine.Modules.Identity (users, roles, sessions, MFA)
- Tenant context propagation
- RLS policy enforcement
- Audit trail capture
- Session/token refresh logic

### S5-S6+
- Modules métier en parallèle (Patients, Clinical, Scheduling, Billing, Reimbursement, Reporting)
- **Dépendance stricte respectée**
- Code review + architecture alignment all reviews

---

## 5. Critique d'architecture et décisions requises

### Modularité
✅ **Décidé:** Modular monolith d'abord.
- Contrats explicites (interfaces) entre modules
- Événements métier internes (pas de bus externe MVP)
- Queue interne SQL + job worker
- Déploiement monolithe unique (une .dll)

**Attaque:** Dépendances circulaires interdites. Chaque module = folder src/Kine.Modules.{Name} autonome.

### Multi-tenant
✅ **Décidé:** Shared schema + tenant_id + RLS.
- ✅ **Décision D-004 validée** mais **dépend Q-B08** (backup isolation).
- Toute table métier = colonne tenant_id obligatoire.
- RLS policy: `tenant_id = current_setting('app.tenant_id')`.
- FK + index composés: `(tenant_id, id)` minimum.

**Attaque:** Validation Q-B08 prioritaire (backup par tenant ou global? restore isolation?).

### Audit
✅ **Modèle append-only** (immutabilité).
- Colonne: id, tenant_id, actor_id, action, entity, entity_id, ts_utc, payload_hash, prev_hash.
- Hash chain: SHA-256(prev_hash || event_hash).
- Détection altération: hash mismatch vs chain.

**Attaque:** Implémenter en S3-S4. Tester integrity monthly.

### Conformité
⚠️ **Decisions en attente:**
- Q-B03: eHealth/eFact scope (integration ou mock?)
- Q-B04: Retention policy (clinical 5y?, invoice 10y?, audit permanent?)
- Q-B02: Key management (cloud KMS vs client bring-key)

**Action:** Bloquer intégrations eHealth tant que Q-B03 ouvert.

---

## 6. Roadmap prochaine étape

**Cette semaine (2026-07-01 à 2026-07-05):**
1. Escalade Q-B02, Q-B03, Q-B04, Q-B08, Q-B10 → Compliance/DPO/SRE
2. Créer SPEC/13, 14, 15
3. Setup repo .NET + Azure subscription
4. Finaliser team roles (Dev Lead, Security Lead, DPO, Compliance Lead)

**Semaine suivante (2026-07-08 à 2026-07-12):**
- Réception réponses Q-B02, Q-B04, Q-B10
- Démarrage S1-S2 (CI/CD + fondations infra)
- Premier prototype auth OIDC + MFA

**Par 2026-07-20:**
- S1-S2 complété (fondations opérationnelles)
- Proof of backup/restore réussi
- Anti-fuite tests passant en CI

---

## 7. Points d'attention pour développeurs

### Discipline multi-tenant
- **Chaque requête doit inclure tenant context.**
- **Code review checklist obligatoire:** "Tenant context present in query?" "RLS policy active?"
- **Tests anti-fuite run before merge** (git hooks).

### Audit trail
- **Toute action sensible → audit_logs append-only:** user creation, role grant, patient record access, invoice creation, reimbursement submission.
- **Payload audité:** avant/après valeurs, qui, quand, pourquoi.

### Sécurité
- **TLS 1.2+ obligatoire** (all endpoints + DB connection).
- **MFA staff obligatoire au MVP** (no exception).
- **Secrets rotation:** API keys, DB credentials, OIDC client secret (automatisé si possible).

### Tests
- **Unit: 60%+ couverture.** Logique métier, règles remboursement, isolation tenant.
- **Integration: Flows critiques.** Auth, patient CRUD, invoice creation + audit trail, backup/restore.
- **E2E: UAT avec 2-3 cabinets S12.**

### Décisions blockantes
- **Arrêter développement chiffrement custom tant que Q-B02 ouvert.** Utiliser Azure KMS ou provider fourni après décision.
- **Arrêter intégration eHealth tant que Q-B03 ouvert.** Mocking autorisé.
- **Attendre Q-B04 avant code nettoyage données.** Retention rules obligatoires.

---

## 8. Glossaire domaine métier

| Terme | Définition |
|---|---|
| **Kinésithérapeute (Kine)** | Praticien (physical therapist) |
| **Cabinet** | Practice / clinic (tenant) |
| **Dossier clinique** | Clinical record / patient chart |
| **Séance** | Session / appointment outcome |
| **RendezVous** | Appointment (scheduled event) |
| **Acte** | Billable act/procedure |
| **eFact** | Belgian invoicing e-service (INAMI) |
| **eAttest** | Belgian medical attestation e-service |
| **eHealth** | Belgian national health interoperability platform |
| **INAMI** | Institut National d'Assurance Maladie-Invalidité (health insurance) |
| **DPO** | Data Protection Officer (RGPD responsible) |
| **Remboursement** | Reimbursement / claim (via INAMI) |
| **RGPD** | Règlement Général sur la Protection des Données (GDPR) |

---

## Résumé exécutif pour la direction

**État:** Architecture définie, blocants légaux/infra identifiés, développement peut démarrer immédiatement sur fondations.

**Risques critiques:** Complexité remboursement (scope réduit MVP = mitigation), fuite inter-tenant (RLS + tests CI = contrôle).

**Prochaines étapes (priorité):**
1. **Lever Q-B03, Q-B04, Q-B08, Q-B10** (Compliance, DPO, SRE) — bloque infra.
2. **Lancer S1-S2** (CI/CD, auth, DB, observabilité) — chemin critique.
3. **Paralléliser S3-S4** avec S1-S2 (identité + multi-tenant).

**Go-live target:** 2026-12-15 (12 semaines si blocants fermés s1w1).
