using System;
using System.ComponentModel.DataAnnotations;

namespace GCUSMS.Models
{
    public class GalleryModel
    {
        [Key]
        public int ImageId { get; set; }
        [Required]
        public string ImageName { get; set; }
        public DateTime UploadedOn { get; set; }
        public string DepartmentName { get; set; }
        public string Tournament { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
    }
}
