# Vérification de l'affichage des cotations

Depuis la racine du dépôt :

```powershell
dotnet run --project tests/FnafTCG.Quotes.Tests
```

Ces vérifications exécutent le vrai service HTTP et le modèle de vue avec des réponses API simulées : prix distinct de la version 249, carte sélectionnée, changement de prix après actualisation, notifications des bindings, devise, tri, historique vide et erreur réseau.

Pour vérifier aussi deux cartes sur l'API publique du groupe 3 (lecture seule) :

```powershell
dotnet run --project tests/FnafTCG.Quotes.Tests -- --live
```

Dans l'application : ouvrir une carte, puis « Afficher les quotes ». Le prix et le graphique se chargent à l'ouverture ; « Actualiser le cours » et une actualisation toutes les 10 secondes relisent l'API. Quitter la page arrête le minuteur. Modifier le prix de vente d'une offre ne modifie pas la cotation du marché.

L'API conserve les 120 dernières cotations : les filtres ne créent pas d'historique plus ancien.
