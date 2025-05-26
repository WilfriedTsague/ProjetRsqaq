using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using GalaSoft.MvvmLight.Command;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;


public class MainViewModel
{
    // Propriété pour les enregistrements
    public ObservableCollection<Dictionary<string, string>> Records { get; set; }

    // Commande pour le bouton
    public ICommand FetchDataCommand { get; }
    private readonly Rsqaq _rsqaq;
    public string[] _args { get; set; }

    public MainViewModel()
    {
        Records = new ObservableCollection<Dictionary<string, string>>();

        // Configureration des services  
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        _rsqaq = serviceProvider.GetService<Rsqaq>();

        FetchDataCommand = new RelayCommand(async () => await FetchData());
    }

    public void Initialize(string[] args)
    {
        _args = args;
    }

    public async Task FetchData()
    {
        string[] colonnes = { "ID_STATION", "NOM_STATION", "Contaminant", "Date", "Resultat" };
        string[] criteres = { "MUNICIPALITE = 'Québec'", "Contaminant = 'Nickel PM10'", "Resultat > 0.07" };

        IEnumerable<Dictionary<string, string>> enregistrements;

        if (_args != null && _args.Length > 0)
        {
            enregistrements = await _rsqaq.EnregistrementsAvecArgsAsync(_args);
        }
        else
        {
            enregistrements = await _rsqaq.EnregistrementsAsync(colonnes, criteres);
        }

        Records.Clear();
        foreach (var record in enregistrements)
        {
            Records.Add(record);
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<RsqaqContext>(); 
        services.AddTransient<Rsqaq>();        
    }

    //sert à signaler des modifications sur les propriétés d'un objet
    public event PropertyChangedEventHandler PropertyChanged;
}

