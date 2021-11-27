using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GCUSMS.Models;

namespace GCUSMS.Contracts
{
    public interface ILeaveApplicationRepository : IRepositoryBase<LeaveApplicationModel>
    {
        ICollection<LeaveApplicationModel> GetLeaveRequestsByStudentID(string studentId);
    }
}
