using GCUSMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Contracts
{
    public interface ITournamentTeamRepository : IRepositoryBase<TournamentTeamModel>
    {
        public TournamentTeamModel FindTeamByTournamentIdandTeamId(int TeamId, int TournamentId);
    }
}
