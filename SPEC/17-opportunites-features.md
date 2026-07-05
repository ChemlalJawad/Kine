# 17 - Opportunites features & optimisations

Analyse produite le 2026-07-05 par 3 agents (etat des lieux SPEC/code, benchmark marche web, revue technique).
Statut: PROPOSITIONS — aucune decision produit prise; a valider par l'utilisateur avant implementation (cf. politique agents).

## 1. Contexte marche (benchmark web, sources dans le texte)

Le marche belge est structure autour des logiciels homologues INAMI (condition de la prime telematique de 800 EUR/an pour le kine). Acteurs homologues: Crossuite (KineO), CGM Oxygen (Compufit), KineQuick et CareConnect Physiotherapist (Corilus), Fysionotes, Kin&, KinPlus. A cote: Rosa (agenda/RDV en ligne + eBilling, agenda de base gratuit, premium ~30 EUR/mois), Kin'Easy (~29 EUR/mois HTVA, positionnement prix agressif), Doctena/Progenda/Doctoranytime (RDV), Physitrack/PhysiApp (exercices a domicile + telesante, non integre nativement par les logiciels belges), Medispring (cooperative, module kine en construction). Doctolib est quasi absent de Belgique.

Ticket d'entree du marche (standards de facto):
- Homologation INAMI/eHealth (argument de vente n.1: prime 800 EUR/an).
- MyCareNet: eFact (tiers payant), eAttest (comptant, OBLIGATOIRE kines au 01/01/2027), eAgreement (Fa/Fb/liste E), consultation assurabilite (MemberData).
- Nomenclature INAMI art. 7 a jour: pseudo-codes, pathologie courante/Fa/Fb/E, BIM, indexations (convention M/25 2025-2026; +2,72% au 01/01/2026: seance courante 31,64 EUR cabinet / 34,80 EUR domicile). Grille officielle: inami.fgov.be/SiteCollectionDocuments/tarif_kinesitherapeutes_20260101.pdf
- Prise de RDV en ligne patient + rappels SMS/email.
- Facturation complete: attestations, tiers payant, notes d'honoraires, suivi paiements, export comptable.
- Dossier kine conforme: prescription (validite 2 mois, contenu obligatoire), bilans, pieces justificatives horodatees AVANT attestation.
- Cloud multi-appareils, multi-praticiens/multi-sites, migration depuis concurrents.

