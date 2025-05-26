using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tp2WPF.Models
{
    public class Sequentielle
    {
        [Key]
        public string Station { get; set; }
        public string Contaminant { get; set; }
        public string Date { get; set; }
        public string Resultat { get; set; }
        public string LD { get; set; }
        public string Statut { get; set; }
    }

}
