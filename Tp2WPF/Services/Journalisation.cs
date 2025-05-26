using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Configuration;

[SupportedOSPlatform("windows")]
public class Journalisation
{
    private readonly ILogger<Journalisation>? _logger;
    private readonly EventLog _eventLog;
    private string? _cheminFichier = ConfigurationManager.AppSettings.Get("cheminFichier");
  

    public Journalisation(ILogger<Journalisation> logger, EventLog eventLog)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLog = eventLog ?? throw new ArgumentNullException(nameof(eventLog));
        _cheminFichier = "../../../application_log.txt";
        // Créer le dossier de journalisation si nécessaire
        string directory = Path.GetDirectoryName(_cheminFichier);

        if (string.IsNullOrEmpty(directory))
        {
            throw new ArgumentNullException(nameof(directory), "Le chemin de journalisation ne peut pas être nul ou vide.");
        }
        else
        {
            Directory.CreateDirectory(directory);
        }

        _eventLog = new EventLog
        {
            Source = "Application_log",
            Log = "TP1_POO4"
        };
    }

    public void LogErreur(string message, Exception ex = null)
    {
        // Afficher l'erreur à l'écran
        _logger.LogError(ex, message);
        _eventLog.WriteEntry(message, EventLogEntryType.Error);

        // Enregistrer l'erreur dans le fichier
        using (var writer = new StreamWriter(_cheminFichier, true))
        {
            writer.WriteLine("------------------------ Date -----------------------------------");
            writer.WriteLine($"{DateTime.Now}: {message}");
            if (ex != null)
            {
                writer.WriteLine("------------------------ Exception --------------------------");
                writer.WriteLine(" ");
                writer.WriteLine($"Exception: {ex.Message}");
                writer.WriteLine("------------------------Stack Trace--------------------------");
                writer.WriteLine(" ");
                writer.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            writer.WriteLine("----------------------------------------------------------------");
        }
    }
}
