# 05 - Conformite Belgique et RGPD

Objectif
- Lister obligations et preuves associees.

Cadre
- RGPD: minimisation, droits, securite, breach process.
- Exigences sante Belgique: a confirmer et versionner.
- Exigences facturation et remboursement: a confirmer et versionner.

Matrice obligation -> implementation -> preuve
- Obligation:
- Implementation produit:
- Preuve attendue:
- Statut:
- Owner:

- Obligation: Controle d acces aux donnees de sante
- Implementation produit: RBAC par tenant + MFA obligatoire staff
- Preuve attendue: Matrice roles/permissions + logs authentification + test acces refuse
- Statut: A implementer MVP
- Owner: Security

- Obligation: Protection des donnees en transit et au repos
- Implementation produit: TLS 1.2+ partout + AES-256 DB/objets/backups
- Preuve attendue: Config TLS, config chiffrement, preuve backup chiffre
- Statut: A implementer MVP
- Owner: Platform

- Obligation: Tracabilite des acces/modifications sensibles
- Implementation produit: Journal audit append-only (user_id, tenant_id, action, horodatage)
- Preuve attendue: Extraits audit + verification hash chain
- Statut: A implementer MVP
- Owner: Platform

- Obligation: Isolation des cabinets (tenants)
- Implementation produit: tenant_id obligatoire + RLS + tests anti-fuite
- Preuve attendue: Policies DB + rapports tests isolation CI
- Statut: A implementer MVP
- Owner: Security

- Obligation: Retention/suppression conforme
- Implementation produit: Matrice retention par type + jobs retention + legal hold
- Preuve attendue: Politique retention versionnee + execution jobs
- Statut: En clarification
- Owner: DPO

- Obligation: Gestion des incidents et breach
- Implementation produit: Runbook incident + workflow escalation + journal incidents
- Preuve attendue: Procedure documentee + exercice simulation
- Statut: A implementer MVP
- Owner: Compliance

Incidents
- Procedure de declaration.
- Delais de notification.
- Journal des incidents.

Actions ouvertes
- Completer avec references officielles valides.
- Verifier periodicite de retention par type de donnee.
