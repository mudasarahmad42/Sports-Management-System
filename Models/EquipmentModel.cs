using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Models
{
    public class EquipmentModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Equipment Name")]
        public string EquipmentName { get; set; }
        [Required]
        [Display(Name = "Type of the Equipment")]
        public string EquipmentType { get; set; }       
        [Required]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }
        [Required]
        public string Condition { get; set; }
        public string Description { get; set; }
        [Display(Name = "Date Entered")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateEntered { get; set; }

    }
}
