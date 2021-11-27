using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Models
{
    public class AllPlayersModel
    {
        [Key]
        public int AllPlayerId { get; set; }
        [ForeignKey("TournamentTeamId")]
        public TournamentTeamModel TournamentTeam { get; set; }
        public int TournamentTeamId { get; set; }
        [ForeignKey("TeamPlayerId")]
        public TeamPlayerModel TeamPlayer { get; set; }
        public int TeamPlayerId { get; set; }
        [ForeignKey("ParticipantId")]
        public ParticipantModel Participants { get; set; }
        public int ParticipantId { get; set; }
    }
}
