# Agents projet Kine

Objectif
- Specialiser les agents pour accelerer dev, analyse et architecture.
- Respecter strictement les regles projet et SPEC-first.

Agents
- kine-orchestrator.agent.md: point d entree principal.
- kine-dev.agent.md: implementation et corrections.
- kine-analysis.agent.md: analyse metier, risques, conformite, cadrage.
- kine-architecture.agent.md: architecture, decisions techniques, evolution.

Regles communes
1. Lire SPEC en premier quand le contexte projet est necessaire.
2. Ne pas prendre de decision sans accord utilisateur.
3. Reponse compacte, orientee execution.
4. Mettre a jour SPEC (decisions/changelog/open questions) quand impact.

Flux recommande
1. Utiliser kine-orchestrator pour trier la demande.
2. Deleguer a l agent specialise.
3. Appliquer puis mettre a jour SPEC.
