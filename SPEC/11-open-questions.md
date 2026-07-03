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

- ID: Q-B19
- Question: IdP OIDC retenu et signal d assurance MFA (claim amr/acr/equivalent) pour refuser tout login staff sans MFA?
- Blocant: Oui
- Owner: Security
- Date cible: 2026-07-08
- Reponse:
- Statut: Ouvert
- Contexte: P0-007 dépend d une preuve machine-lisible de MFA côté IdP; sans cela, impossible de définir le gate d authentification de façon déterministe.

- ID: Q-B20
- Question: Mutuelle/diagnostic/compteur de seances doivent-ils rester des champs plats sur Patient, ou etre portes par les futurs modules Clinical (diagnostic, plan de traitement, seances reelles) et Reimbursement (mutuelle/organisme assureur)?
- Blocant: Non
- Owner: Product Manager + Architecte
- Date cible: 2026-07-20
- Reponse: Tranche par l'utilisateur (2026-07-03) pour le volet seances: "+ Ajouter une seance" cree desormais une vraie SeanceClinique (Kine.Modules.Clinical, auditee via seance_created) et le compteur affiche est DERIVE du nombre de seances reelles; Patient.SessionsDone n'est plus ni affiche ni edite (champ conserve en base pour retro-compatibilite API, deprecie). Mutuelle/Diagnosis restent des champs plats Patient en attendant Clinical (plan de traitement) et Reimbursement complets.
- Statut: Partiellement ferme (seances tranchees; mutuelle/diagnostic restent ouverts)
- Contexte: Ajoutes en placeholder MVP sur Patient (Mutuelle, Diagnosis, SessionsPrescribed, SessionsDone) pour matcher le mockup design (dossier patient avec barre de progression). Aucune validation INAMI, aucun lien avec de vrais rendez-vous/actes. A ne pas considerer comme le modele cible une fois Clinical/Reimbursement implementes -- prevoir une migration de donnees le moment venu.

  Analyse complementaire (audit page-a-page vs mockup, 2026-07-02): deux champs voisins ont ete tranches differemment dans le frontend actuel et illustrent la tension au coeur de Q-B20.
  - "Telephone" et "Derniere seance" affiches sur le dossier patient sont desormais **derives** (PatientContact pour le telephone; dernier Appointment au statut Termine pour la derniere seance), pas stockes en flat-field -- il n'existe aucune source de verite honnete pour un champ plat ici puisque des entites reelles (Contact, Appointment) portent deja cette information.
  - "SessionsDone" (compteur affiche dans la barre de progression) reste, lui, un entier manuel sur Patient sans lien avec les Appointments reels: aucune entite "seance clinique" n'existe encore (Kine.Modules.Clinical est un scaffold vide), donc il n'y a pas d'equivalent honnete a deriver -- contrairement a Billing (voir ci-dessous), ou une vraie entite est apparue.
  - Precedent utile: durant cette meme session, Kine.Modules.Billing est passe de "placeholder maquette" a un vrai module (Invoice, InvoiceStatus, ActeInamiCatalog, endpoints CRUD, store in-memory tenant-scope) implemente en parallele. Cela demontre le chemin de migration pour SessionsDone: le jour ou Clinical expose une entite "Seance"/"ActeClinique" reelle, SessionsDone devrait devenir un COMPTE derive (COUNT des seances reelles terminees) plutot qu'un entier modifie a la main -- exactement comme "Derniere seance" l'est deja pour Scheduling. D'ici la, le compteur manuel reste un placeholder assume, pas une decision d'architecture cible.
  - Decision requise (non prise ici, cf. politique "no autonomous scope decisions"): faut-il, avant meme l'implementation complete de Clinical, faire de "+ Ajouter une seance" une action qui cree un enregistrement reel et tracable (ne serait-ce qu'un stub minimal dans Clinical ou reutilisant Scheduling/Appointment avec un flag "seance de suivi"), plutot que d'incrementer un entier sans trace ni audit? Cf. Q-B21/Q-B22 ci-dessous pour deux questions connexes decouvertes durant le meme audit.

- ID: Q-B21
- Question: Agenda -- vue par jour (groupement des creneaux avec en-tete "Mardi 1 juillet 2026" façon mockup 1b) doit-elle rester une simple presentation groupee du planning existant, ou evoluer vers une vraie navigation jour-par-jour (selecteur de date, pagination, filtre praticien)?
- Blocant: Non
- Owner: Product Manager + Design
- Date cible: 2026-07-15
- Reponse:
- Statut: Ouvert
- Contexte: Le regroupement par jour (helpers formatFullDay/dayKey, sections .agenda-day) est desormais implemente et fonctionnel dans AgendaPage -- tous les creneaux sont charges puis groupes cote client, sans navigation ni filtre de date. Suffisant pour le volume demo actuel; a rediscuter si le volume de creneaux par cabinet grandit (pagination/perf) ou si le mockup 1b implique une navigation explicite jour precedent/suivant non encore construite.

- ID: Q-B22
- Question: Le vocabulaire de statut Appointment (Confirme/Annule/No-show/Termine, 4 valeurs) est-il suffisant pour couvrir les besoins reels du cabinet (ex: distinguer "reporte" de "annule", ou "en cours" pendant la seance), ou faut-il enrichir l'enum avant que Clinical/Reimbursement ne s'y accrochent?
- Blocant: Non
- Owner: Product Manager
- Date cible: 2026-07-20
- Reponse:
- Statut: Ouvert
- Contexte: Le mapping actuel (appointmentStatusLabels/appointmentStatusBadgeClass) est un mirroir direct de l'enum backend AppointmentStatus a 4 valeurs, elle-meme reprise du mockup. Toute extension (ex: "Reporte") impacterait Scheduling (domaine), le frontend (labels/badges), et potentiellement Billing (une seance Reportee ne devrait pas generer de facture). A trancher avant que Clinical ne commence a consommer ce statut pour deriver SessionsDone (cf. Q-B20).
