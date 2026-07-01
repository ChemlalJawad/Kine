# 08 - Roadmap

Version cible
- MVP en 6 mois (objectif de reference).

Phases
1. Cadrage legal et perimetre.
2. Fondation architecture et securite.
3. Modules coeur: patient, agenda, facturation, remboursement.
4. Conformite operationnelle minimale.
5. Pilote terrain et ajustements.

Sorties attendues par phase
- Liste des livrables
- Definition of done
- Risques et mitigations

Plan execution Sprint 1-3 (2 semaines par sprint)

Sprint 1 - Fondation plateforme
- Livrables
- Skeleton backend .NET 8 modular monolith + skeleton frontend React.
- CI pipeline (build + tests unitaires + lint).
- Infra dev/staging/prod en Terraform (network, DB, storage, vault).
- Authn de base OIDC + gestion tenant context middleware.
- Definition of done
- Build vert en CI.
- Environnements dev/staging provisionnes.
- Endpoint health + auth de base fonctionnels.

Sprint 2 - Multi-tenant securite audit
- Livrables
- RLS activee sur tables de base Identity/Patients.
- RBAC par tenant (roles cabinet) + MFA staff enforcee.
- Journal audit append-only branche sur actions sensibles.
- Tests integration anti-fuite tenant.
- Definition of done
- Tests RLS/RBAC verts.
- Audit produit des evenements sur create/update/read sensibles.
- Aucun acces cross-tenant dans la suite de tests.

Sprint 3 - Domaine patient + agenda v1
- Livrables
- Modules Patients et Dossier clinique v1 (CRUD + consentements).
- Module Agenda v1 (slots, rendez-vous, annulation).
- UI staff minimal pour parcours patient -> rdv.
- Definition of done
- Parcours e2e patient -> rdv passe en staging.
- Logs audit presents pour operations sensibles.
- KPI techniques minimum exposes (latence, erreurs).
