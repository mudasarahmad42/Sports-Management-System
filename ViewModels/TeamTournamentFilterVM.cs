using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.ViewModels
{
    public class TeamTournamentFilterVM
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<TournamentTeamVM> TournamentTeamList { get; set; }
    }
}
