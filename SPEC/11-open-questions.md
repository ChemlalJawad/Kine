# 11 - Questions ouvertes

Format
- ID
- Question
- Blocant: Oui | Non
- Owner
- Date cible
- Reponse
- Statut

Questions initiales
- Q-001: Details operatoires eFact/eAttest pour kine
- Q-002: Regles exactes de retention par type de document
- Q-003: Contraintes d interop eHealth obligatoires au MVP

Questions bloquantes architecture/cloud/database
- ID: Q-B01
- Question: Mode isolation tenant MVP (shared schema + tenant_id, schema par tenant, DB par tenant)
- Blocant: Oui
- Owner: Architecte
- Date cible: 2026-07-08
- Reponse: Valide - shared schema + tenant_id obligatoire + RLS
- Statut: Ferme

- ID: Q-B02
- Question: Mode gestion des cles (KMS cloud uniquement vs cles client)
- Blocant: Oui
- Owner: Security
- Date cible: 2026-07-08
- Reponse:
- Statut: Ouvert

- ID: Q-B03
- Question: Integrations eHealth/eFact/eAttest obligatoires au MVP
- Blocant: Oui
- Owner: Compliance
- Date cible: 2026-07-10
- Reponse:
- Statut: Ouvert

- ID: Q-B04
- Question: Retention exacte par type (clinical, facture, remboursement, audit, backup)
- Blocant: Oui
- Owner: DPO
- Date cible: 2026-07-10
- Reponse:
- Statut: Ouvert

- ID: Q-B05
- Question: Niveau de preuve d audit requis pour remboursement
- Blocant: Non
- Owner: Compliance
- Date cible: 2026-07-12
- Reponse:
- Statut: Ouvert

- ID: Q-B06
- Question: Identite MVP (local auth autorise ou IdP externe obligatoire)
- Blocant: Oui
- Owner: Security
- Date cible: 2026-07-08
- Reponse: Valide - OIDC + MFA staff obligatoire
- Statut: Ferme

- ID: Q-B07
- Question: SLA incident et obligations de notification hors baseline RGPD
- Blocant: Non
- Owner: Compliance
- Date cible: 2026-07-12
- Reponse:
- Statut: Ouvert

- ID: Q-B08
- Question: Contraintes de residence backup et failover intra-UE
- Blocant: Oui
- Owner: SRE
- Date cible: 2026-07-10
- Reponse:
- Statut: Ouvert

- ID: Q-B09
- Question: Regles redaction PII/PHI dans logs et observabilite
- Blocant: Non
- Owner: Security
- Date cible: 2026-07-12
- Reponse:
- Statut: Ouvert

- ID: Q-B10
- Question: Cibles RTO/RPO MVP
- Blocant: Oui
- Owner: SRE
- Date cible: 2026-07-08
- Reponse: PITR 30 jours + restore drill mensuel (RTO/RPO exacts a finaliser)
- Statut: Partiel

- ID: Q-B11
- Question: Conditions exclusion INAMI pour kinésithérapie V1 vs V2?
- Blocant: Non
- Owner: Compliance + Product Manager
- Date cible: 2026-07-15
- Reponse:
- Statut: Ouvert
- Contexte: Remboursement MVP scope réduit; certaines conditions (age, nombre séances) peuvent être exclusions INAMI. Clarifier V1 = base seule vs inclure checks.

- ID: Q-B12
- Question: Approbation cabinet requise avant soumission INAMI dossier remboursement?
- Blocant: Non
- Owner: Product Manager
- Date cible: 2026-07-15
- Reponse:
- Statut: Ouvert
- Contexte: Processus remboursement: soumission auto ou review/approbation admin cabinet requise avant eFact? UX + audit trail.

- ID: Q-B13
- Question: Nomenclature INAMI: liste actes autorisées, montants, codes exacts à intégrer?
- Blocant: Oui
- Owner: Compliance + Product Manager
- Date cible: 2026-07-15
- Reponse:
- Statut: Ouvert
- Contexte: MVP facturation dépend codes INAMI. Source officielle + format import required. Lien SPEC/14 (reimbursement rules).

- ID: Q-B14
- Question: Audit trail: niveau de détail pour remboursement (payload complet vs summary)?
- Blocant: Non
- Owner: Security + Compliance
- Date cible: 2026-07-20
- Reponse:
- Statut: Ouvert
- Contexte: Immutabilité audit critical pour remboursement. Definir payload schema (amounts before/after, user, INAMI response, etc.).

- ID: Q-B15
- Question: Deletion/archivage données: comment respecter RGPD droit à l'oubli + rétention légale?
- Blocant: Oui
- Owner: DPO + Compliance
- Date cible: 2026-07-20
- Reponse:
- Statut: Ouvert
- Contexte: Patient demande suppression dossier; audit trail doit rester. Pseudonymisation vs deletion vs logical soft-delete? Lien Q-B04 (retention rules).

- ID: Q-B16
- Question: Certificats TLS: auto-signed (dev) vs CA-signed (prod)? Gestion rotation?
- Blocant: Non
- Owner: SRE + Security
- Date cible: 2026-07-20
- Reponse:
- Statut: Ouvert
- Contexte: Infra S1 requires TLS 1.2+ setup. Certificate automation (Let's Encrypt?) et rotation.

- ID: Q-B17
- Question: Monitoring + alerting minimale: seuils précis (erreurs, latency, incidents)?
- Blocant: Non
- Owner: SRE + Engineering
- Date cible: 2026-07-20
- Reponse:
- Statut: Ouvert
- Contexte: Architecture spec dit "alerting minimal". Définiront: auth failures, 5xx errors, backup fails, job reimbursement fails. Seuils et escalade.

- ID: Q-B18
- Question: Documentation juridique: contrat client SaaS, TOS, DPA (RGPD) requis avant MVP?
- Blocant: Non
- Owner: Legal + Compliance
- Date cible: 2026-08-01
- Reponse:
- Statut: Ouvert
- Contexte: Go-live gate requirement. Template à finaliser (DPA, confidentialité, support SLA, retention clauses).
