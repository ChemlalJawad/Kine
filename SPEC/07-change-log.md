# 07 - Log des changements

Format
- Date:
- Type: Added | Changed | Fixed | Removed
- Zone:
- Description:
- Auteur:

Historique
- Date: 2026-07-01
- Type: Added
- Zone: Backend
- Description: Ajout du middleware de contexte tenant Q-INE avec rejet des requetes sans tenant
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Frontend
- Description: P0-009 complet avec UI staff minimale Patients (list/create/update/archive, contacts, consentements) et transmission des headers tenant/actor
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Frontend
- Description: Initialisation frontend React TypeScript pour Q-INE avec routing et shell auth minimal
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Backend
- Description: Initialisation du backend modular monolith .NET 8 avec modules references et endpoint health
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: SPEC
- Description: Creation du dossier SPEC et des fichiers de gouvernance
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Agents
- Description: Creation des agents projet specialises sous .github/agents (orchestrator, dev, analysis, architecture)
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Instructions
- Description: Ajout de .github/instructions/agents.instructions.md pour gouvernance des agents et sync SPEC
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Architecture
- Description: Proposition architecture v1 MVP (6 mois, 10-30 cabinets): decomposition modules, strategie DB multi-tenant/audit, blueprint cloud UE, modele securite, trajectoire modular monolith -> services, sequence 12 semaines
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Decisions
- Description: Ajout ADR proposes D-004 a D-006 (DECISION-REQ) pour multi-tenant DB, blueprint cloud UE et trajectoire evolution architecture
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Risques
- Description: Enrichissement registre des risques avec top 10 risques, mitigations et owners
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Questions ouvertes
- Description: Ajout Q-B01 a Q-B10 pour bloquants architecture, cloud, database, securite et conformite
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Conformite
- Description: Matrice controle day-1 completee (identite, chiffrement, audit, isolation tenant, retention, incidents)
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Decisions
- Description: Validation des ADR D-004, D-005, D-006 en statut Accepte + ajout D-007 (perimetre conformite MVP)
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Architecture
- Description: Ajout architecture technique V1 executable (stack, decomposition repo, design DB, securite cabinet, observabilite, regles evolution)
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Planning
- Description: Ajout plan Sprint 1-3 detaille dans roadmap + initialisation backlog Sprint-ready P0-001 a P0-010
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Questions ouvertes
- Description: Fermeture Q-B01 et Q-B06, mise a jour partielle Q-B10 selon validations GO
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Prompts
- Description: Creation des prompts .github/prompts pour workflow request -> new chat (orchestrate/dev/analysis/architecture)
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Agents
- Description: Optimisation des agents Kine pour demarrage stateless en nouveau chat et sortie compacte stricte
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: SPEC
- Description: Ajout du protocole operationnel request -> new chat dans SPEC/README.md
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Pilotage
- Description: Creation de SPEC/16-project-tracking.md pour repartition multi-agents, statuts des taches P0 et journal orchestration
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Branding
- Description: Renommage public du produit en Q-INE et alignement de la documentation principale
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Backend
- Description: Activation RLS sur les tables coeur avec policies tenant_scope et tests de non-fuite cross-tenant
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Decisions
- Description: Ajout D-008 pour formaliser la gouvernance Orchestrator/Dev/Analysis/Architecture et la gestion des blocages
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Backend
- Description: Ajout Swagger/OpenAPI (Swashbuckle) sur Kine.Api pour rendre les endpoints decouvrables (/swagger); P0-007 (MFA staff obligatoire) non implemente, bloque par absence de design authn/MFA (voir SPEC/16-project-tracking.md)
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Architecture/Securite
- Description: MFA staff MVP clarifiee: enforcement via OIDC IdP + validation claim amr/acr au middleware; aucun secret MFA local MVP
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Backend/Audit
- Description: P0-008 - Journal audit append-only (Kine.Modules.Audit): AuditEvent immuable, AuditTrailService produisant une chaine de hash (prev_hash/event_hash, SHA-256), AuditChainVerifier detectant alteration/reordering, InMemoryAuditLogStore isole par tenant sans API update/delete et renvoyant un snapshot defensif. 6 tests unitaires ajoutes (chainage, isolation tenant, verification integrite, detection tampering/reorder, non-mutabilite du store).
- Description: P0-007 - Ajout StaffMfaEnforcementMiddleware imposant un gate MFA base sur claims OIDC (amr/acr) pour toute requete staff authentifiee, sans stockage MFA local; enregistre dans le pipeline Kine.Api
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Backend/Patients
- Description: P0-009 - Module Patients v1 (Kine.Modules.Patients): entites Patient/PatientContact/PatientConsent tenant-scopees, PatientService CRUD (creation, mise a jour, statut, historique) avec InMemoryPatientStore isole par tenant; suppression patient implementee en soft-archive (Status=Archived) pour preserver l historique tant que la conception RGPD d effacement (Q-B15, ouverte) n est pas tranchee; consentement conserve via revocation horodatee plutot que suppression. Endpoints HTTP minimalistes ajoutes sur Kine.Api (/api/patients CRUD + sous-ressources contacts/consents), scopes par tenant via TenantContextMiddleware. 13 tests unitaires (PatientService) + 5 tests d integration (endpoints, isolation cross-tenant, archivage) ajoutes. UI staff CRUD (Kine.Web) non couverte dans ce lot: aucune maquette/convention UI formulaire n existe encore dans SPEC; a cadrer dans un lot dedie.
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Backend/Agenda
- Description: P0-010 - Module Agenda v1 (Kine.Modules.Scheduling): entites PractitionerSlot (disponibilite) et Appointment tenant-scopees, SchedulingService avec creation de creneau, reservation de rdv (BookAppointment consomme un slot libre), annulation (libere le slot) et no-show (statut fige, slot non libere) via InMemorySchedulingStore isole par tenant; transitions de statut plutot que suppression pour preserver l historique (SPEC/02). Endpoints HTTP ajoutes sur Kine.Api (/api/scheduling/slots, /api/scheduling/appointments + cancel/no-show), scopes par tenant via TenantContextMiddleware, reponses 404/409 explicites (slot/rdv introuvable vs transition invalide). 14 tests unitaires (SchedulingService) + 6 tests d integration (endpoints, reservation, conflit, annulation, no-show, isolation cross-tenant) ajoutes. UI staff minimale ajoutee (AgendaPage): creation de creneaux, reservation rdv a partir de la liste patients existante, annulation/no-show, en reutilisant le pattern Patients (headers tenant/actor, panels, formulaires).
- Auteur: Agent

