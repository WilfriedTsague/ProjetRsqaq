using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tp2WPF.interfaces
{
    public interface IRsqaq
    {
        // Liste de tous les contaminants distincts
        public List<string> Contaminants { get; }

        // Liste de toutes les stations actives distinctes avec leurs données sous forme «colonne => donnée»
        Task<List<Dictionary<string, string>>> StationsActivesAsync();

        // Liste de toutes les stations actives d'une municipalité avec leurs données sous forme «colonne => donnée»
        Task<List<Dictionary<string, string>>> StationsActivesAsync(string municipalite);

        /* Liste de tous les enregistrements ayant un résultat >= LD pour un ID de station et un contaminant donnés sous forme «colonne => donnée» avec les colonnes suivantes:
         *      Date
         *      Resultat
         */
        Task<List<Dictionary<string, string>>> ResultatsStationContaminantAsync(string stationId, string contaminant);

        /* Liste de tous les enregistrements distincts avec les colonnes et les critères donnés
         *      Exemple d'appel: EnregistrementsAsync(new[] { "id_station", "nom_station", "contaminant" }, new[] { "municipalite = 'Québec'", "date_fermeture = ''" })
         */
        Task<List<Dictionary<string, string>>> EnregistrementsAsync(string[] colonnes, string[] criteres);
    }
}