Differenciateurs observes chez 1-2 acteurs seulement: portail patient + paiement en ligne + kiosk (Crossuite), suite IA (Crossuite AIQU; en France: KineFlow/Maiia Bilan pour bilan par dictee vocale), exercices a domicile avec suivi d'adherence (Physitrack, personne ne l'integre nativement en Belgique), prediction no-show, site web du cabinet inclus, souverainete des donnees (Medispring), pricing transparent (Kin'Easy) vs devis opaques (CGM/Corilus).

## 2. Propositions de features (priorisees, a valider)

### Vague A — Parite marche critique (pre-requis commercial)
- F-A1 Moteur de nomenclature INAMI art. 7: pseudo-codes reels, situations pathologiques (courante/Fa/Fb/E), compteurs de seances par situation, regle des 25%, statuts BIM, indexation annuelle parametrable. Remplace le catalogue statique demo (P0-011). Depend de Q-B13.
- F-A2 Integration MyCareNet reelle: eFact, eAttest (deadline legale 01/01/2027), eAgreement, MemberData. Le module Reimbursement (machine 9 etats) est deja concu pour s'y brancher; leve le mock Q-B03.
- F-A3 Trajectoire homologation INAMI/eHealth: certificat eHealth, agrement CIN, exigences dossier kine reglementaire. Sans elle, pas de prime 800 EUR pour le client = handicap commercial majeur.
- F-A4 Gestion de la prescription medicale: entite Prescription (medecin, date, diagnostic, nb seances, validite 2 mois), lien seances->prescription, alerte expiration et quota atteint.
- F-A5 Rappels RDV SMS/email avec confirmation active (reduit les no-shows, standard marche).

### Vague B — Completude cabinet
- F-B1 Prise de RDV en ligne patient (page publique par cabinet, creneaux ouverts a la reservation, anti-doublon).
- F-B2 Multi-praticiens reel: agenda par kine, RDV rattache a un praticien, vue cabinet.
- F-B3 Documents et exports: attestation/facture PDF, courrier de fin de traitement au prescripteur, export comptable (CSV/Winbooks).
- F-B4 Portail patient v1 (deja P1 backlog): factures, documents, prescriptions, prochains RDV.
- F-B5 Statistiques cabinet enrichies: CA par pseudo-code, taux no-show, seances restantes par accord, suivi seuil 500 prestations (prime telematique).

### Vague C — Differenciation
- F-C1 Exercices a domicile integres au dossier (programmes, videos, adherence, douleur) — gap marche belge, personne ne l'integre nativement.
- F-C2 Aide IA a la redaction du bilan kine (dictee vocale -> bilan structure pre-rempli), garde-fous RGPD/AI Act.
- F-C3 Score de risque no-show + rappels adaptatifs.
- F-C4 Tele-readaptation (visio liee agenda + dossier).
- F-C5 Positionnement confiance: mettre en avant audit hash-chaine, RLS, hebergement UE — deja aligne avec l'architecture Q-INE.

## 3. Optimisations techniques (revue de code, actionnables sans DB)

### Critiques
- T-1 Chaine d'audit non atomique (AuditTrailService.cs l.35-38): 2 requetes concurrentes meme tenant => meme PrevHash => AuditChainVerifier.IsValid false a jamais (fausse detection de tampering). Fix: AppendWithChain atomique dans IAuditLogStore. Aucun test de concurrence existant.
- T-2 Double reservation possible (SchedulingService.BookAppointment l.56-94): check IsBooked et ecriture non couverts par un meme lock. Fix: TryBookSlot atomique dans le store.
- T-3 PatientsEndpoints: ArgumentException non catchee => 500 au lieu de 400 (seul module incoherent). Fix recommande: IExceptionHandler global ProblemDetails (ArgumentException->400, KeyNotFoundException->404, InvalidOperationException->409), supprime ~80% des try/catch dupliques.
- T-4 Sous-ressources sans controle d'appartenance: PUT/DELETE contacts et RevokeConsent ignorent le patientId de la route; DELETE contact inexistant => 204 + faux event d'audit. Le journal (preuve legale) contient des donnees fausses.

### Importants
- T-5 References inter-modules non validees (facture/dossier/seance vers patientId inexistant) — verifier l'existence cote API en attendant les FK PostgreSQL.
- T-6 UpdatePatient: null = inchange => impossible d'effacer mutuelle/diagnostic; front envoie '' (incoherent avec create).
- T-7 Duplication Tenant()/Actor() dans 7 fichiers *Endpoints.cs — extensions HttpContext partagees.
- T-8 Couverture non mesurable: ajouter coverlet.collector (cible 60% SPEC/15). Tests anti-tenant-leak OK; manquent: concurrence audit, double-booking, cas 400 Patients.
- T-9 Frontend: zero loading state (double-submit possible), reponses out-of-order sur PatientsPage (selection rapide => donnees d'un autre patient affichees; fix AbortController), edition mutant directement la liste affichee (separer editDraft).
- T-10 Accessibilite: Modal sans role=dialog/aria-modal/focus trap; messages erreur/succes sans aria-live; erreurs backend affichees brutes (anglais) dans une UI francaise.
- T-11 Lint/tests frontend absents (ESLint/Prettier/vitest) malgre la gate prevue SPEC/15.

### Nice-to-have
- Normaliser tenant id (casse/charset) dans TenantContextMiddleware; decouper PatientsPage.tsx (780 lignes); hook useAuthHeaders() commun; contrat de pagination des listes AVANT la DB (evite un breaking change); N+1 dans ReportingEndpoints a ne pas porter vers PostgreSQL.

### Doit attendre PostgreSQL
FK/unicite reelles, RLS effective (script p0-005 pret), atomicite mutation+audit (outbox), RBAC contraignant avec OIDC reel.

## 4. Sequencement suggere (a valider)

1. Quick wins techniques T-1..T-4 (protegent la conformite et la fiabilite, petits fixes).
2. F-A4 Prescription + F-A1 nomenclature reelle (debloque la credibilite metier; F-A1 depend de Q-B13).
3. F-A5 rappels + F-B2 multi-praticiens + T-9/T-10 (qualite percue).
4. F-A2/F-A3 MyCareNet + homologation (gros chantier, dependances externes eHealth/CIN; deadline eAttest 01/01/2027).
5. Vague C selon retours terrain.

## 5. Questions ouvertes soulevees

- Q: homologation INAMI visee pour quelle echeance? (conditionne F-A2/F-A3, lourds).
- ~~Q: cible commerciale?~~ TRANCHEE (D-012, 2026-07-05): cabinets de groupe, concurrence frontale CGM/Corilus.
- Q: F-C1 (exercices) build vs partenariat (API Physitrack)?

Note: les quick wins techniques T-1..T-4 ont ete implementes le 2026-07-05 (P0-014..P0-017, cf. 07-change-log).
