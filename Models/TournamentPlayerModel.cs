using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Models
{
    public class TournamentPlayerModel
    {
        [Key]
        public int TournamentPlayerId { get; set; }
        [ForeignKey("TournamentTeamId")]
        public TournamentTeamModel TournamentTeam { get; set; }
        public int TournamentTeamId { get; set; }
        [ForeignKey("TeamPlayerId")]
        public TeamPlayerModel TeamPlayer { get; set; }
        public int TeamPlayerId { get; set; }
        public bool PlayedThisTournament { get; set; }
    }
}
