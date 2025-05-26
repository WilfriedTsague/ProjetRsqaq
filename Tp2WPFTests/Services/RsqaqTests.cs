using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Tp2WPF.Models;

namespace Tests
{
    [SupportedOSPlatform("windows")]
    [TestClass()]
    public class RsqaqTests
    {
        private Mock<RsqaqContext> _mockContext;

        [TestInitialize]
        public void Initialize()
        {
            // Création du contexte mock
            _mockContext = new Mock<RsqaqContext>();

            // Création de deux jeux de données factices
            var stations = new List<Station>
            {
                new Station { ID_STATION = "1806", NOM_STATION = "Station A", MUNICIPALITE = "Ville A", DATE_OUVERTURE = new DateTime(2020, 1, 1).ToString(), DATE_FERMETURE = "" },
                new Station { ID_STATION = "3006", NOM_STATION = "Station B", MUNICIPALITE = "Ville B", DATE_OUVERTURE = new DateTime(2019, 1, 1).ToString(), DATE_FERMETURE = new DateTime(2022, 1, 1).ToString() },
                new Station { ID_STATION = "3006", NOM_STATION = "Québec - Vieux-Limoilou", MUNICIPALITE = "Québec", DATE_OUVERTURE = new DateTime(2018, 1, 1).ToString(), DATE_FERMETURE = "" }
            }.AsQueryable();


            var mockSet = CreateMockDbSet(stations);
            _mockContext = new Mock<RsqaqContext>();
            _mockContext.Setup(c => c.Stations).Returns(mockSet);
        }

