using GCUSMS.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GCUSMS.ViewModels
{
    public class CommentVM
    {
        public int Id { get; set; }
        [ForeignKey("BlogID")]
        public BlogVM Blog { get; set; }
        public int BlogID { get; set; }
        [ForeignKey("CommenterID")]
        public StudentVM Author { get; set; }
        public string CommenterID { get; set; }
        public string Content { get; set; }
        public DateTime CommentedOn { get; set; }
    }
}
