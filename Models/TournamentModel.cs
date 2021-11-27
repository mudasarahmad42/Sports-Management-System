using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Models
{
    public class TournamentModel
    {
        [Key]
        public int TournamentId { get; set; }
        [Required]
        public string TournamentName { get; set; }
        [Required]
        public string TournamentVenue { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public DateTime DueDate { get; set; }
        public int TeamsAllowed { get; set; }
        public bool isActive { get; set; } = false;
        public bool isIntraDepartmental { get; set; } = false;
        public string DepartmentName { get; set; }
        public string Message { get; set; }
        public string Winner { get; set; }
        public string RunnerUp { get; set; }
        public string Status { get; set; }
        public int PointsForWinning { get; set; } = 0;
        public int PointsForLosing { get; set; } = 0;
        public int PointsForDraw { get; set; } = 0;
        public string SportsCategory { get; set; }
        public string GenderAllowed { get; set; }
        public string TournamentType { get; set; }
    }
}