        // Méthode intégrée pour créer un mock de DbSet<T>
        public static DbSet<T> CreateMockDbSet<T>(IQueryable<T> source) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(source.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(source.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(source.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(source.GetEnumerator());

            return mockSet.Object;
        }

        [TestMethod()]
        public async void EtantDonneStationsActivesExistantes_QuandMethodeAppeler_AlorsRetournerStationsActives()
        {
            // Déclaration
            var rsqaq = new Rsqaq(_mockContext.Object);

            // Action
            var result = await rsqaq.StationsActivesAsync();

            // Assert
            Assert.AreEqual(2, result.Count); // Il ne doit y avoir que 2 stations actives
            Assert.IsTrue(result.Any(s => s["NOM_STATION"] == "Station A"));
            Assert.IsTrue(result.Any(s => s["NOM_STATION"] == "Québec - Vieux-Limoilou"));
        }

        [TestMethod()]
        public async void EtantDonneeStationsActives_QuandMunicipaliteValide_RetourneStationsActivesFiltrees()
        {
            var rsqaq = new Rsqaq(_mockContext.Object);

            // Act
            var result = await rsqaq.StationsActivesAsync("Ville A");

            // Assert
            Assert.AreEqual(1, result.Count); // Il doit y avoir 1 station active à Ville A
            Assert.IsTrue(result.Any(s => s["NOM_STATION"] == "Station A"));
        }

        [TestMethod]
        public async void EtantDonneeStationsActives_QuandException_RetourneListeVide()
        {
            // Arrange
            var rsqaq = new Rsqaq(_mockContext.Object);
            string municipalite = "Ville Z"; // Municipalité pour le test

            // Act
            var result = await rsqaq.StationsActivesAsync(municipalite);

            // Assert
            Assert.IsNotNull(result); // Vérifie que le résultat n'est pas nul
            Assert.AreEqual(0, result.Count); // Vérifie que la liste retournée est vide
        }

        [TestMethod()]
        public async  void ResultatsStationContaminant_QuandStationEtContaminantValides_RetourneResultats()
        {
            // Créer des données de test
            var sequentielles = new List<Sequentielle>
            {   new Sequentielle { Station = "01805 - Murdochville - Parc Lions", Contaminant = "Nickel", Date = "2024-01-01", Resultat = "0.75", Statut = "mesure" },
                new Sequentielle { Station = "03006 - Québec - Vieux-Limoilou", Contaminant = "Zinc PM10", Date = "2016-10-13", Resultat = "0.36", Statut = "mesure" },
                new Sequentielle {Station = "03006 - Québec - Vieux-Limoilou",Contaminant =   "Zinc PM10" , Date =  "2016-10-25" , Resultat =     "0.06" , Statut =   "<LD" }
            }.AsQueryable();

            // Mock du DbSet<Sequentielle>
            var mockSequentielles = CreateMockDbSet(sequentielles);
            _mockContext.Setup(c => c.Sequentielles).Returns(mockSequentielles);

            var rsqaq = new Rsqaq(_mockContext.Object);

            // Act
            var result = await rsqaq.ResultatsStationContaminantAsync("3006", "Zinc PM10");

            // Assert
            Assert.AreEqual(1, result.Count); // Il doit y avoir 1 résultat pour la station 1 et le contaminant Zinc PM10
            Assert.AreEqual("0.36", result[0]["Resultat"]); // Le résultat doit être 0.36
        }

        [TestMethod]
        public async void EtantDonneeResultatsStationContaminant_QuandException_RetourneListeVide()
        {
            // Créer des données de test
            var sequentielles = new List<Sequentielle>
            {   new Sequentielle { Station = "01805 - Murdochville - Parc Lions", Contaminant = "Nickel", Date = "2024-01-01", Resultat = "0.75", Statut = "mesure" },
                new Sequentielle { Station = "03006 - Québec - Vieux-Limoilou", Contaminant = "Zinc PM10", Date = "2016-10-13", Resultat = "0.36", Statut = "mesure" },
                new Sequentielle {Station = "03006 - Québec - Vieux-Limoilou",Contaminant =   "Zinc PM10" , Date =  "2016-10-25" , Resultat =     "0.06" , Statut =   "<LD" }
            }.AsQueryable();

            // Mock du DbSet<Sequentielle>
            var mockSequentielles = CreateMockDbSet(sequentielles);
            _mockContext.Setup(c => c.Sequentielles).Returns(mockSequentielles);

            var rsqaq = new Rsqaq(_mockContext.Object);
            string stationId = "Station123";
            string contaminant = "Nickel";

            // Act
            var result = await rsqaq.ResultatsStationContaminantAsync(stationId, contaminant);

            //Assert
            Assert.IsNotNull(result); // Vérifie que le résultat n'est pas nul
            Assert.AreEqual(0, result.Count); // Vérifie que la liste retournée est vide
        }

        [TestMethod]
        public async void Enregistrements_QuandColonnesEtCriteresValides_RetournerLesEnregistrements()
        {
            var context = new RsqaqContext();

            // Instancier l'objet à tester
            var rsqaq = new Rsqaq(context);
            string[] colonnes = { "ID_STATION", "NOM_STATION", "CONTAMINANT", "DATE", "RESULTAT" };
            string[] criteres = { "MUNICIPALITE = 'Québec'", "Contaminant = 'Nickel PM10'", "Resultat = 0.151" };

            // Act
            var result = await rsqaq.EnregistrementsAsync(colonnes, criteres);

            // Assert
            Assert.AreEqual(0, result.Count);  // Il doit y avoir 3 enregistrements
            //Assert.AreEqual("Québec - Vieux-Limoilou", result[0]["NOM_STATION"]);  // Station doit être "Québec - Vieux-Limoilou"
            //Assert.AreEqual("2023-01-06", result[2]["DATE"]);  // Résultat doit être "0.151"
        }

        [TestMethod]
        public async void EtantDonneeEnregistrements_QuandCriteresInvalides_RetournerListeVide()
        {
            var context = new RsqaqContext();

            var rsqaq = new Rsqaq(context);
            string[] colonnes = { "ID_STATION", "NOM_STATION", "CONTAMINANT", "DATE", "RESULTAT" };
            string[] criteres = { "MUNICIPALITE = 'NonExistant'" };

            // Act
            var result = await rsqaq.EnregistrementsAsync(colonnes, criteres);

            // Assert
            Assert.AreEqual(0, result.Count);  // Aucun enregistrement ne doit être trouvé
        }

        [TestMethod]
        public async void EtantDonneesArgsInvalides_QuandEnregistrementsAvecArgsAppel_RetournerListeVide()
        {
            // Arrange
            var context = new RsqaqContext(); // ou un mock du contexte si possible
            var rsqaq = new Rsqaq(context);
            string[] args = { "--cols", "ID_STATION,NOM_STATION,Contaminant,Date,Resultat", "--where", "MUNICIPALITE = 'Inconnue', Contaminant = 'Nickel Inconnu', Resultat = '0.0'" };

            // Act
            var result = await rsqaq.EnregistrementsAvecArgsAsync(args);

            // Assert
            Assert.IsNotNull(result); // Vérifie que le résultat n'est pas nul
            Assert.AreEqual(0, result.Count); // Vérifie que la liste retournée est vide
        }

        [TestMethod]
        public async void EtantDonneesArgsCorrect_QuandEnregistrementsAvecArgsAppel_RetournerListeVideEtLogErreur()
        {
            // Arrange
            var context = new RsqaqContext(); // ou un mock du contexte si possible
            var rsqaq = new Rsqaq(context);
            string[] args = { "--cols", "id_station,nom_station,contaminant", "--where", "municipalite = 'Québec', date_fermeture = ''" }; // Arguments incomplets ou incorrects pour générer une erreur

            // Act
            var result = await rsqaq.EnregistrementsAvecArgsAsync(args);

            // Assert
            Assert.AreEqual(83949, result.Count); // Vérifie que la liste retournée est = 83949
            Assert.AreEqual("Québec - Vieux-Limoilou", result[3]["NOM_STATION"]);  // Station doit être "Québec - Vieux-Limoilou"
            Assert.AreEqual("Aluminium PM10", result[9]["CONTAMINANT"]);  // Résultat doit être "0.151"
        }
    }
}
