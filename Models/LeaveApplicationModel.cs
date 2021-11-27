using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Models
{
    public class LeaveApplicationModel
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Start Date")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }
        [Display(Name = "End Date")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }
        public DateTime DateRequested { get; set; }
        public DateTime DateActioned { get; set; }
        public string Message { get; set; }
        public bool? Approved { get; set; }
        public string LeaveSubject { get; set; }
        [ForeignKey("RequestingStudentId")]
        public StudentModel RequestingStudent { get; set; }
        public string RequestingStudentId { get; set; }
    }
}
