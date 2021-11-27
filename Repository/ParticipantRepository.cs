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
    public class ParticipantRepository : IParticipantRepository
    {
        private readonly ApplicationDbContext _db;

        public ParticipantRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(ParticipantModel entity)
        {
            _db.Participants.Add(entity);
            return Save();
        }

        public bool Delete(ParticipantModel entity)
        {
            _db.Participants.Remove(entity);
            return Save();
        }

        public ICollection<ParticipantModel> FindAll()
        {
            var ParticipantList = _db.Participants
                .Include(q => q.RequestingStudent)
                .Include(c => c.TournamentApplied)
                .ToList();
            return ParticipantList;
        }

        public ParticipantModel FindbyId(int id)
        {
            var PartcipantById = _db.Participants
                .Include(q => q.RequestingStudent)
                .Include(c => c.TournamentApplied)
               .FirstOrDefault(a => a.ParticipantId == id);
            return PartcipantById;
        }


        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public bool Update(ParticipantModel entity)
        {
            _db.Participants.Update(entity);
            return Save();
        }
    }
}
