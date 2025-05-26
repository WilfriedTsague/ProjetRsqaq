using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tp2WPF.Models
{
   public class Station
   {
        [Key]
        public string ID_STATION { get; set; }
        public string NOM_STATION { get; set; }
        public string RA { get; set; }
        public string ADRESSE { get; set; }
        public string MUNICIPALITE { get; set; }
        public string TYPE_MILIEU { get; set; }
        public string DATE_OUVERTURE { get; set; }
        public string DATE_FERMETURE { get; set; }
        public string LATITUDE { get; set; }
        public string LONGITUDE { get; set; }
   }

}
