using System;
using System.ComponentModel.DataAnnotations;

namespace GCUSMS.Models
{
    public class FeedbackModel
    {
        [Key]
        public int FeedbackID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public string FeedbackMessage { get; set; }
        public DateTime DateSubmitted { get; set; }
    }
}
