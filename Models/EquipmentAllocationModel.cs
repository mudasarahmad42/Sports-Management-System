using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Models
{
    public class EquipmentAllocationModel
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
        [Required]
        public int QuantityAllocated { get; set; }
        public DateTime DateAllocated { get; set; }
        public bool? Returned { get; set; }
        public DateTime DateReturned { get; set; }
        public string Comments { get; set; }
        public int QuantityAccepted { get; set; }
        [ForeignKey("RequestingStudentId")]
        public StudentModel RequestingStudent { get; set; }
        public string RequestingStudentId { get; set; }
        [ForeignKey("RequestedEquipmentId")]
        public EquipmentModel RequestedEquipment { get; set; }
        public int RequestedEquipmentId { get; set; }

    }
}
