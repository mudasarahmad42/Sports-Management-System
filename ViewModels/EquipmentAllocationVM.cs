using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using GCUSMS.Models;

namespace GCUSMS.ViewModels
{
    public class EquipmentAllocationVM
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
        public string AllocatedBy { get; set; }
        public DateTime DateAllocated { get; set; }
        public bool? Returned { get; set; }
        public DateTime DateReturned { get; set; }
        public string Comments { get; set; }
        public int QuantityAccepted { get; set; }
        [Required]
        [Display(Name = "Allocated Quantity")]
        public int QuantityAllocated { get; set; }
        [ForeignKey("RequestingStudentId")]
        public StudentVM RequestingStudent { get; set; }
        public string RequestingStudentId { get; set; }
        [ForeignKey("RequestedEquipmentId")]
        public EquipmentVM RequestedEquipment { get; set; }
        public int RequestedEquipmentId { get; set; }
    }

    public class StudentEquipmentAllocationVM
    {
        public int TotalEquipmentAllocationss { get; set; }
        public int ReturnedEquipment { get; set; }
        public int PendingEquipment { get; set; }
        public int LostEquipment { get; set; }
        public List<EquipmentAllocationVM> EquipmentAllocations { get; set; }
        [ForeignKey("RequestingStudentId")]
        public StudentModel RequestingStudent { get; set; }
        public string RequestingStudentId { get; set; }
    }
}
