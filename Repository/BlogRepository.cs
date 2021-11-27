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
    public class BlogRepository : IBlogRepository
    {
        private readonly ApplicationDbContext _db;

        public BlogRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(BlogModel entity)
        {
            _db.Blogs.Add(entity);
            return Save();
        }

        public bool Delete(BlogModel entity)
        {
            _db.Blogs.Remove(entity);
            return Save();
        }

        public ICollection<BlogModel> FindAll()
        {
            var Blogs = _db.Blogs
               .Include(q => q.Author)
               .Include(c => c.Comments)
               .ToList();
            return Blogs;
        }

        public BlogModel FindbyId(int id)
        {
            var BlogById = _db.Blogs
               .Include(q => q.Author)
               .Include(p => p.Comments)
                    .ThenInclude(comment => comment.Author)
                .Include(post => post.Comments)
                    .ThenInclude(comment => comment.Comments)
               .FirstOrDefault(q => q.BlogID == id);
            return BlogById;
        }

        public ICollection<BlogModel> GetBlogsByStudentID(string studentId)
        {
            var Blogs = FindAll();
            var BlogId = Blogs.Where(q => q.Author.Id == studentId).ToList();
            return BlogId;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public IEnumerable<BlogModel> SearchBlogs(string searchString)
        {
            var BlogSearch = _db.Blogs
                .OrderByDescending(q => q.UpdatedOn)
                .Include(q => q.Author)
                .Where(q => q.Title.Contains(searchString) || q.Content.Contains(searchString));
            return BlogSearch;
        }

        public bool Update(BlogModel entity)
        {
            _db.Blogs.Update(entity);
            return Save();
        }
    }
}
