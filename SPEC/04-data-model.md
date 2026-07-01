# 04 - Modele de donnees

Entites coeur
- Tenant
- Utilisateur
- Role
- Patient
- DossierClinique
- Seance
- RendezVous
- Facture
- LigneFacture
- DossierRemboursement
- JournalAudit

Regles de modelisation
- Cle technique stable.
- Horodatage creation/mise a jour.
- Auteur des operations sensibles.
- Statut explicite pour chaque workflow.

Contraintes
- Isolation des donnees par tenant.
- Integrite referentielle stricte.
- Historisation des transitions critiques.
