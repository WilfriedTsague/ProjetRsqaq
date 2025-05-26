using Tp2WPF.Models;
using Tp2WPF.interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

[SupportedOSPlatform("windows")]
public class Rsqaq : IRsqaq
{
    private readonly RsqaqContext _context;
    private readonly Journalisation _journalisation;
    private readonly EventLog _eventLog;

    public Rsqaq(RsqaqContext context)
    {
        _context = context;

        _eventLog = new EventLog();
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.AddConsole().SetMinimumLevel(LogLevel.Information);
            })
            .BuildServiceProvider();

        var logger = serviceProvider.GetService<ILogger<Journalisation>>();

        _journalisation = new Journalisation(logger, _eventLog);
    }

    public List<string> Contaminants
    {
        get
        {
            try
            {
                return _context.Sequentielles
                    .Select(seq => seq.Contaminant)
                    .Distinct()
                    .ToList();
            }
            catch (Exception ex)
            {
                _journalisation.LogErreur("Erreur lors de la récupération des contaminants", ex);
                return new List<string>();
            }
        }
    }

    public async Task<List<Dictionary<string, string>>> StationsActivesAsync()
    {
        try
        {
            return await _context.Stations
                .Where(station => string.IsNullOrEmpty(station.DATE_FERMETURE))
                .Select(sta => new Dictionary<string, string>
                {
                    { "ID_STATION", sta.ID_STATION },
                    { "NOM_STATION", sta.NOM_STATION },
                    { "Municipalite", sta.MUNICIPALITE },
                    { "DATE_OUVERTURE", sta.DATE_OUVERTURE },
                    { "DATE_FERMETURE", sta.DATE_FERMETURE }
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _journalisation.LogErreur("Erreur lors de la récupération des stations actives", ex);
            return new List<Dictionary<string, string>>();
        }
    }

    public async Task<List<Dictionary<string, string>>> StationsActivesAsync(string municipalite)
    {
        try
        {
            var stations = await StationsActivesAsync();

            return stations
                .Where(station => station["Municipalite"] == municipalite)
                .ToList();
        }
        catch (Exception ex)
        {
            _journalisation.LogErreur($"Erreur lors de la récupération des stations actives pour {municipalite}", ex);
            return new List<Dictionary<string, string>>();
        }
    }
    public async Task<List<Dictionary<string, string>>> ResultatsStationContaminantAsync(string stationId, string contaminant)
    {
        string statut = "mesure";
        try
        {
            return await _context.Sequentielles
                .Where(seq => seq.Station.Contains(stationId) && seq.Contaminant == contaminant && seq.Statut == statut)
                .Select(seq => new Dictionary<string, string>
                {
                    { "Station", seq.Station },
                    { "Contaminant", seq.Contaminant },
                    { "Date", seq.Date },
                    { "Resultat", seq.Resultat.ToString() }
                })
                .ToListAsync();
        }

        catch (Exception ex)
        {
            _journalisation.LogErreur("Erreur lors de la récupération des résultats", ex);
            return new List<Dictionary<string, string>>();
        }
    }

    public async Task<List<Dictionary<string, string>>> EnregistrementsAsync(string[] colonnes, string[] criteres)
    {
        var resultats = new List<Dictionary<string, string>>();
        var sql = new StringBuilder();
        sql.Append("SELECT ");

        foreach (string col in colonnes)
        {
            if (col.Equals("Station", StringComparison.OrdinalIgnoreCase) ||
                col.Equals("Contaminant", StringComparison.OrdinalIgnoreCase) ||
                col.Equals("Date", StringComparison.OrdinalIgnoreCase) ||
                col.Equals("Resultat", StringComparison.OrdinalIgnoreCase) ||
                col.Equals("LD", StringComparison.OrdinalIgnoreCase) ||
                col.Equals("Statut", StringComparison.OrdinalIgnoreCase))
            {
                sql.Append($"sequentielles.{col}, ");
            }
            else
            {
                sql.Append($"stations.{col}, ");
            }
        }

        sql.Length -= 2;
        sql.AppendLine();
        sql.AppendLine("FROM sequentielles");
        sql.AppendLine("JOIN stations ON sequentielles.Station LIKE '%' || stations.ID_STATION || '%'");

        if (criteres.Any())
        {
            sql.Append("WHERE ");
            var criteriaParts = new List<string>();

            foreach (var critere in criteres)
            {
                var parts = critere.Split(new[] { '=', '>', '<' }, 2);
                if (parts.Length == 2)
                {
                    var colonne = parts[0].Trim();
                    var valeur = parts[1].Trim().Trim('\'');
                    var operateur = critere.Contains(">") ? ">" : critere.Contains("<") ? "<" : "=";
                    criteriaParts.Add($"{colonne} {operateur} @{colonne.Replace('.', '_')}");
                }
            }

            sql.AppendLine(string.Join(" AND ", criteriaParts));
        }

        using var connection = new SqliteConnection(_context.Database.GetDbConnection().ConnectionString);
        await connection.OpenAsync();

        try
        {
            using var command = new SqliteCommand(sql.ToString(), connection);

            foreach (var critere in criteres)
            {
                var parts = critere.Split(new[] { '=', '>', '<' }, 2);
                if (parts.Length == 2)
                {
                    var colonne = parts[0].Trim();
                    var valeur = parts[1].Trim().Trim('\'');
                    if (double.TryParse(valeur, out var seuil))
                    {
                        command.Parameters.AddWithValue($"@{colonne.Replace('.', '_')}", seuil);
                    }
                    else
                    {
                        command.Parameters.AddWithValue($"@{colonne.Replace('.', '_')}", valeur);
                    }
                }
            }

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, string>();
                foreach (var col in colonnes)
                {
                    var value = reader[col]?.ToString() ?? string.Empty;
                    row[col] = value;
                }
                resultats.Add(row);
            }
        }
        catch (Exception ex)
        {
            _journalisation.LogErreur("Erreur lors de l'exécution de la commande SQL asynchrone", ex);
        }

        return resultats;
    }

    public async Task<List<Dictionary<string, string>>>? EnregistrementsAvecArgsAsync(string[] args)
    {
        string[] colonnes = null;
        string[] criteres = null;
        List<Dictionary<string, string>>? enrg = null;

        try
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--cols" && args[i + 1] != "--where" && i + 1 < args.Length)
                {
                    colonnes = args[i + 1].Split(',');
                }
                else if (args[i] == "--where" && i + 1 < args.Length)
                {
                    criteres = args[i + 1].Split(',');
                }
            }

            // Appeler la méthode Enregistrements avec les colonnes et critères extraits
            enrg = await EnregistrementsAsync(colonnes, criteres);
        }
        catch (Exception ex)
        {
            _journalisation.LogErreur("Erreur Un argument requis est manquant ou incorrect", ex);
        }
        return enrg;
    }
}
