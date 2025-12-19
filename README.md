# Chess Federation Manager

Application multiplateforme (Avalonia UI + .NET 9) destinée au personnel administratif d’une fédération d’échecs.

## Fonctionnalités
- Gestion des joueurs (CRUD complet).
- Création et gestion des compétitions.
- Inscriptions/désinscriptions des joueurs aux compétitions.
- Encodage des parties (couleurs, coups, résultat).
- Calcul automatique des Elo via `GameService` (facteur K = 32) et persistance SQLite.
- **Fonctionnalité supplémentaire : onglet _Leaderboard_** affichant le classement global trié des joueurs.

## Architecture
- `ChessFederationManager.Domain` : entités (`Player`, `Competition`, `Game`, `Move`) et règles métier.
- `ChessFederationManager.Application` : services (`PlayerService`, `CompetitionService`, `GameService`).
- `ChessFederationManager.Infrastructure` : persistance (EF Core SQLite + implémentations in-memory).
- `ChessFederationManager.UI` : client Avalonia (onglets Players, Competitions, Games, Leaderboard).

## Persistance
Par défaut l’application crée/charge `data/chess.db` **dans le dossier de travail courant**. Lors d’un `dotnet run` lancé à la racine du dépôt, le fichier se situe dans `Chessfederationmanager/data/chess.db`. Depuis un IDE, le fichier peut être copié/consulté dans `src/ChessFederationManager.UI/bin/<config>/net9.0/data/chess.db`.

## Rapport
Tous les éléments demandés (introduction, description de la fonctionnalité supplémentaire, diagrammes, SOLID, conclusion…) sont détaillés dans [`docs/REPORT.md`](docs/REPORT.md).

## Lancer l’application
```bash
dotnet run --project src/ChessFederationManager.UI/ChessFederationManager.UI.csproj
```

## Tests
```bash
dotnet test
```
