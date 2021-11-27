using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GCUSMS.Contracts;
using GCUSMS.Data;
using GCUSMS.Models;
using Microsoft.EntityFrameworkCore;

namespace GCUSMS.Repository
{
    public class CkEditorImagesRepository : ICkEditorImagesRepository
    {
        private readonly ApplicationDbContext _db;

        public CkEditorImagesRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(CkEditorImagesModel entity)
        {
            _db.CkEditorImages.Add(entity);
            return Save();
        }

        public bool Delete(CkEditorImagesModel entity)
        {
            _db.CkEditorImages.Remove(entity);
            return Save();
        }

        public ICollection<CkEditorImagesModel> FindAll()
        {
            var CkEditorImages = _db.CkEditorImages
              .Include(q => q.CkEditorAuthor)
              .ToList();
            return CkEditorImages;
        }

        public CkEditorImagesModel FindbyId(int id)
        {
            var CkEditorImages = _db.CkEditorImages
               .Include(q => q.CkEditorAuthor)
               .FirstOrDefault(q => q.CkEditorImageId == id);
            return CkEditorImages;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public bool Update(CkEditorImagesModel entity)
        {
            _db.CkEditorImages.Update(entity);
            return Save();
        }
    }
}
