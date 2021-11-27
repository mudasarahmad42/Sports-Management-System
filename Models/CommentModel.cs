using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Models
{
    public class CommentModel
    {
        public int Id { get; set; }
        [ForeignKey("BlogID")]
        public BlogModel Blog { get; set; }
        public int BlogID { get; set; }
        [ForeignKey("CommenterID")]
        public StudentModel Author { get; set; }
        public string CommenterID { get; set; }
        public string Content { get; set; }
        public DateTime CommentedOn { get; set; }
        public virtual IEnumerable<CommentModel> Comments { get; set; }
    }
}
