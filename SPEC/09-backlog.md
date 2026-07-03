# 09 - Backlog produit

Priorite P0
- Parcours complet RDV -> seance -> facture -> remboursement.
- Gestion patient et dossier clinique minimal.
- Journal audit pour operations sensibles.

Priorite P1
- Reporting activite et financier.
- Portail patient de base.

Priorite P2
- Optimisations, automatisations avancees, extensions.

Format item
- ID
- Titre
- Description
- Critere acceptance
- Priorite
- Statut

Items initialises (Sprint-ready)

- ID: P0-001
- Titre: Initialiser backend modular monolith .NET 8
- Description: Creer structure modules et contracts internes
- Critere acceptance: Build passe, modules references, health endpoint actif
- Priorite: P0
- Statut: Done

- ID: P0-002
- Titre: Initialiser frontend React TypeScript
- Description: Base SPA staff cabinet avec routing et auth shell
- Critere acceptance: App chargee, login route disponible
- Priorite: P0
- Statut: Done

- ID: P0-003
- Titre: Provisionner infra dev/staging/prod
- Description: Terraform pour reseau prive, DB, storage, vault
- Critere acceptance: Plan/apply reussi sur env dev, state versionne
- Priorite: P0
- Statut: Done

- ID: P0-004
- Titre: Implementer tenant context middleware
- Description: Injection tenant obligatoire depuis token pour chaque requete
- Critere acceptance: requete sans tenant rejetee
- Priorite: P0
- Statut: Todo

- ID: P0-005
- Titre: Activer RLS sur tables coeur
- Description: Policies SQL sur Identity/Patients et tests associes
- Critere acceptance: tests anti-fuite cross-tenant verts
- Priorite: P0
- Statut: Done

- ID: P0-006
- Titre: Implementer RBAC par cabinet
- Description: Roles AdminCabinet/Kine/Assistant/Billing et permissions minimales
- Critere acceptance: matrice permissions testee
- Priorite: P0
- Statut: Done (RbacMiddleware: roles via claims OIDC ou header X-Roles en dev/demo, matrice par zone API testee en integration; enforcement complet pour toute requete arrivera avec l'OIDC reel)

- ID: P0-007
- Titre: MFA staff obligatoire
- Description: Forcer MFA sur tous comptes staff
- Critere acceptance: login staff sans MFA refuse
- Priorite: P0
- Statut: Fait (gate claims OIDC amr/acr, sans stockage MFA local)

- ID: P0-008
- Titre: Journal audit append-only
- Description: Tracer acces et modifications sensibles avec hash chain
- Critere acceptance: events audit produits et verificables
- Priorite: P0
- Statut: Done (AuditTrailService branche sur toutes les mutations sensibles Patients/Scheduling/Billing/Clinical/Reimbursement; endpoints /api/audit/events et /api/audit/verify; chaine verifiee en test d'integration)

- ID: P0-009
- Titre: Module Patients v1
- Description: CRUD patient, contacts, consentements
- Critere acceptance: APIs et UI staff operationnelles
- Priorite: P0
- Statut: Done (API + UI staff minimales)

- ID: P0-010
- Titre: Module Agenda v1
- Description: Slots praticien, creation rdv, annulation/no-show
- Critere acceptance: parcours patient->rdv valide en staging
- Priorite: P0
- Statut: Done (API + UI staff minimale, tests verts; validation staging hors perimetre local)

- ID: P0-011
- Titre: Module Facturation v1
- Description: Facture par acte INAMI (catalogue statique demo), statuts Pending/Reimbursed/Rejected, endpoints /api/billing, UI staff (creation + transitions), KPIs Dashboard derives
- Critere acceptance: creation facture depuis un patient, transitions de statut tracees, isolation tenant testee
- Priorite: P0
- Statut: Done (API + UI + tests; montants INAMI = placeholders a valider, cf. SPEC/14 et Q-B03; workflow eFact complet = futur module Reimbursement)

- ID: P0-012
- Titre: Module Clinical v1 (seances cliniques)
- Description: SeanceClinique reelle (date, note, lien rdv optionnel), endpoints /api/clinical, UI dossier patient (creation en popup, liste, progression derivee), audit seance_created; resout le volet seances de Q-B20
- Critere acceptance: seance creee/listee par patient, isolation tenant testee, compteur derive affiche
- Priorite: P0
- Statut: Done (API + UI + tests; notes texte libre MVP, structure clinique riche = post-MVP)

- ID: P0-013
- Titre: Module Reimbursement v1 (dossiers INAMI)
- Description: ReimbursementCase regroupant des factures, machine a etats SPEC/14 (9 etats, transitions validees), soumission eFact mockee (Q-B03), audit des transitions, UI dossiers dans la page Facturation
- Critere acceptance: creation dossier depuis factures en attente, transitions valides/invalides testees, isolation tenant testee
- Priorite: P0
- Statut: Done (mock eFact assume tant que Q-B03 est ouvert; reponse INAMI reelle non integree)

- ID: P1-001
- Titre: Reporting v1
- Description: Agregats read-only par mois (rdv, no-shows, seances, CA facture/rembourse) via /api/reporting/summary + export CSV + page Reporting (acces AdminCabinet)
- Critere acceptance: agregats corrects sur donnees de test, export CSV telechargeable, acces refuse aux autres roles
- Priorite: P1
- Statut: Done (agregation composee dans la couche API; module Reporting reste un marqueur tant qu'aucune persistence dediee n'existe)

- ID: P1-002
- Titre: Pipeline CI
- Description: GitHub Actions: build + tests .NET, typecheck + build frontend
- Critere acceptance: workflow vert sur push/PR main
- Priorite: P1
- Statut: Done (.github/workflows/ci.yml; couverture 60% et analyse statique restent a ajouter, cf. SPEC/15)
