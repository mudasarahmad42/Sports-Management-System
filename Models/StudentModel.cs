using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GCUSMS.Models
{
    public class StudentModel : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string FatherName { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        [Required]
        public string RollNumber { get; set; }
        public string Section { get; set; }
        public string Session { get; set; }
        public string Semester { get; set; }
        [Required]
        public string DepartmentName { get; set; }
        public string Gender { get; set; }
        public DateTime DateCreated { get; set; }
        public string ProfileImagePath { get; set; }
        //For Editing User Details
        [NotMapped]
        [Display(Name = "Upload Profile Image")]
        public IFormFile ProfileImage { get; set; }
        //Keep Count of Password Reset
        public int PasswordResetCount { get; set; }
        public DateTime LastPasswordReset { get; set; }
    }
}
