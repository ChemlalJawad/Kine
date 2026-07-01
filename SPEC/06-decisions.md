# 06 - Journal des decisions (ADR simplifie)

Format d entree
- ID:
- Date:
- Sujet:
- Decision:
- Contexte:
- Alternatives:
- Impact:
- Statut:

Decisions existantes
- ID: D-001
- Date: 2026-07-01
- Sujet: Mode agent
- Decision: Mode strict execution + token minimal
- Contexte: Besoin de reponses courtes et executables
- Alternatives: Mode souple
- Impact: Moins de verbosite, besoin de validation explicite pour decisions
- Statut: Accepte

- ID: D-002
- Date: 2026-07-01
- Sujet: Source de verite
- Decision: Dossier SPEC comme reference principale
- Contexte: Centraliser infos, analyses, architecture, changements
- Alternatives: Connaissance repartie dans plusieurs endroits
- Impact: Meilleure coherence, maintenance simplifiee
- Statut: Accepte

- ID: D-003
- Date: 2026-07-01
- Sujet: Strategie agents projet
- Decision: Creer 4 agents specialises (orchestrator, dev, analysis, architecture) sous .github/agents
- Contexte: Structurer le travail futur pour construire l app avec discipline SPEC-first
- Alternatives: Agent unique generaliste
- Impact: Delegation plus claire, meilleure qualite de sortie, meilleure tracabilite
- Statut: Accepte

- ID: D-004
- Date: 2026-07-01
- Sujet: Multi-tenant DB MVP
- Decision: PostgreSQL unique en UE, schema partage avec tenant_id obligatoire + RLS activee
- Contexte: Cible 10-30 cabinets en 6 mois, KISS, isolation logique stricte requise
- Alternatives: 1) Schema par tenant 2) Base par tenant
- Impact: Simplicite operationnelle forte, besoin de discipline stricte sur policies et index
- Statut: Accepte

- ID: D-005
- Date: 2026-07-01
- Sujet: Blueprint cloud UE MVP
- Decision: Region UE unique + environnements separes (dev/staging/prod) + reseau prive + coffre secrets + PITR 30 jours
- Contexte: Besoin de fiabilite, securite et deploiement rapide MVP
- Alternatives: 1) Multi-region immediate 2) Environnements partages
- Impact: Time-to-market optimise, resilience regionale non couverte au MVP
- Statut: Accepte

- ID: D-006
- Date: 2026-07-01
- Sujet: Strategie d evolution architecture
- Decision: Modular monolith d abord, extraction service seulement sur criteres SLO/couplage/cadence
- Contexte: Eviter sur-ingenierie, garder frontieres claires des domaines
- Alternatives: Microservices des le jour 1
- Impact: Complexite reduite au MVP, migration ciblee preparee par contrats/evenements
- Statut: Accepte

- ID: D-007
- Date: 2026-07-01
- Sujet: Perimetre conformite MVP
- Decision: Baseline RGPD + controles day-1 obligatoires; integrations eHealth/eFact/eAttest confirmees hors MVP tant que non validees officiellement
- Contexte: Demarrage implementation sans bloquer la construction de la plateforme
- Alternatives: 1) Bloquer toute implementation 2) Integrer eHealth complet des phase 1
- Impact: Permet execution immediate, maintient les points legaux specifiques en verification controlee
- Statut: Accepte

- ID: D-008
- Date: 2026-07-01
- Sujet: Gouvernance orchestration multi-agents
- Decision: Orchestrator repartit les taches; Dev implemente; en cas de blocage technique/compliance, Analysis + Architecture cadrent la solution pour reprise Dev; suivi centralise dans SPEC/16-project-tracking.md
- Contexte: Besoin utilisateur de pilotage actif, logs et avancement visible
- Alternatives: Coordination ad hoc sans journal central
- Impact: Trajectoire d execution plus lisible, blocages traites plus vite, trace operationnelle unifiee
- Statut: Accepte

- ID: D-009
- Date: 2026-07-01
- Sujet: Nom produit
- Decision: Le produit/app est nomme Q-INE; le nom "Kine" reste un referentiel technique/historique tant que la migration des artefacts n'est pas requise
- Contexte: Demande utilisateur de renommer l'application
- Alternatives: Conserver Kine comme nom public
- Impact: Branding et documentation alignes sur Q-INE; les identifiants techniques peuvent rester inchanges a court terme
- Statut: Accepte
