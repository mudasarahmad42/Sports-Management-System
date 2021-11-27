using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GCUSMS.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using AspNetCoreHero.ToastNotification.Abstractions;
using GCUSMS.Data;

namespace GCUSMS.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<StudentModel> _userManager;
        private readonly SignInManager<StudentModel> _signInManager;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly INotyfService _notyf;
        private readonly ApplicationDbContext _db;

        public IndexModel(
            UserManager<StudentModel> userManager,
            SignInManager<StudentModel> signInManager,
            IWebHostEnvironment hostEnvironment,
            INotyfService notyf,
            ApplicationDbContext db
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _hostEnvironment = hostEnvironment;
            _notyf = notyf;
            _db = db;
        }

        public string Username { get; set; }

        public string BeforeImagePath { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        //DEFAULT EDIT MODEL
        //public class InputModel
        //{
        //    [Phone]
        //    [Display(Name = "Phone number")]
        //    public string PhoneNumber { get; set; }
        //}

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            //Student Model
            public int Age { get; set; }
            public string Section { get; set; }
            public string Session { get; set; }
            public string Semester { get; set; }

            public string DepartmentName { get; set; }

            public string Gender { get; set; }

            public IFormFile EditProfileImage { get; set; }

            public string ProfileImagePath { get; set; }
        }

        private async Task LoadAsync(StudentModel user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var Department = _userManager.GetUserAsync(User).Result.DepartmentName;
            var Gender = _userManager.GetUserAsync(User).Result.Gender;
            var Section = _userManager.GetUserAsync(User).Result.Section;
            var Session = _userManager.GetUserAsync(User).Result.Session;
            var Semester = _userManager.GetUserAsync(User).Result.Semester;

            var ImagePathBefore = _userManager.GetUserAsync(User).Result.ProfileImagePath;

            Username = userName;
            BeforeImagePath = ImagePathBefore;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                DepartmentName = Department,
                Gender = Gender,
                Section = Section,
                Session = Session,
                Semester = Semester
                
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            //Updating Phone Number
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            //Updating Profile Image
            var UserImage = user.ProfileImagePath;

            if (Input.EditProfileImage != null)
            {

                string uniqueFileName = UploadedFile(Input);

                if (uniqueFileName != null)
                {
                    //delete from root
                    if (UserImage != null)
                    {
                        var ImageDel = Path.Combine(_hostEnvironment.WebRootPath, "images/Users", UserImage);
                        FileInfo file = new FileInfo(ImageDel);
                        if (file != null)
                        {
                            System.IO.File.Delete(ImageDel);
                            file.Delete();
                        }
                    }

                    user.ProfileImagePath = uniqueFileName;
                }

            }

            //Updating Age
            var Age = _userManager.GetUserAsync(User).Result.Age;

            if (Input.Age != Age)
            {
                user.Age = Input.Age;
            }


            /*UNCOMMENT THIS IF YOU WANT USERS TO UPDATE THEIR DEPARTMENTS ON THEIR OWN*/
            ////Updating Department
            //var Department = _userManager.GetUserAsync(User).Result.DepartmentName;

            //if (Input.DepartmentName != Department && Input.DepartmentName != null)
            //{
            //    user.DepartmentName = Input.DepartmentName.ToUpper();
            //}

            //Updating Gender
            var Gender = _userManager.GetUserAsync(User).Result.Gender;

            if (Input.Gender != Gender && Input.Gender != null)
            {
                user.Gender = Input.Gender.ToUpper();
            }

            //Updating Section
            var Section = _userManager.GetUserAsync(User).Result.Section;

            if (Input.Section != Section && Input.Section != null)
            {
                user.Section = Input.Section.ToUpper(); ;
            }

            //Updating Session
            var Session = _userManager.GetUserAsync(User).Result.Session;

            if (Input.Session != Session && Input.Session != null)
            {
                user.Session = Input.Session.ToUpper(); ;
            }

            //Updating Semester
            var Semester = _userManager.GetUserAsync(User).Result.Semester;

            if (Input.Semester != Semester && Input.Semester != null)
            {
                user.Semester = Input.Semester.ToUpper(); ;
            }

            //Updating StudentModel
            await _db.SaveChangesAsync();

            _notyf.Success("Your Profile has been updated");
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        private string UploadedFile(InputModel model)
        {
            string uniqueFileName = null;

            if (model != null)
            {
                //Check if the Upload file is Image?
                var FileName = model.EditProfileImage.FileName;
                var allowedExtensions = new[] { ".gif", ".png", ".jpg", ".jpeg", ".webp", ".svg" };
                var extension = Path.GetExtension(FileName);
                if (!allowedExtensions.Contains(extension))
                {
                    _notyf.Warning(extension.ToUpper() + " File types are not Allowed");
                    return null;
                }

                if (model.EditProfileImage != null)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images/Users");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.EditProfileImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.EditProfileImage.CopyTo(fileStream);
                    }
                }
                return uniqueFileName;
            }
            else
            {
                return null;
            }
        }
    }
}
