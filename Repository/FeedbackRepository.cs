using GCUSMS.Contracts;
using GCUSMS.Data;
using GCUSMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Repository
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly ApplicationDbContext _db;

        public FeedbackRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(FeedbackModel entity)
        {
            _db.Feedbacks.Add(entity);
            return Save();
        }

        public bool Delete(FeedbackModel entity)
        {
            _db.Feedbacks.Remove(entity);
            return Save();
        }

        public ICollection<FeedbackModel> FindAll()
        {
            var Feedback = _db.Feedbacks
               .ToList();
            return Feedback;
        }

        public FeedbackModel FindbyId(int id)
        {
            var Feedback = _db.Feedbacks
               .FirstOrDefault(q => q.FeedbackID == id);
            return Feedback;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public bool Update(FeedbackModel entity)
        {
            _db.Feedbacks.Update(entity);
            return Save();
        }
    }
}
