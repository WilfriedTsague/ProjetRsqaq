using Tp2WPF.Models;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Configuration;

public class RsqaqContext : DbContext
{
    public virtual DbSet<Sequentielle> Sequentielles { get; set; }
    public virtual DbSet<Station> Stations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        // Vérifiez si les options sont déjà configurées
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = ConfigurationManager.AppSettings.Get("DefaultConnection");
            optionsBuilder.UseSqlite(connectionString);
        }
    }
    
}
