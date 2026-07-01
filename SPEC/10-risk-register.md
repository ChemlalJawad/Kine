# 10 - Registre des risques

Format
- ID
- Risque
- Probabilite
- Impact
- Mitigation
- Owner
- Statut

Risques initiaux
- R-001: Exigences legales incompltes ou evolutives
- R-002: Complexite remboursement plus elevee que prevu
- R-003: Fuite inter-tenant due a erreur de controle
- R-004: Dette technique si vitesse > qualite

Registre detaille
- ID: R-001
- Risque: Exigences legales incompltes ou evolutives
- Probabilite: Haute
- Impact: Haut
- Mitigation: Gate legal par release + validation references officielles
- Owner: Compliance Lead
- Statut: Ouvert

- ID: R-002
- Risque: Complexite remboursement plus elevee que prevu
- Probabilite: Haute
- Impact: Haut
- Mitigation: Scope remboursement MVP reduit + rollout progressif
- Owner: Product Manager
- Statut: Ouvert

- ID: R-003
- Risque: Fuite inter-tenant
- Probabilite: Moyenne
- Impact: Critique
- Mitigation: Tenant context obligatoire + RLS + tests anti-fuite en CI
- Owner: Security Engineer
- Statut: Ouvert

- ID: R-004
- Risque: Dette technique vitesse > qualite
- Probabilite: Moyenne
- Impact: Haut
- Mitigation: Quality gates (tests, review, check architecture)
- Owner: Engineering Manager
- Statut: Ouvert

- ID: R-005
- Risque: References officielles belges manquantes avant go-live
- Probabilite: Moyenne
- Impact: Haut
- Mitigation: Registre des sources officielles + checklist pre-go-live
- Owner: Compliance Lead
- Statut: Ouvert

- ID: R-006
- Risque: Mauvaise retention par type de document
- Probabilite: Moyenne
- Impact: Haut
- Mitigation: Matrice retention par classe de donnee + validation DPO
- Owner: DPO
- Statut: Ouvert

- ID: R-007
- Risque: Journal audit incomplet ou alterable
- Probabilite: Moyenne
- Impact: Haut
- Mitigation: Journal append-only + hash chain + acces restreint
- Owner: Platform Engineer
- Statut: Ouvert

- ID: R-008
- Risque: Modele identite insuffisant
- Probabilite: Moyenne
- Impact: Haut
- Mitigation: OIDC + MFA staff + revues acces periodiques
- Owner: Security Engineer
- Statut: Ouvert

- ID: R-009
- Risque: Restauration backup non fiable
- Probabilite: Moyenne
- Impact: Critique
- Mitigation: Drill restauration mensuel + preuve RTO/RPO
- Owner: SRE Lead
- Statut: Ouvert

- ID: R-010
- Risque: Choix cloud/DB finalises avant closure des questions bloquantes
- Probabilite: Moyenne
- Impact: Haut
- Mitigation: Gate decision architecture lie a Q-B03/Q-B04/Q-B08
- Owner: CTO/Architect
- Statut: Ouvert

- ID: R-011
- Risque: Violation isolation tenant due a erreur query (sans tenant_id filter)
- Probabilite: Moyenne
- Impact: Critique
- Mitigation: Anti-fuite tests automatises CI (gate pre-merge obligatoire) + checklist code review "Tenant context?" + S3-S4 fondations strictes
- Owner: Security Engineer + Engineering Manager
- Statut: Ouvert
- Priorite: CRITIQUE (execution immediate S1)

- ID: R-012
- Risque: Deploiement production sans preuves backup/restore operationnelles
- Probabilite: Haute
- Impact: Critique
- Mitigation: Drill restauration mensuel automatise (S1+) + RTO/RPO cibles confirmees (Q-B10) + test restore sur staging avant chaque prod deployment
- Owner: SRE Lead
- Statut: Ouvert
- Priorite: HAUTE (gate S12 pre-go-live)

- ID: R-013
- Risque: Modele remboursement incorect (actes non INAMI, regles mal implantees)
- Probabilite: Haute
- Impact: Haut
- Mitigation: Validation INAMI rules dans SPEC/14 (externe audit) + tests unitaires exhaustifs + scope MVP reduit + UAT avec 2-3 kinés reels S12
- Owner: Product Manager + Compliance
- Statut: Ouvert
- Priorite: HAUTE (S9-S10 implem)

- ID: R-014
- Risque: Code/secrets exposes en repo ou logs
- Probabilite: Moyenne
- Impact: Critique
- Mitigation: Secret vault obligatoire (KMS ou Azure KeyVault) + git pre-commit hooks (secret scanning) + PII redaction logs (no email/phone/SSN) + code review secret check
- Owner: Security Engineer
- Statut: Ouvert

- ID: R-015
- Risque: Delai fermeture Q-B03/Q-B04/Q-B08/Q-B10 retarde S1 start ou impose pivot architecture
- Probabilite: Moyenne
- Impact: Haut
- Mitigation: Escalade immedite (cette semaine) aupres Compliance/DPO/SRE + decision interim (mock eHealth, placeholder retention rules) permettant dev continue
- Owner: CTO + Product Manager
- Statut: Ouvert
- Priorite: CRITIQUE (cette semaine)
