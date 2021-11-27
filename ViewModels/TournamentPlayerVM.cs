using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.ViewModels
{
    public class TournamentPlayerVM
    {
        [Key]
        public int TournamentPlayerId { get; set; }
        [ForeignKey("TournamentTeamId")]
        public TournamentTeamVM TournamentTeam { get; set; }
        public int TournamentTeamId { get; set; }
        [ForeignKey("TeamPlayerId")]
        public TeamPlayerVM TeamPlayer { get; set; }
        public int TeamPlayerId { get; set; }
        public bool PlayedThisTournament { get; set; }
    }
}
