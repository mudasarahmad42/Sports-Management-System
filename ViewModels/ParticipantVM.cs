using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.ViewModels
{
    public class ParticipantVM
    {
        [Key]
        public int ParticipantId { get; set; }
        public DateTime DateApplied { get; set; }
        public DateTime DateSelected { get; set; }
        public bool? isSelected { get; set; }
        [ForeignKey("RequestingStudentId")]
        public StudentVM RequestingStudent { get; set; }
        public string RequestingStudentId { get; set; }
        [ForeignKey("AppliedTournamentId")]
        public TournamentVM TournamentApplied { get; set; }
        public int AppliedTournamentId { get; set; }
        [Required]
        [EmailAddress]
        public string ValidEmail { get; set; }
    }
}
