# 07 - Log des changements

Format
- Date:
- Type: Added | Changed | Fixed | Removed
- Zone:
- Description:
- Auteur:

Historique
- Date: 2026-07-01
- Type: Added
- Zone: Backend
- Description: Ajout du middleware de contexte tenant Q-INE avec rejet des requetes sans tenant
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Frontend
- Description: Initialisation frontend React TypeScript pour Q-INE avec routing et shell auth minimal
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Backend
- Description: Initialisation du backend modular monolith .NET 8 avec modules references et endpoint health
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: SPEC
- Description: Creation du dossier SPEC et des fichiers de gouvernance
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Agents
- Description: Creation des agents projet specialises sous .github/agents (orchestrator, dev, analysis, architecture)
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Instructions
- Description: Ajout de .github/instructions/agents.instructions.md pour gouvernance des agents et sync SPEC
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Architecture
- Description: Proposition architecture v1 MVP (6 mois, 10-30 cabinets): decomposition modules, strategie DB multi-tenant/audit, blueprint cloud UE, modele securite, trajectoire modular monolith -> services, sequence 12 semaines
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Decisions
- Description: Ajout ADR proposes D-004 a D-006 (DECISION-REQ) pour multi-tenant DB, blueprint cloud UE et trajectoire evolution architecture
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Risques
- Description: Enrichissement registre des risques avec top 10 risques, mitigations et owners
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Questions ouvertes
- Description: Ajout Q-B01 a Q-B10 pour bloquants architecture, cloud, database, securite et conformite
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Conformite
- Description: Matrice controle day-1 completee (identite, chiffrement, audit, isolation tenant, retention, incidents)
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Decisions
- Description: Validation des ADR D-004, D-005, D-006 en statut Accepte + ajout D-007 (perimetre conformite MVP)
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Architecture
- Description: Ajout architecture technique V1 executable (stack, decomposition repo, design DB, securite cabinet, observabilite, regles evolution)
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Planning
- Description: Ajout plan Sprint 1-3 detaille dans roadmap + initialisation backlog Sprint-ready P0-001 a P0-010
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Questions ouvertes
- Description: Fermeture Q-B01 et Q-B06, mise a jour partielle Q-B10 selon validations GO
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Prompts
- Description: Creation des prompts .github/prompts pour workflow request -> new chat (orchestrate/dev/analysis/architecture)
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Agents
- Description: Optimisation des agents Kine pour demarrage stateless en nouveau chat et sortie compacte stricte
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: SPEC
- Description: Ajout du protocole operationnel request -> new chat dans SPEC/README.md
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Pilotage
- Description: Creation de SPEC/16-project-tracking.md pour repartition multi-agents, statuts des taches P0 et journal orchestration
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Branding
- Description: Renommage public du produit en Q-INE et alignement de la documentation principale
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Backend
- Description: Activation RLS sur les tables coeur avec policies tenant_scope et tests de non-fuite cross-tenant
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Decisions
- Description: Ajout D-008 pour formaliser la gouvernance Orchestrator/Dev/Analysis/Architecture et la gestion des blocages
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Backend
- Description: Ajout Swagger/OpenAPI (Swashbuckle) sur Kine.Api pour rendre les endpoints decouvrables (/swagger); P0-007 (MFA staff obligatoire) non implemente, bloque par absence de design authn/MFA (voir SPEC/16-project-tracking.md)
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Architecture/Securite
- Description: MFA staff MVP clarifiee: enforcement via OIDC IdP + validation claim amr/acr au middleware; aucun secret MFA local MVP
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Backend/Audit
- Description: P0-008 - Journal audit append-only (Kine.Modules.Audit): AuditEvent immuable, AuditTrailService produisant une chaine de hash (prev_hash/event_hash, SHA-256), AuditChainVerifier detectant alteration/reordering, InMemoryAuditLogStore isole par tenant sans API update/delete et renvoyant un snapshot defensif. 6 tests unitaires ajoutes (chainage, isolation tenant, verification integrite, detection tampering/reorder, non-mutabilite du store).
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Backend/Patients
- Description: P0-009 - Module Patients v1 (Kine.Modules.Patients): entites Patient/PatientContact/PatientConsent tenant-scopees, PatientService CRUD (creation, mise a jour, statut, historique) avec InMemoryPatientStore isole par tenant; suppression patient implementee en soft-archive (Status=Archived) pour preserver l historique tant que la conception RGPD d effacement (Q-B15, ouverte) n est pas tranchee; consentement conserve via revocation horodatee plutot que suppression. Endpoints HTTP minimalistes ajoutes sur Kine.Api (/api/patients CRUD + sous-ressources contacts/consents), scopes par tenant via TenantContextMiddleware. 13 tests unitaires (PatientService) + 5 tests d integration (endpoints, isolation cross-tenant, archivage) ajoutes. UI staff CRUD (Kine.Web) non couverte dans ce lot: aucune maquette/convention UI formulaire n existe encore dans SPEC; a cadrer dans un lot dedie.
- Auteur: Agent
