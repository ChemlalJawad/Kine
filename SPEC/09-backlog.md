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
- Statut: Todo

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
- Statut: Todo

- ID: P0-007
- Titre: MFA staff obligatoire
- Description: Forcer MFA sur tous comptes staff
- Critere acceptance: login staff sans MFA refuse
- Priorite: P0
- Statut: Todo

- ID: P0-008
- Titre: Journal audit append-only
- Description: Tracer acces et modifications sensibles avec hash chain
- Critere acceptance: events audit produits et verificables
- Priorite: P0
- Statut: Todo

- ID: P0-009
- Titre: Module Patients v1
- Description: CRUD patient, contacts, consentements
- Critere acceptance: APIs et UI staff operationnelles
- Priorite: P0
- Statut: In progress (backend+API done; UI staff a cadrer, cf SPEC/16)

- ID: P0-010
- Titre: Module Agenda v1
- Description: Slots praticien, creation rdv, annulation/no-show
- Critere acceptance: parcours patient->rdv valide en staging
- Priorite: P0
- Statut: Todo
