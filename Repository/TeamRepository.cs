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
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _db;

        public TeamRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(TeamModel entity)
        {
            _db.Teams.Add(entity);
            return Save();
        }

        public bool Delete(TeamModel entity)
        {
            _db.Teams.Remove(entity);
            return Save();
        }

        public ICollection<TeamModel> FindAll()
        {
            var Teams = _db.Teams
               .ToList();
            return Teams;
        }

        public TeamModel FindbyId(int id)
        {
            var TeamById = _db.Teams
                .FirstOrDefault(q => q.TeamId == id);
            return TeamById;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public bool Update(TeamModel entity)
        {
            _db.Teams.Update(entity);
            return Save();
        }
    }
}
