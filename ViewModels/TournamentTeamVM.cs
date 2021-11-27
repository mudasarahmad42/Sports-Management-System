using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.ViewModels
{
    public class TournamentTeamVM
    {
        [Key]
        public int TournamentTeamId { get; set; }
        public int MatchesPlayed { get; set; } = 0;
        public int MatchesWon { get; set; } = 0;
        public int MatchesLost { get; set; } = 0;
        public int MatchesDrawed { get; set; } = 0;
        public int TotalPoints { get; set; } = 0;
        [ForeignKey("TournamentId")]
        public TournamentVM Tournament { get; set; }
        public int TournamentId { get; set; }
        [ForeignKey("TeamId")]
        public TeamVM Team { get; set; }
        public int TeamId { get; set; }
    }
}
