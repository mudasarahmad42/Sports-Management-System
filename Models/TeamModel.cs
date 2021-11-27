using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Models
{
    public class TeamModel
    {
        [Key]
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int MaximumPlayers { get; set; }
        public string TeamSports { get; set; }
        public string TeamLevel { get; set; }
        public string DepartmentName { get; set; }
        public string LogoImagePath { get; set; }
    }
}
