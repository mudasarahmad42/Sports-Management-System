using GCUSMS.Contracts;
using GCUSMS.Data;
using GCUSMS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Repository
{
    public class TournamentTeamRepository : ITournamentTeamRepository
    {
        private readonly ApplicationDbContext _db;

        public TournamentTeamRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(TournamentTeamModel entity)
        {
            _db.TournamentTeam.Add(entity);
            return Save();
        }

        public bool Delete(TournamentTeamModel entity)
        {
            _db.TournamentTeam.Remove(entity);
            return Save();
        }

        public ICollection<TournamentTeamModel> FindAll()
        {
            var TournamentTeam = _db.TournamentTeam
               .Include(q => q.Team)
               .Include(q => q.Tournament)
               .ToList();
            return TournamentTeam;
        }

        public TournamentTeamModel FindbyId(int id)
        {
            var TournamentTeam = _db.TournamentTeam
               .Include(q => q.Team)
               .Include(q => q.Tournament)
               .FirstOrDefault(q => q.TournamentTeamId == id);
            return TournamentTeam;
        }

        public TournamentTeamModel FindTeamByTournamentIdandTeamId(int TeamId ,int TournamentId)
        {
            var TournamentTeam = _db.TournamentTeam
               .Include(q => q.Team)
               .Include(q => q.Tournament)
               .FirstOrDefault(q => q.TeamId == TeamId && q.TournamentId == TournamentId);
            return TournamentTeam;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public bool Update(TournamentTeamModel entity)
        {
            _db.TournamentTeam.Update(entity);
            return Save();
        }
    }
}
