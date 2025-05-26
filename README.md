# Rsqaq

## Description
Rsqaq est une application console C# destinée à la gestion et à l’analyse de données environnementales. Le projet importe des données de surveillance issues de stations (fichiers CSV), les stocke dans une base SQLite via Entity Framework, puis permet l’affichage de résultats conditionnels selon des seuils définis de contaminants.

---

## Fonctionnalités
- Importation de données stations et contaminants à partir de fichiers CSV.
- Persistance via Entity Framework Core et base SQLite.
- Filtrage de résultats par ville, contaminant et seuil.
- Requêtes LINQ avancées pour l’analyse de données.
- Interface `IRsqaq` pour abstraction et testabilité.
- Modèle de données structuré (`Station`, `Contaminant`, etc.)

---

## Technologies utilisées
- **Langage** : C#
- **ORM** : Entity Framework Core
- **Base de données** : SQLite
- **Manipulation de fichiers** : `System.IO`, `CsvHelper` (optionnel)
- **Requêtes** : LINQ
- **Architecture** : Programmation orientée objet + interface d’abstraction

---

## Exemple d’utilisation
Affichage des enregistrements pour la ville de Québec avec le contaminant "Nickel PM10" dépassant 70 ng/m³ :
```csharp
var resultats = rsqaqService.GetDepassements("Québec", "Nickel PM10", 70);
