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
    public class TeamPlayerRepository : ITeamPlayerRepository
    {
        private readonly ApplicationDbContext _db;

        public TeamPlayerRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(TeamPlayerModel entity)
        {
            _db.TeamPlayer.Add(entity);
            return Save();
        }

        public bool Delete(TeamPlayerModel entity)
        {
            _db.TeamPlayer.Remove(entity);
            return Save();
        }

        public ICollection<TeamPlayerModel> FindAll()
        {
            var TeamPlayerList = _db.TeamPlayer
                .Include(q => q.Player)
                .Include(c => c.Team)
               .ToList();
            return TeamPlayerList;
        }

        public TeamPlayerModel FindbyId(int id)
        {
            var TeamPlayerById = _db.TeamPlayer
                 .Include(q => q.Player)
                 .Include(c => c.Team)
                .FirstOrDefault(a => a.TeamPlayerId == id);
            return TeamPlayerById;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public bool Update(TeamPlayerModel entity)
        {
            _db.TeamPlayer.Update(entity);
            return Save();
        }
    }
}
