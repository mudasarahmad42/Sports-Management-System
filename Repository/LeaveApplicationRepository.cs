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
    public class LeaveApplicationRepository : ILeaveApplicationRepository
    {
        private readonly ApplicationDbContext _db;

        public LeaveApplicationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(LeaveApplicationModel entity)
        {
            _db.LeaveApplications.Add(entity);
            return Save();
        }

        public bool Delete(LeaveApplicationModel entity)
        {
            _db.LeaveApplications.Remove(entity);
            return Save();
        }

        public ICollection<LeaveApplicationModel> FindAll()
        {
            var LeaveApplication = _db.LeaveApplications
               .Include(q => q.RequestingStudent)
               .ToList();
            return LeaveApplication;
        }

        public LeaveApplicationModel FindbyId(int id)
        {
            var LeaveApplication =  _db.LeaveApplications
               .Include(q => q.RequestingStudent)
               .FirstOrDefault(q => q.Id == id);
            return LeaveApplication;
        }

        public ICollection<LeaveApplicationModel> GetLeaveRequestsByStudentID(string studentId)
        {
            var leaveRequests = FindAll();
            return leaveRequests.Where(q => q.RequestingStudentId == studentId)
            .ToList();
        }

        public bool Save()
        {
            var changes = _db.SaveChanges();
            return changes > 0;
        }

        public bool Update(LeaveApplicationModel entity)
        {
            _db.LeaveApplications.Update(entity);
            return Save();
        }
    }
}
