# 14 - Règles métier remboursement

## Scope MVP

Remboursement de base en Belgique (INAMI) pour kinésithérapie.
Scope **réduit V1** → progression controlée V2/V3 (voir roadmap évolution).

**Blocant:** Dépend Q-B03 (eHealth/eFact scope).

## Flux remboursement V1 (Simplifié)

```
1. Praticien effectue séance
   └─> Dossier clinique + acte INAMI enregistrés

2. Cabinet génère facture (actes + montants)
   └─> Fact approbation cabinet OK

3. Cabinet (AdminCabinet role) crée dossier remboursement
   └─> Sélectionne factures + actes
   └─> Event: dossier_créé

4. [TBD Q-B03] Soumission eFact/eAttest à INAMI (ou mock)
   └─> Event: dossier_soumis

5. [TBD] Suivi statut INAMI (approved, rejected, pending)
   └─> Event: dossier_approuvé / dossier_rejeté / dossier_correction_requise

6. [Si rejet/correction] Cabinet corrige + resoumission
   └─> Event: dossier_corrigé

7. [Approuvé final] Remboursement INAMI crédité (hors plateforme, tracking seul)
   └─> Event: remboursement_validé
```

## Calcul montants (Règles INAMI)

**Format:** Code acte INAMI → montant officiel (source INAMI).

### Exemple (à confirmer INAMI):
| Code INAMI | Description | Montant HT | Tarif patient | Couverture INAMI |
|---|---|---|---|---|
| 101001 | Kiné séance 30min | 30€ | 5€ | 25€ |
| 101002 | Kiné séance 60min | 55€ | 10€ | 45€ |
| 101005 | Bilan kinésique | 50€ | 10€ | 40€ |

**Règles MVP:**
- 1 séance = 1 acte INAMI uniquement
- Pas de cumul actes par séance V1
- Montants fixes officiels (pas de remise/avenant)
- Arrondi à 0.01€ (devises EUR)

## États dossier remboursement

```
DRAFT           (créé, non soumis)
SUBMITTED       (soumis à INAMI)
PENDING         (en cours INAMI)
APPROVED        (approuvé, remboursement futur)
REJECTED        (rejeté INAMI, raison loggée)
CORRECTION_REQ  (correction INAMI requise, détails loggés)
CORRECTED       (corrigé et resoumis)
COMPLETED       (remboursement finalisé)
ARCHIVED        (clôturé, traçabilité maintenue)
```

**Transitions autorisées:**
- DRAFT → SUBMITTED
- SUBMITTED → PENDING
- PENDING → APPROVED | REJECTED | CORRECTION_REQ
- CORRECTION_REQ → CORRECTED
- CORRECTED → SUBMITTED
- APPROVED | REJECTED → COMPLETED
- COMPLETED → ARCHIVED

## Audit trail remboursement

Toute transition d'état + modification = enregistrement audit_logs:
```json
{
  "actor_id": "user_uuid",
  "action": "reimbursement_case_status_changed",
  "entity": "ReimbursementCase",
  "entity_id": "case_uuid",
  "previous_status": "DRAFT",
  "new_status": "SUBMITTED",
  "timestamp": "2026-07-15T10:30:00Z",
  "payload": {
    "submissionRef": "EFACT-2026-001",
    "inami_response": null
  },
  "tenant_id": "cabinet_uuid"
}
```

## Cas complexes (V2+, hors MVP)

- Cumul actes (bilan + séance dans 1 facture) → V2
- Remises/convention tarifs → V2
- Tiers payant intégration → V2
- Franchises/déductibles patient → V2
- Exclusions médicales (conditions non remboursables) → V2
- Appel rejet INAMI → V2

## Données requises par acte (MVP)

```
Facture
├─ Acte
│  ├─ code_inami (101001, 101002, ...)
│  ├─ description
│  ├─ date_execution (séance date)
│  ├─ montant_brut
│  ├─ montant_remboursable
│  ├─ patient_id
│  ├─ praticien_id
│  └─ ...
└─ ...

ReimbursementCase
├─ factures_ids []
├─ actes_ids []
├─ statut
├─ submission_ref (eFact ID si soumis)
├─ inami_response (JSON brut réponse INAMI si reçue)
└─ ...
```

## Questions ouvertes → Q-B03, Q-B04

1. **Q-B03:** eAttest/eFact intégration MVP requis? Ou mock OK?
2. **Q-B04:** Rétention dossier remboursement approuvé? (3y? 5y? 10y?)
3. **[À ajouter Q-B11]:** Conditions exclusion INAMI à implémenter V1 ou V2?
4. **[À ajouter Q-B12]:** Approbation automatique ou review cabinet avant soumission?

## Propriétaire

**Owner:** Product Manager + Compliance (INAMI rules)
**Reviewed:** Kinésithérapeute pilote (domain expert)
**Updated:** À chaque release (regs évolutions)

**Last validated:** 2026-07-01 (architecture review)
**Next gate:** 2026-07-15 (validation INAMI rules)
