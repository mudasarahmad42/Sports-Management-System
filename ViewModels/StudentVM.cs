using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GCUSMS.Models;
using Microsoft.AspNetCore.Http;

namespace GCUSMS.ViewModels
{
    public class StudentVM
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
        [Required]
        public string ProfileImagePath { get; set; }
        public int PasswordResetCount { get; set; }
        public DateTime LastPasswordReset { get; set; }
    }

    public class StudentDetailVM
    {
        //Student Record
        [ForeignKey("RequestingStudentId")]
        public StudentModel RequestingStudent { get; set; }
        public string RequestingStudentId { get; set; }
    }


    public class HODSVM
    {
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }       
        public DateTime DateCreated { get; set; }
        [Display(Name = "Upload Profile Image")]
        public IFormFile ProfileImage { get; set; }
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
