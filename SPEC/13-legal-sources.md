# 13 - Sources légales officielles (Belgique)

Registre centralisé des sources officielles pour conformité MVP et compliance gate par release.

## Sources institutionnelles

### Santé et médecine
- **INAMI** (Institut National d'Assurance Maladie-Invalidité)
  - Site officiel: https://www.inami.fgov.be
  - Répertoire actes (nomenclature): Codes INAMI pour kinésithérapie
  - Règles remboursement: Barèmes, conditions, récupération
  - eAttest (attestations numériques)
  - eFact (facturation numérique)
  
- **FPS Santé Publique** (Fédéral Public Service Santé)
  - Directives exercice professionnel
  - Normes hygiène et sécurité
  
- **Ordre des Kinésithérapeutes Belgique** (Professional body)
  - Code éthique
  - Normes pratique
  
### Protection données (RGPD, PDPA)
- **CNIL Belgique** (Commission Nationale pour la Protection de la Vie Privée)
  - Directives RGPD secteur santé
  - Conformité droit d'accès patient
  - Sécurité données médicales (PII/PHI)
  - Retention rules guidance
  
### Sécurité et cryptographie
- **Gouvernement Belgique** (Cybersecurity)
  - Critères chiffrement national
  - Norms TLS/certificats
  
- **ANSSI France** (si applicable, baseline UE)
  - Recommandations chiffrement AES-256
  - Standards TLS 1.2+

## Mapping SPEC → Sources

| SPEC section | Question | Source | Status |
|---|---|---|---|
| 02-Requirements (non-fonctionnelles) | Conformité secteur santé | INAMI + FPS | À confirmer |
| 03-Architecture (sécurité) | Chiffrement, audit, RLS | CNIL Belgique | À confirmer |
| 04-Data-Model (retention) | Rétention données cliniques, factures, audit | INAMI + RGPD Belgique | **Q-B04 ouvert** |
| 05-Compliance | eHealth, eFact, eAttest intégrations | INAMI + eDossier Santé | **Q-B03 ouvert** |
| 06-Decisions | Key management (KMS vs bring-key) | CNIL + FPS | **Q-B02 ouvert** |

## Validation pré-go-live (Gate légal S12)

Checklist de conformité par source avant déploiement production:

- [ ] **INAMI - nomenclature actes:** Tous actes facturable mappés code INAMI
- [ ] **INAMI - règles remboursement:** Logique calcul validée INAMI
- [ ] **eFact/eAttest:** Intégration validée (si MVP) ou mockée (si scope fermé Q-B03)
- [ ] **RGPD/CNIL:** Droit d'oubli, droit d'accès implémentés; PII/PHI en AES-256
- [ ] **Retention:** Matrice par type document signée DPO (Q-B04 closed)
- [ ] **Audit trail:** Journal append-only audit fonctionnel (hash chain validé)
- [ ] **Chiffrement:** TLS 1.2+ + at-rest encryption selon Q-B02 décision

## Dates cibles de validation

- **2026-07-10:** Q-B03 fermé (eHealth scope decision)
- **2026-07-10:** Q-B04 fermé (retention rules signées DPO)
- **2026-07-20:** Sources officielles vérifiées pour scope final
- **2026-12-10:** Gate légal complet pre-go-live

## Propriétaire

**Owner:** Compliance Lead + DPO
**Reviewed:** CTO, Product Manager
**Updated:** À chaque release (at least)
