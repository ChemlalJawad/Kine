# SPEC - Source de verite du projet

But
- Centraliser toutes les informations du projet ici.
- Eviter les decisions implicites hors SPEC.
- Permettre a l agent de consulter SPEC en premier.

Regles d usage
1. Toute nouvelle decision doit etre ajoutee dans 06-decisions.md.
2. Tout changement livre doit etre journalise dans 07-change-log.md.
3. Toute evolution d architecture doit mettre a jour 03-architecture.md.
4. Toute exigence fonctionnelle/non-fonctionnelle doit etre tenue dans 02-requirements.md.
5. Les points ouverts vont dans 11-open-questions.md.

Ordre de lecture pour l agent
1. README.md
2. 06-decisions.md
3. 02-requirements.md
4. 03-architecture.md
5. 07-change-log.md
6. 11-open-questions.md

Etat actuel
- Statut: Initialisation SPEC
- Derniere mise a jour: 2026-07-01

Workflow recommande: request -> new chat
1. Ouvrir un nouveau chat pour chaque demande.
2. Lancer un prompt dans .github/prompts selon besoin (orchestrate/dev/analysis/architecture).
3. L agent charge SPEC baseline en premier.
4. Execution compacte uniquement: Actions executed, Result, Blocker.
5. Toute decision/changement doit etre synchronise dans SPEC.
