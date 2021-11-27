using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GCUSMS.Contracts;
using GCUSMS.Data;
using GCUSMS.Models;

namespace GCUSMS.Repository
{
    public class TournamentRepository : ITournamentRepository
    {
        private readonly ApplicationDbContext _db;

        public TournamentRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(TournamentModel entity)
        {
            _db.Tournaments.Add(entity);
            return Save();
        }

        public bool Delete(TournamentModel entity)
        {
            _db.Tournaments.Remove(entity);
            return Save();
        }

        public ICollection<TournamentModel> FindAll()
        {
            var TournamentList = _db.Tournaments
               .ToList();
            return TournamentList;
        }

        public TournamentModel FindbyId(int id)
        {
            var TournamentbyId = _db.Tournaments
             .FirstOrDefault(a => a.TournamentId == id);
            return TournamentbyId;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public bool Update(TournamentModel entity)
        {
            _db.Tournaments.Update(entity);
            return Save();
        }
    }
}
