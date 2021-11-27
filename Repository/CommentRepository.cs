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
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _db;

        public CommentRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(CommentModel entity)
        {
            _db.Comments.Add(entity);
            return Save();
        }

        public bool Delete(CommentModel entity)
        {
            _db.Comments.Remove(entity);
            return Save();
        }

        public ICollection<CommentModel> FindAll()
        {
            var Comments = _db.Comments
            .Include(a => a.Author)
            .Include(b => b.Blog)
            .ToList();
            return Comments;
        }

        public CommentModel FindbyId(int id)
        {
            var CommentsById = _db.Comments
            .Include(a => a.Author)
            .Include(b => b.Blog)
            .FirstOrDefault(q => q.Id == id);
            return CommentsById;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public bool Update(CommentModel entity)
        {
            _db.Comments.Update(entity);
            return Save();
        }
    }
}
