using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Models
{
    public class CkEditorImagesModel
    {
        [Key]
        public int CkEditorImageId { get; set; }
        [ForeignKey("CkEditorAuthorId")]
        public StudentModel CkEditorAuthor { get; set; }
        public string CkEditorAuthorId { get; set; }
        public string CkEditorImagePath { get; set; }
        public bool IsActive { get; set; }
    }
}
