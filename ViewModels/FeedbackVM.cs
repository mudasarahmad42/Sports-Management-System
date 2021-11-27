using System;
using System.ComponentModel.DataAnnotations;

namespace GCUSMS.ViewModels
{
    public class FeedbackVM
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
