using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GCUSMS.Models;

namespace GCUSMS.ViewModels
{
    public class LeaveApplicationVM
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }
        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }
        public DateTime DateRequested { get; set; }
        public DateTime DateActioned { get; set; }
        public string Message { get; set; }
        public bool? Approved { get; set; }
        public string LeaveSubject { get; set; }
        [ForeignKey("RequestingStudentId")]
        public StudentVM RequestingStudent { get; set; }
        public string RequestingStudentId { get; set; }
        public string TrimmedMessage
        {
            get
            {
                if (this.Message.Length > 20)
                    return this.Message.Substring(0, 7) + "...";
                else
                    return this.Message;
            }
        }
    }

    public class AdminLeaveApplicationVM
    {
        public int TotalRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int PendingRequests { get; set; }
        public int RejectedRequests { get; set; }

        public List<LeaveApplicationVM> LeaveApplications { get; set; }
    }

    public class StudentLeaveApplicationVM
    {
        public int TotalRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int PendingRequests { get; set; }
        public int RejectedRequests { get; set; }
        public List<LeaveApplicationVM> LeaveApplications { get; set; }

        [ForeignKey("RequestingStudentId")]
        public StudentModel RequestingStudent { get; set; }
        public string RequestingStudentId { get; set; }
    }

    public class CreateLeaveApplicationsVM
    {
        [Display(Name = "Start Date")]
        [Required]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        [Required]
        public DateTime EndDate { get; set; }
        [Display(Name = "Message")]
        [MaxLength(2000)]
        public string Message { get; set; }
        public string LeaveSubject { get; set; }
    }
}