- Date: 2026-07-01
- Type: Fixed
- Zone: Frontend
- Description: Correction d un bug pre-existant dans PatientsPage/AgendaPage: useAuth() n exposait pas tenantId/actorId directement (uniquement via user.tenantId/user.actorId), ce qui aurait envoye des headers tenant/actor vides a l API. Les deux pages lisent desormais tenantId/actorId depuis user. Corrige a l occasion de P0-010 car necessaire au bon fonctionnement du parcours patient -> rendez-vous.
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Frontend
- Description: Correction bug dev 404 sur /api/*: le frontend appelait des chemins relatifs sans proxy ni URL absolue (la requete atteignait le serveur Vite lui-meme au lieu de Kine.Api). Ajout d un proxy Vite (/api -> backend, cible configurable via VITE_API_PROXY_TARGET) et elargissement de la policy CORS LocalDev pour accepter tout port localhost (Vite change de port des que 5173 est occupe). Ajout de Kine.Api/Seed/DemoDataSeeder.cs pour peupler tenant-demo au demarrage (patients, contacts, consentements, creneaux/rdv du jour) afin que l UI ne soit plus vide au premier lancement.
- Auteur: Agent

- Date: 2026-07-01
- Type: Changed
- Zone: Frontend
- Description: Refonte visuelle PatientsPage pour correspondre au mockup Claude Design (option 1b, handoff zip): layout 2 colonnes (liste + dossier patient) au lieu de 3 colonnes CRUD brutes, formulaire de creation replie par defaut, bandeau de message d etat au lieu d une colonne dediee. Boutons de creation (Kine.Web): etats hover/active/focus-visible/disabled + icone "+".
- Auteur: Agent

- Date: 2026-07-01
- Type: Added
- Zone: Backend/Patients
- Description: Extension du modele Patient (Mutuelle, Diagnosis, SessionsPrescribed, SessionsDone en champs texte/entiers optionnels) pour permettre au dossier patient (Kine.Web) d afficher mutuelle, diagnostic et une barre de progression des seances comme dans le mockup design. Ce sont des champs plats sur Patient en placeholder MVP: aucune validation INAMI, aucun lien reel avec un plan de traitement ou des rendez-vous -- a remplacer par le futur module Clinical/Reimbursement. PatientService.CreatePatient/UpdatePatient et les DTOs HTTP (CreatePatientRequest/UpdatePatientRequest) etendus avec des parametres optionnels (retro-compatibles avec les appels existants et les tests). DemoDataSeeder mis a jour avec les valeurs du mockup (Sophie/Marc/Amina/Louis).
- Auteur: Agent

- Date: 2026-07-02
- Type: Changed
- Zone: Frontend
- Description: Refonte visuelle AgendaPage pour correspondre au mockup Claude Design (option 1b): panel unique pleine largeur au lieu du layout 3 colonnes CRUD brutes (disponibilites / rendez-vous / etat), avec un planning fusionnant creneaux et rendez-vous (une ligne par creneau, triee par heure, affichant le patient/statut si reserve ou "Creneau libre" sinon). Creation de creneau et reservation de rendez-vous repliees par defaut derriere des boutons bascule (meme pattern que PatientsPage), formulaire refermé automatiquement apres succes. Annulation/no-show restent inline sur chaque ligne reservee. Aucune perte de fonctionnalite CRUD.
- Auteur: Agent

- Date: 2026-07-02
- Type: Fixed
- Zone: Frontend
- Description: Correction d un bug de mise en page site-large ("boites collees"): la regle .panel partageait un max-width:720px pense uniquement pour .login-card. Chaque page appliquant un panel dans une grille (Patients, Agenda) ou pleine largeur (Dashboard, Facturation) heritait de cette limite, plafonnant certains panels a 720px pendant que leurs voisins de grille restaient plus larges -- largeurs/espacements incoherents d une page a l autre. max-width retire de la regle partagee .panel/.login-card (le login-card garde sa largeur via width:min(100%,420px) deja presente); suppression des contournements inline style={{maxWidth:'none'}} devenus inutiles sur DashboardPage/FacturationPage. Nettoyage CSS mort associe (.patients-grid, .status-panel, non references depuis la refonte Agenda).
- Auteur: Agent

- Date: 2026-07-02
- Type: Changed
- Zone: Frontend
- Description: Formulaires de creation deplaces dans des popups (nouveau composant Kine.Web/src/components/Modal.tsx: overlay + panel centre, fermeture via clic exterieur/Echap/bouton Fermer) au lieu de formulaires inline colles contre les listes/boutons: Agenda (Nouveau creneau, Nouveau rendez-vous) et Patients (Nouveau patient). Ajout d une action dediee "+ Ajouter une seance" dans le dossier patient (incremente SessionsDone de 1 et sauvegarde immediatement), en complement du formulaire d edition existant (modifier infos patient). Layout Patients confirme a deux colonnes (liste + dossier) conforme a la maquette; aucun changement necessaire sur ce point, deja en place depuis la refonte precedente.
- Auteur: Agent

- Date: 2026-07-02
- Type: Fixed
- Zone: Frontend
- Description: PatientsPage.tsx -- tous les handlers de mutation (creation patient, ajout de seance, enregistrer, archiver, contacts, consentements) appelaient l API sans try/catch: une erreur (ex. requete rejetee) echouait silencieusement, sans message affiche, laissant croire a l utilisateur que l action "ne fonctionne pas" (ex. bouton "+ Ajouter une seance"). Tous ces handlers entourent desormais l appel API d un try/catch qui alimente la bannière d erreur existante, alignes sur le pattern deja utilise dans AgendaPage.
- Auteur: Agent

- Date: 2026-07-02
- Type: Note
- Zone: Backend/Seed
- Description: Signalement utilisateur "le dashboard n a pas de planning": DemoDataSeeder ne seme les rendez-vous du jour qu au demarrage du process (skip si le store contient deja des patients), avec "aujourd hui" fige a DateTime.UtcNow.Date au moment du Seed(). Si l API Kine.Api tourne depuis un jour precedent (store in-memory jamais vide, jamais reseme), le filtre isToday() du Dashboard (base sur la date reelle courante) ne trouve plus aucun rendez-vous du jour -- comportement attendu de stores in-memory non persistes, pas un bug de code; necessite un redemarrage de l API pour reseme "aujourd hui" avec la date courante. Pas de changement de code effectue (aucune conception de reseed automatique/DB demandee); a tracker si le comportement doit devenir plus robuste.
- Auteur: Agent

- Date: 2026-07-02
- Type: Changed
- Zone: Frontend
- Description: Revue exhaustive de toutes les pages (Dashboard, Agenda, Patients, Facturation, AppShell) contre le mockup design (Q-INE Redesign.dc.html, option 1b) suite a demande explicite. Ecarts trouves et corriges: (1) badge du creneau libre en Agenda utilisait "badge-success" (vert, comme Confirme) au lieu de "badge-neutral" (gris, comme le mockup) -- corrige; (2) libelles de statut rendez-vous sans accents ("Planifie", "Annule", "Termine") et statut 0 renomme "Confirme" pour matcher le vocabulaire du mockup -- extrait dans data/appointmentStatus.ts partage entre Agenda et Dashboard (evite la duplication precedente des deux memes const dans les deux fichiers); (3) libelles de statut facture sans accents ("Rembourse", "Rejete") -- corrige et extrait dans data/facturationData.ts, partage avec le Dashboard; (4) KPIs du Dashboard ne correspondaient pas au mockup ("Patients archives"/"Total rendez-vous" au lieu de "CA du mois"/"Remboursements en attente") -- recalcules a partir des memes donnees statiques que FacturationPage (voir data/facturationData.ts: totalMontant/countPending), pour rester honnetes (pas de faux chiffres inventes) tout en affichant les bons libelles; (5) dossier patient: la grille de details montrait Date de naissance/Mutuelle/Statut/Cree par au lieu de Naissance/Mutuelle/Telephone/Derniere seance (mockup) -- Telephone derive des contacts existants (type Phone, priorite au contact primaire), Derniere seance derivee du dernier rendez-vous Termine (nouvel appel a listAppointments dans PatientsPage) -- aucun nouveau champ stocke, uniquement calcule (voir Q-B20 ci-dessous pour l analyse complete); (6) le formulaire d edition patient (Prenom/Nom/.../Enregistrer/Archiver) etait toujours visible sous la barre de progression au lieu d etre derriere un bouton "Modifier" comme le mockup -- deplace dans une popup (reutilise le composant Modal), "Archiver" y a ete deplace comme action secondaire; le dossier par defaut affiche desormais exactement les 2 boutons du mockup ("+ Ajouter une seance", "Modifier"); (7) marque/brand sidebar "Cabinet staff" -> "Cabinet Vandenberghe" (mockup); displayName demo par defaut "Staff cabinet" -> "Sophie Vandenberghe" (mockup montre "Bonjour, Sophie"), salutation Dashboard n utilise desormais que le prenom. Contacts/Consentements (fonctionnalites RGPD reelles) conserves sous les 2 boutons bien qu absents du mockup simplifie -- deviation deliberee et documentee, pas une regression.
- Auteur: Agent

- Date: 2026-07-02
- Type: Changed
- Zone: Backend/Seed
- Description: DemoDataSeeder etendu avec un rendez-vous Termine par patient (Sophie -1j, Marc -2j, Amina -7j, Louis -51j) pour que "Derniere seance" (desormais calculee, voir ci-dessus) ait une vraie valeur pour les 4 patients de demo au lieu de rester a "—" pour 3 d entre eux.
- Auteur: Agent

- Date: 2026-07-02
- Type: Added
- Zone: Backend/Frontend/Facturation
- Description: Module Facturation reel ajoute (Kine.Modules.Billing): entites Invoice/InvoiceStatus (Pending/Reimbursed/Rejected)/ActeInami, BillingService (CreateInvoice valide contre ActeInamiCatalog, MarkReimbursed/MarkRejected via transition d etat), InMemoryInvoiceStore tenant-scope, endpoints /api/billing/actes + /api/billing/invoices (+ mark-reimbursed/mark-rejected). Frontend: src/api/billingApi.ts (nouveau, consomme un httpClient partage) + FacturationPage.tsx reecrite (creation de facture en popup Modal, mutuelle auto-remplie depuis le dossier patient, montant fixe par l acte, actions inline Rembourse/Rejete sur les factures en attente). Ceci remplace le placeholder data/facturationData.ts (donnees statiques inventees, desormais supprime du repo) qui servait uniquement a satisfaire visuellement le mockup: repond directement a la question posee sur "quelles infos garder en DB" pour la facturation -- une Invoice est maintenant une entite reelle, tracable et transitionnee par etat, plutot qu un tableau statique. DashboardPage.tsx mis a jour en consequence: "CA du mois" et "Remboursements en attente" sont recalcules a partir de billingApi.listInvoices() (somme des montants factures du mois courant / compte des factures au statut Pending), plus aucune donnee inventee affichee comme un KPI.
- Auteur: Agent

- Date: 2026-07-02
- Type: Changed
- Zone: Frontend/Agenda
- Description: AgendaPage.tsx -- planning desormais regroupe par jour (helpers formatFullDay "Mardi 1 juillet 2026" / dayKey, sections .agenda-day) au lieu d une liste plate de creneaux, pour matcher la vue journee du mockup 1b. Chargement/CRUD inchange (tous les creneaux sont recuperes puis groupes cote client); aucune navigation date/pagination ajoutee -- cf. Q-B21 (SPEC/11-open-questions.md) pour la question ouverte sur une eventuelle navigation jour-par-jour explicite.
- Auteur: Agent

- Date: 2026-07-02
- Type: Fixed
- Zone: Backend/Frontend/Facturation
- Description: Restauration du module Facturation decrit dans l entree "Module Facturation reel ajoute" ci-dessous: les fichiers de code de cette session precedente etaient partiellement perdus/tronques sur disque (Kine.Modules.Billing absent, FacturationPage/Dashboard toujours sur data/facturationData.ts statique, plusieurs fichiers frontend et AuthContext.tsx/AppShell.tsx tronques en fin de fichier -- AuthContext.tsx ne compilait plus). Reimplementation complete conformement a la description existante (Invoice/InvoiceStatus/ActeInami, BillingService + ActeInamiCatalog, InMemoryInvoiceStore, endpoints /api/billing, billingApi.ts + httpClient.ts partage, FacturationPage reecrite, KPIs Dashboard recalcules depuis l API, facturationData.ts supprime, vue journee Agenda avec formatFullDay/dayKey) avec en plus: seed Billing dans DemoDataSeeder (4 factures demo alignees sur le mockup), 12 tests unitaires BillingServiceTests + 6 tests d integration BillingEndpointTests (tenant manquant, catalogue, roundtrip create->mark-reimbursed, code INAMI inconnu, double transition 409, isolation cross-tenant), et correction des dependances useEffect ([] -> [auth]) sur Dashboard/Agenda/Facturation/Patients. Fichiers tronques repares a l identique. Verification: vite build + tsc --noEmit OK; dotnet build/test a relancer cote poste local (SDK indisponible dans l environnement d execution).
- Auteur: Agent

- Date: 2026-07-03
- Type: Added
- Zone: Backend/Frontend (lot "continuer le dev de tout", valide par l'utilisateur)
- Description: (1) P0-008 complete -- AuditTrailService/InMemoryAuditLogStore enregistres dans le pipeline et branches sur toutes les mutations sensibles (patient_created/updated/archived, contacts/consentements, slot_created, appointment_booked/cancelled/no_show, invoice_created/reimbursed/rejected, seance_created, reimbursement_case_created/status_changed); nouveaux endpoints read-only /api/audit/events et /api/audit/verify (verification hash chain). (2) P0-006 complete -- RbacMiddleware avec roles AdminCabinet/Kine/Assistant/Billing extraits des claims OIDC ou du header X-Roles (fallback dev/demo, aligne sur le gate MFA: sans information de role la requete passe, des qu'un role est fourni la matrice est appliquee et retourne 403 si insuffisant); matrice par zone API (patients/scheduling: ecriture Admin+Kine+Assistant; billing/reimbursement: Admin+Billing; clinical: ecriture Admin+Kine; reporting/audit: Admin seul); frontend envoie X-Roles (AuthContext.roles, demo=AdminCabinet). (3) Clinical v1 -- Q-B20 tranche par l'utilisateur: SeanceClinique reelle (date, note, lien rdv optionnel, append-only), ClinicalService/InMemorySeanceStore, endpoints /api/clinical/patients/{id}/seances; PatientsPage: "+ Ajouter une seance" ouvre une popup et cree une seance reelle, progression derivee du COUNT de seances, section "Seances" dans le dossier, champ SessionsDone deprecie (conserve en API, plus edite). (4) Reimbursement v1 -- ReimbursementCase (factures liees par id opaque), machine a etats SPEC/14 complete (9 etats, transitions validees, tests exhaustifs), soumission eFact MOCKEE (Q-B03 ouvert: SubmissionRef locale EFACT-yyyy-xxxxxxxx, aucune integration eHealth), endpoints /api/reimbursement/cases (+ /status), section "Dossiers de remboursement" dans FacturationPage (creation depuis les factures en attente, boutons de transition contextuels). (5) Reporting v1 -- /api/reporting/summary (agregats mensuels: rdv/termines/annules/no-shows/seances/CA facture/rembourse + patients actifs/archives) et /api/reporting/export.csv, composes dans la couche API (le module Reporting reste un marqueur); nouvelle page Reporting (KPIs, tableau mensuel, export CSV) + entree de navigation. (6) CI GitHub Actions (.github/workflows/ci.yml): build+tests .NET, typecheck+build frontend. Seed demo etendu (32 seances alignees sur les anciens compteurs, 1 dossier remboursement Draft regroupant les factures en attente). Tests ajoutes: ClinicalServiceTests (8), ReimbursementServiceTests (13 dont matrice de transitions), ClinicalEndpointTests (3), ReimbursementEndpointTests (5), RbacEndpointTests (6), AuditTrailEndpointTests (2), ReportingEndpointTests (2). Verification: vite build + tsc --noEmit OK; dotnet build/test a executer cote poste local (SDK indisponible dans l'environnement d'execution).
- Auteur: Agent

- Date: 2026-07-03
- Type: Fixed
- Zone: Frontend/DevEnv
- Description: Correction des erreurs 500 signalees sur tous les appels /api en dev: la cause etait un ECONNREFUSED du proxy Vite (cible par defaut http://localhost:5000) alors que run-dev.ps1 demarre Kine.Api sur http://localhost:5080 -- aucun bug backend/"base de donnees" (les stores restent in-memory: les donnees repartent de zero a chaque redemarrage de l'API, comportement attendu tant que PostgreSQL n'est pas branche, cf. P0-003 bloque). Alignement du port: creation de src/Kine.Api/Properties/launchSettings.json (http://localhost:5080, utilise aussi par un dotnet run sans --urls) et defaut du proxy vite.config.ts passe a 5080; httpClient detecte desormais l'API injoignable (fetch KO ou 500 proxy ECONNREFUSED/HTML) et affiche un message actionnable au lieu d'un 500 opaque. Correction connexe: date de naissance effacee envoyait "" au binding DateOnly? (echec d'enregistrement) -- normalisee en null.
- Auteur: Agent

- Date: 2026-07-03
- Type: Changed
- Zone: Frontend
- Description: Modernisation de la selection date/heure: remplacement des <input type="date|datetime-local"> natifs (rendu heterogene/vieillot selon navigateurs) par react-datepicker 9 + date-fns (locale fr) via un composant partage src/components/DateTimeField.tsx (calendrier localise, dropdown mois/annee, pas de 15 min pour les heures, clearable, bornes min/max, style aligne sur le theme via overrides .react-datepicker dans styles.css, popper au-dessus des modales). Integre dans: Agenda (debut/fin de creneau, fin pre-remplie a +30 min, garde-fou fin>debut), Patients (date de naissance en creation/edition, date+heure de seance). Correction de fond au passage: les drafts stockent de vrais objets Date et la conversion API utilise toISOString() -- l'ancien format "YYYY-MM-DDTHH:mm"+"Z" naif etiquetait l'heure locale comme UTC, decalant l'affichage de 1-2h (fuseau Europe/Brussels); DateOnly (naissance) converti sans passage UTC pour eviter le decalage de jour.
- Auteur: Agent

- Date: 2026-07-02
- Type: Changed
- Zone: SPEC
- Description: Q-B20 (SPEC/11-open-questions.md) complete avec l analyse derive-vs-manuel demandee: Telephone/Derniere seance sont deriveés (source de verite = Contact/Appointment reels), SessionsDone reste un entier manuel faute d entite "seance clinique" reelle (Clinical est un scaffold vide) -- le precedent Billing (placeholder -> module reel dans la meme session) illustre le chemin de migration a suivre le jour ou Clinical existe. Ajout Q-B21 (Agenda: vue jour groupee vs navigation jour-par-jour explicite) et Q-B22 (richesse du vocabulaire de statut Appointment, notamment avant que Clinical/Billing ne s y accrochent).
- Auteur: Agent

- Date: 2026-07-05
- Type: Added
- Zone: SPEC
- Description: Analyse d'opportunites features et optimisations demandee par l'utilisateur, menee par 3 agents (etat des lieux SPEC/code, benchmark web du marche belge des logiciels kine, revue technique du code). Nouveau document SPEC/17-opportunites-features.md: contexte marche (concurrents homologues INAMI, standards de facto dont obligation eAttest kines au 01/01/2027, differenciateurs), propositions F-A/F-B/F-C priorisees, optimisations techniques T-1..T-11 (dont 4 critiques: chaine d'audit non atomique, double-booking possible, 500 au lieu de 400 sur Patients, controles d'appartenance manquants sur contacts/consents), sequencement suggere et questions ouvertes. SPEC/09-backlog.md: ajout de 13 items en statut "Propose" (P0-014..P0-017, P1-003..P1-007, P2-003..P2-005) — AUCUN engage, tous en attente de validation utilisateur conformement a la politique agents. Aucun code modifie.
- Auteur: Agent

- Date: 2026-07-05
- Type: Fixed
- Zone: Backend (P0-014..P0-017, valides par l'utilisateur depuis SPEC/17)
- Description: (1) P0-014 chaine d'audit atomique -- IAuditLogStore.Append remplace par AppendWithChain(tenantId, buildFromPrevHash): le hash precedent est resolu et l'event appende sous le lock du store (la fabrique s'execute dans le lock), fermant la course ou deux Record concurrents chainaient sur le meme PrevHash et rendaient AuditChainVerifier.IsValid faux a jamais (faux positif de tampering); garde-fous tenant/prevHash dans le store; test Parallel.For 50 writers + unicite des PrevHash. (2) P0-015 anti double-booking -- ISchedulingStore.TryReserveSlot(tenantId, slotId, nowUtc, out slot) check-and-set atomique (enum SlotReservationResult NotFound/AlreadyBooked/Reserved); SchedulingService.BookAppointment ne fait plus GetSlot/check/UpdateSlot en trois appels; test 20 bookings concurrents -> exactement 1 succes. (3) P0-016 mapping global des erreurs -- nouveau ExceptionMappingMiddleware (ArgumentException->400, KeyNotFoundException->404, InvalidOperationException->409, corps { error } inchange pour le front), enregistre en tete de pipeline; PatientService aligne sur la convention des autres modules (KeyNotFoundException pour introuvable, ex-InvalidOperationException); try/catch supprimes de PatientsEndpoints (ceux des autres modules, identiques au middleware, restent et pourront etre retires opportunement); corrige le 500 sur POST /api/patients avec firstName vide. (4) P0-017 appartenance des sous-ressources -- UpdateContact/RemoveContact/RevokeConsent prennent desormais le patientId de la route et verifient contact.PatientId/consent.PatientId (404 sinon); DELETE d'un contact inexistant retourne 404 au lieu de 204+event d'audit fantome; les events d'audit ne sont ecrits qu'apres succes du service. Tests: PatientServiceTests adaptes (KeyNotFoundException, nouvelles signatures) + 3 tests d'appartenance; AuditTrailTests + SchedulingServiceTests tests de concurrence; PatientsEndpointTests +3 (400 firstName vide, 404 contact d'un autre patient, 404 delete contact inexistant). Decisions liees: D-012 (cible commerciale cabinets de groupe, concurrence frontale CGM/Corilus -- tranchee par l'utilisateur), D-013 (convention d'erreurs). Verification: revue statique seule -- dotnet build/test a executer cote poste local (SDK indisponible dans l'environnement d'execution).
- Auteur: Agent

- Date: 2026-07-05
- Type: Added
- Zone: Backend/Frontend (F-B2 multi-praticiens + F-A4 prescriptions, valides par l'utilisateur; priorisation D-012)
- Description: (1) F-B2 / P1-006 -- entite Practitioner (registre des kines du cabinet, numero INAMI optionnel) dans le module Scheduling; ISchedulingStore.Add/Get/GetAllPractitioners; SchedulingService.CreatePractitioner/ListPractitioners; CreateSlot valide desormais que PractitionerId reference un praticien existant du tenant (KeyNotFoundException -> 404); endpoints GET/POST /api/scheduling/practitioners (audit practitioner_created); AgendaPage: filtre "Tous les praticiens", noms de praticiens affiches sur les creneaux (au lieu d'ids), selection du praticien par dropdown a la creation de creneau, modal "+ Praticien"; seed: 2 praticiens demo (Julie Peeters, Thomas Dujardin), slots repartis. (2) F-A4 / P1-003 -- entite Prescription (module Clinical: prescripteur, n. INAMI optionnel, date, diagnostic, nb seances, ValidUntilUtc = date + 2 mois regle INAMI); IPrescriptionStore + InMemoryPrescriptionStore; ClinicalService.CreatePrescription/ListPrescriptions (PrescriptionUsage: seances utilisees/restantes, expiration); SeanceClinique.PrescriptionId optionnel: l'imputation verifie appartenance patient (404), validite (409) et quota (409) -- NOTE MVP: verification puis ajout non atomiques, contrainte DB a l'arrivee de PostgreSQL; endpoints GET/POST /api/clinical/patients/{id}/prescriptions (audit prescription_created); PatientsPage: section Prescriptions avec badges Valide/Expiree/Quota atteint et compteurs, modal "Nouvelle prescription", dropdown d'imputation dans la modal seance; seed: 1 prescription par patient actif, seances demo imputees. Tests: SchedulingServiceTests adaptes (praticien requis) + 2 nouveaux; ClinicalServiceTests ctor 2 stores + 6 tests prescriptions; SchedulingEndpointTests adaptes; ClinicalEndpointTests +3 (roundtrip usage, 409 quota, isolation tenant). Verification: tsc --noEmit + vite build OK; dotnet build/test a executer cote poste local (SDK indisponible). Incident environnement: le montage VM a expose des copies tronquees des fichiers frontend pendant l'edition (AgendaPage/PatientsPage/schedulingApi/clinicalApi) -- fichiers reecrits en entier et verifies (tsc/vite OK), contenu final identique cote workspace.
- Auteur: Agent
