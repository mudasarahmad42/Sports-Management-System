using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Models
{
    public class BlogModel
    {
        [Key]
        public int BlogID { get; set; }
        public StudentModel Author { get; set; }
        [Required]
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool isPublished  { get; set; } = false;
        public bool isApproved { get; set; } = false;
        public string EditedBy { get; set; }
        public string Excerpt { get; set; }
        public string Category { get; set; }
        public string BlogImagePath { get; set; }
        public virtual IEnumerable<CommentModel> Comments { get; set; }
    }
}
