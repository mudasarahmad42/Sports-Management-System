using GCUSMS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.ViewModels
{
    public class TeamPlayerVM
    {
        [Key]
        public int TeamPlayerId { get; set; }
        public string PlayerWeight { get; set; }
        public string PlayerHeight { get; set; }
        public int PlayerFitnessPoints { get; set; }
        public string PlayerRole { get; set; } = "Player";
        public bool IsActive { get; set; }
        public string CaptainType { get; set; }
        [ForeignKey("PlayerId")]
        public StudentModel Player { get; set; }
        public string PlayerId { get; set; }
        [ForeignKey("TeamId")]
        public TeamModel Team { get; set; }
        public int TeamId { get; set; }
    }


    public class EditTeamPlayerVM
    {
        [Key]
        public int TeamPlayerId { get; set; }
        public string PlayerWeight { get; set; }
        public string PlayerHeight { get; set; }
        public int PlayerFitnessPoints { get; set; }
        public string PlayerRole { get; set; } = "Player";
        public bool IsActive { get; set; }
        public string CaptainType { get; set; }
        [ForeignKey("PlayerId")]
        public StudentModel Player { get; set; }
        public string PlayerId { get; set; }
        [ForeignKey("TeamId")]
        public TeamModel Team { get; set; }
        public int TeamId { get; set; }
    }
}
