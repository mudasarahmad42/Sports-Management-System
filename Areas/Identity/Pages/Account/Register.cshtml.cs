using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using GCUSMS.Models;
using GCUSMS.Data;

namespace GCUSMS.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<StudentModel> _signInManager;
        private readonly UserManager<StudentModel> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly INotyfService _notyf;
        private readonly ApplicationDbContext _db;

        public RegisterModel(
            UserManager<StudentModel> userManager,
            SignInManager<StudentModel> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IWebHostEnvironment hostEnvironment,
            INotyfService notyf,
            ApplicationDbContext db
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _hostEnvironment = hostEnvironment;
            _notyf = notyf;
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
           
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Father Name")]
            public string FatherName { get; set; }

            [Required]
            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
            public DateTime DateOfBirth { get; set; }

           
            [Display(Name = "Age")]
            public int Age { get; set; }

            [Required]
            [Display(Name = "Roll Number")]
            public string RollNumber { get; set; }

            [Display(Name = "Section")]
            public string Section { get; set; }

            [Display(Name = "Session")]
            public string Session { get; set; }

            [Display(Name = "Semester")]
            public string Semester { get; set; }

            [Required]
            [Display(Name = "Department Name")]
            public string DepartmentName { get; set; }

            [Required]
            public string Gender { get; set; }

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

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");


            //Uncomment this if you want to bypass null reference exception

            if (Input.RollNumber == null || Input.Email == null)
            {
                _notyf.Error("Something Went wrong");
                return Page();
            }
            var StudentList = _db.Students.ToList();
            var ExistingRollNumber = StudentList.Where(q => q.RollNumber == Input.RollNumber.ToUpper()).Any();

            if (ExistingRollNumber)
            {
                _notyf.Error("Account with this Roll Number Already Exist");
                return Page();
            }

            string ProfileImageFileName = UploadedFile(Input);

            if (ProfileImageFileName == null)
            {
                _notyf.Error("Please Upload an Image File Only ", 7);
                return Page();
            }


            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {

                var user = new StudentModel
                {
                    FirstName = Input.FirstName.ToUpper(),
                    LastName = Input.LastName.ToUpper(),
                    FatherName = Input.FatherName.ToUpper(),
                    DateOfBirth = Input.DateOfBirth,
                    Age = Input.Age,
                    RollNumber = Input.RollNumber.ToUpper(),
                    Section = Input.Section.ToUpper(),
                    Session = Input.Session.ToUpper(),
                    Semester = Input.Semester,
                    DepartmentName = Input.DepartmentName.ToUpper(),
                    Gender = Input.Gender.ToUpper(),
                    DateCreated = DateTime.Now,
                    ProfileImagePath = ProfileImageFileName,
                    UserName = Input.Email,
                    Email = Input.Email,
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(user, "Student").Wait();
                    _notyf.Success("User Account has been created successfully");
                    _logger.LogInformation("User created a new account with password.");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _notyf.Error(error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private string UploadedFile(InputModel model)
        {
            string uniqueFileName = null;

            //Check if the Upload file is Image?
            var FileName = model.ProfileImage.FileName;
            var allowedExtensions = new[] { ".gif", ".png", ".jpg", ".jpeg", ".webp" };
            var extension = Path.GetExtension(FileName);
            if (!allowedExtensions.Contains(extension))
            {
                _notyf.Warning(extension.ToUpper() + " File types are not Allowed");
                return null;
            }

            if (model.ProfileImage != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images/Users");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProfileImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.ProfileImage.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

    }
}
