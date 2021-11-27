using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GCUSMS.Models;

namespace GCUSMS.Contracts
{
   public interface IEquipmentAllocationRepository : IRepositoryBase<EquipmentAllocationModel>
   {
        ICollection<EquipmentAllocationModel> GetEquipmentAllocationByStudentID(string studentId);     
   }
}
