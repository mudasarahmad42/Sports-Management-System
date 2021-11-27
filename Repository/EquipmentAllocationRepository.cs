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
    public class EquipmentAllocationRepository : IEquipmentAllocationRepository
    {
        private readonly ApplicationDbContext _db;

        public EquipmentAllocationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(EquipmentAllocationModel entity)
        {
            _db.EquipmentAllocations.Add(entity);
            return Save();
        }

        public bool Delete(EquipmentAllocationModel entity)
        {
            _db.EquipmentAllocations.Remove(entity);
            return Save();
        }

        public ICollection<EquipmentAllocationModel> FindAll()
        {
            var EquipmentsAllocationList = _db.EquipmentAllocations
                .Include(q => q.RequestingStudent)
                .Include(c => c.RequestedEquipment)
               .ToList();
            return EquipmentsAllocationList;
        }

        public EquipmentAllocationModel FindbyId(int id)
        {
            var EquipmentAllocationsById = _db.EquipmentAllocations
                .Include(q => q.RequestingStudent)
                .Include(c => c.RequestedEquipment)
               .FirstOrDefault(a => a.Id == id);
            return EquipmentAllocationsById;
        }

        public ICollection<EquipmentAllocationModel> GetEquipmentAllocationByStudentID(string studentId)
        {
            var EquipmentAllocations = FindAll();
            var EquipmentAllocationId = EquipmentAllocations.Where(q => q.RequestingStudentId == studentId).ToList();
            return EquipmentAllocationId;
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;

        }

        public bool Update(EquipmentAllocationModel entity)
        {
            _db.EquipmentAllocations.Update(entity);
            return Save();
        }
    }
}
