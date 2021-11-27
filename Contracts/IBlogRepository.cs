using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GCUSMS.Models;

namespace GCUSMS.Contracts
{
    public interface IBlogRepository : IRepositoryBase<BlogModel>
    {
        public IEnumerable<BlogModel> SearchBlogs(string search);
        ICollection<BlogModel> GetBlogsByStudentID(string studentId);
    }
}
