using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GCUSMS.Contracts;
using GCUSMS.Data;
using GCUSMS.Models;

namespace GCUSMS.Repository
{
    public class EquipmentRepository : IEquipmentRepository
    {

        private readonly ApplicationDbContext _db;

        public EquipmentRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(EquipmentModel entity)
        {
            _db.Equipments.Add(entity);
            return Save();
        }

        public bool Delete(EquipmentModel entity)
        {
            _db.Equipments.Remove(entity);
            return Save();
        }

        public ICollection<EquipmentModel> FindAll()
        {
            var EquipmentsList = _db.Equipments
               .ToList();
            return EquipmentsList;
        }

        public EquipmentModel FindbyId(int id)
        {
            var EquipmentById = _db.Equipments
               .FirstOrDefault(q => q.Id == id);
            return EquipmentById;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public bool Update(EquipmentModel entity)
        {
            _db.Equipments.Update(entity);
            return Save();
        }
    }
}
