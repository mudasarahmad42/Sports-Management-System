using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GCUSMS.Contracts;
using GCUSMS.Data;
using GCUSMS.Models;
using GCUSMS.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using GCUSMS.Areas.Identity.Pages.Account.Manage;
using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;

namespace GCUSMS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly INotyfService _notyf;
        private readonly UserManager<StudentModel> _userManager;
        private readonly SignInManager<StudentModel> _signInManager;
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILeaveApplicationRepository _leaverepo;
        private readonly IEquipmentAllocationRepository _equipmentRepo;
        private readonly ITeamPlayerRepository _repoPlayer;
        private readonly IBlogRepository _blogRepo;
        private readonly ICommentRepository _commentRepo;
        private readonly IParticipantRepository _participantRepo;
        private readonly ICkEditorImagesRepository _repoCkEditorImages;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;

        public DashboardController
        (
            INotyfService notyf,
            UserManager<StudentModel> userManager,
            ApplicationDbContext db,
            IMapper mapper,
            ILeaveApplicationRepository leaveRepo,
            IEquipmentAllocationRepository equipmentRepo,
            ITeamPlayerRepository repoPlayer,
            IBlogRepository BlogRepo,
            ICommentRepository CommentRepo,
            IParticipantRepository ParticipantRepo,
            ICkEditorImagesRepository repoCkEditorImages,
            SignInManager<StudentModel> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            IWebHostEnvironment hostEnvironment
        )
        {
            _notyf = notyf;
            _userManager = userManager;
            _db = db;
            _mapper = mapper;
            _leaverepo = leaveRepo;
            _equipmentRepo = equipmentRepo;
            _repoPlayer = repoPlayer;
            _blogRepo = BlogRepo;
            _commentRepo = CommentRepo;
            _participantRepo = ParticipantRepo;
            _repoCkEditorImages = repoCkEditorImages;
            _signInManager = signInManager;
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        // GET: DashboardController
        public ActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// Returns users list
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        public ActionResult Users()
        {
            var model = _db.Students.ToList();
            return View(model);
        }


        /// <summary>
        /// View user details
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        public ActionResult UserDetails(string id)
        {
            var Students = _db.Students.FirstOrDefault(q => q.Id == id);
            //var model = _mapper.Map<StudentVM>(Students);

            //Leaves
            ViewBag.TotalRequests = _leaverepo.GetLeaveRequestsByStudentID(id).Count();
            ViewBag.TotalApprovedRequests = _leaverepo.GetLeaveRequestsByStudentID(id).Where(q => q.Approved == true).Count();
            ViewBag.TotalRejectedRequests = _leaverepo.GetLeaveRequestsByStudentID(id).Where(q => q.Approved == false).Count();
            ViewBag.TotalPendingRequests = _leaverepo.GetLeaveRequestsByStudentID(id).Where(q => q.Approved == null).Count();



            //Equipment Allocations
            ViewBag.TotalAllocations = _equipmentRepo.GetEquipmentAllocationByStudentID(id).Count();
            ViewBag.TotalReturnedEquipment = _equipmentRepo.GetEquipmentAllocationByStudentID(id).Where(q => q.Returned == true).Count();
            ViewBag.TotalLostEquipment = _equipmentRepo.GetEquipmentAllocationByStudentID(id).Where(q => q.Returned == false).Count();
            ViewBag.TotalPendingEquipment = _equipmentRepo.GetEquipmentAllocationByStudentID(id).Where(q => q.Returned == null).Count();

            //Blogs Participations
            ViewBag.TotalBlogs = _blogRepo.GetBlogsByStudentID(id).Count();
            ViewBag.ApprovedBlogs = _blogRepo.GetBlogsByStudentID(id).Where(q => q.isApproved == true).Count();

            //Tournament Participations
            ViewBag.TotalParticipations = _participantRepo.FindAll().Where(q => q.RequestingStudentId == id).Count();
            ViewBag.Selectedin = _participantRepo.FindAll().Where(q => q.RequestingStudentId == id && q.isSelected == true).Count();

            //Teams Student is part of
            ViewBag.TotalTeams = _repoPlayer.FindAll().Where(q => q.PlayerId == id).Count();
            
            return View(Students);
        }

        /// <summary>
        /// Delete user using userId
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteUser(string id)
        { 
            //Getting the User
            var user = _db.Students.FirstOrDefault(a => a.Id == id);

            try
            {

                if (user.Id != null)
                {
                    //Getting the Equipment allocation record of this user
                    var equipmentAllocationsList = _equipmentRepo.FindAll().Where(c => c.RequestingStudentId == id);
                    var NumberofEqAllocations = equipmentAllocationsList.Count();

                    if (NumberofEqAllocations != 0)
                    {
                        var allocationCounter = NumberofEqAllocations;

                        do
                        {
                            var equipmentAllocations = _equipmentRepo.FindAll().FirstOrDefault(c => c.RequestingStudentId == id);
                            if (equipmentAllocations != null)
                            {
                                var resultEqAllocations = _equipmentRepo.Delete(equipmentAllocations);
                            }
                            allocationCounter--;
                        }
                        while (allocationCounter != 0);
                    }

                    //Getting the Leave allocation record of this user
                    var leaveAllocationsList = _leaverepo.FindAll().Where(c => c.RequestingStudentId == id);
                    var NumberofLeaveAllocations = leaveAllocationsList.Count();


                    if (NumberofLeaveAllocations != 0)
                    {
                        var leaveAllocationCounter = NumberofLeaveAllocations;

                        do
                        {
                            var leaveAllocations = _leaverepo.FindAll().FirstOrDefault(c => c.RequestingStudentId == id);
                            if (leaveAllocations != null)
                            {
                                var resultLeave = _leaverepo.Delete(leaveAllocations);
                            }
                            leaveAllocationCounter--;

                        } while (leaveAllocationCounter != 0);
                    }


                    //Delete all the ckeditor images
                    var CkEditorImages = _repoCkEditorImages.FindAll().Where(q => q.CkEditorAuthorId == id);
                    var NumberofCkEditorImages = CkEditorImages.Count();

                    if (NumberofCkEditorImages != 0)
                    {
                        var ckImageCounter = NumberofCkEditorImages;

                        do
                        {
                            var ckImage = _repoCkEditorImages.FindAll().FirstOrDefault(q => q.CkEditorAuthorId == id);
                            if (ckImage != null)
                            {
                                var resultCkImage = _repoCkEditorImages.Delete(ckImage);

                                //delete from root
                                var CkEditorImageName = ckImage.CkEditorImagePath.Replace("https://localhost:44353/images/Blogs/CKEditorImages/", "").Replace("  ", " ");
                                var ImageDel = Path.Combine(_hostEnvironment.WebRootPath, "images/Blogs/CkEditorImages", CkEditorImageName);
                                FileInfo file = new FileInfo(ImageDel);
                                if (file != null)
                                {
                                    System.IO.File.Delete(ImageDel);
                                    file.Delete();
                                }
                            }
                            ckImageCounter--;

                        } while (ckImageCounter != 0);
                    }


                    //Getting all the comments of the user
                    var CommentsList = _commentRepo.FindAll().Where(q => q.Author.Id == id);
                    var NumberofComments = CommentsList.Count();

                    if (NumberofComments != 0)
                    {
                        var CommentCounter = NumberofComments;

                        do
                        {
                            var Comment = _commentRepo.FindAll().FirstOrDefault(c => c.Author.Id == id);
                            if (Comment != null)
                            {
                                var resultComment = _commentRepo.Delete(Comment);
                            }
                            CommentCounter--;

                        } while (CommentCounter != 0);
                    }


                    //Getting all the blogs by this user
                    var BlogsList = _blogRepo.FindAll().Where(q => q.Author.Id == id);
                    var NumberofBlogs = BlogsList.Count();
                

                    if (NumberofBlogs != 0)
                    {
                        var BlogCounter = NumberofBlogs;

                        do
                        {
                            var Blog = _blogRepo.FindAll().FirstOrDefault(c => c.Author.Id == id);
                            var BlogHeaderImage = Blog.BlogImagePath;
                            if (Blog != null)
                            {
                                //Delete Image
                                var ImageDel = Path.Combine(_hostEnvironment.WebRootPath, "images/Blogs", BlogHeaderImage);
                                FileInfo file = new FileInfo(ImageDel);
                                if (file != null)
                                {
                                    System.IO.File.Delete(ImageDel);
                                    file.Delete();
                                }

                                var resultComment = _blogRepo.Delete(Blog);
                            }
                            BlogCounter--;

                        } while (BlogCounter != 0);
                    }

                    //Getting all the participation by this user
                    var ParticipationList = _participantRepo.FindAll().Where(q => q.RequestingStudentId == id);
                    var NumberofParticipations = ParticipationList.Count();


                    if (NumberofParticipations != 0)
                    {
                        var ParticipationCounter = NumberofParticipations;

                        do
                        {
                            var Participant = _participantRepo.FindAll().FirstOrDefault(c => c.RequestingStudentId == id);
                            if (Participant != null)
                            {
                                var resultComment = _participantRepo.Delete(Participant);
                            }
                            ParticipationCounter--;

                        } while (ParticipationCounter != 0);
                    }

                    //Getting all the team this user is part of
                    var TeamList = _repoPlayer.FindAll().Where(q => q.PlayerId == id);
                    var NumberOfTeams = TeamList.Count();


                    if (NumberOfTeams != 0)
                    {
                        var TeamsCounter = NumberOfTeams;

                        do
                        {
                            var Player = _repoPlayer.FindAll().FirstOrDefault(c => c.PlayerId == id);
                            if (Player != null)
                            {
                                var resultComment = _repoPlayer.Delete(Player);
                            }
                            TeamsCounter--;

                        } while (TeamsCounter != 0);
                    }

                    //Delete Prfile Image from root
                    if (user.ProfileImagePath != null)
                    {
                        var ImageDel = Path.Combine(_hostEnvironment.WebRootPath, "images/Users", user.ProfileImagePath);
                        FileInfo file = new FileInfo(ImageDel);
                        if (file != null)
                        {
                            System.IO.File.Delete(ImageDel);
                            file.Delete();
                            user.ProfileImagePath = null;
                        }
                    }

                    //Delete User Now
                    var resultStudent = _db.Students.Remove(user);
                    int changes = await _db.SaveChangesAsync();

                    if (changes < 0)
                    {
                        _notyf.Error("Unexpected error occurred deleting user with ID: " + user.Id, 10);
                        throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{user.Id}'.");
                    }

                    _notyf.Information("Admin Deleted user with ID: " + user.Id + " successfully ", 20);
                    _notyf.Information("Full Name: " + user.FirstName + " " + user.LastName, 10);
                    _notyf.Information("Roll Number: " + user.RollNumber, 7);

                    _logger.LogInformation("Admin Deleted user with ID '{UserId}' ", user.Id);
                }
                else
                {
                    _notyf.Error("User does not exist", 20);
                }

            }
            catch (Exception)
            {
                _notyf.Error("Something went wrong");
                return RedirectToAction("Index","Home");
            }

            return RedirectToAction("Users");
        }


        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);

                var userDepartmentName = user.DepartmentName;

                if (userDepartmentName != null)
                {
                    ViewBag.userDepartmentName = userDepartmentName;
                }

                if (user == null)
                {
                    ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                    _notyf.Error("User Not Found ", 7);
                    return RedirectToAction("UserDetails", new { Id = userId });
                }

                // UserManager service GetClaimsAsync method gets all the current claims of the user
                var existingUserClaims = await _userManager.GetClaimsAsync(user);

                var model = new UserClaimsVM
                {
                    UserId = userId
                };

                // Loop through each claim we have in our application
                foreach (Claim claim in ClaimsStore.AllClaims)
                {
                    UserClaims userClaim = new UserClaims
                    {
                        ClaimType = claim.Type
                    };


                    // If the user has the claim, set IsSelected property to true, so the checkbox
                    // next to the claim is checked on the UI
                    if (existingUserClaims.Any(c => c.Type == claim.Type))
                    {
                        userClaim.IsSelected = true;
                    }

                    model.Claims.Add(userClaim);
                }

                return View(model);
            }
            else
            {
                _notyf.Warning("User Does not Exist");
                return RedirectToAction("Index");
            }

        }

        //POST ManageUserClaims
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsVM model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.UserId} cannot be found";
                _notyf.Error("User Not Found ", 7);
                return RedirectToAction("UserDetails", new { Id = model.UserId });
            }

            // Get all the user existing claims and delete them
            var claims = await _userManager.GetClaimsAsync(user);
            var result = await _userManager.RemoveClaimsAsync(user, claims);
        
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing claims");
                _notyf.Error("Cannot remove user existing claims");
                return View(model);
            }

            // Add all the claims that are selected on the UI
            result = await _userManager.AddClaimsAsync(user,
                model.Claims.Where(c => c.IsSelected).Select(c => new Claim(c.ClaimType, c.ClaimType)));

            _notyf.Success("Claims of User with ID: " + model.UserId + " has been Updated successfully ");
            _notyf.Information("You have updated rights of this user fo this application", 12);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected claims to user");
                _notyf.Error("Cannot add selected claims to user");
                return View(model);
            }

            return RedirectToAction("UserDetails", new { Id = model.UserId });

        }


        /// <summary>
        /// Method to check authoriztion of 'CAPTAIN' claim
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "MakeCaptain")]
        public IActionResult OnlyCaptainPage()
        {
            return View();
        }

        //Update ProfileImage
        [Authorize]
        private string EditImage(StudentModel model)
        {
            string uniqueFileName = null;


            if (model.ProfileImage != null)
            {
                //Check if the Upload file is Image?
                var FileName = model.ProfileImage.FileName;

                var allowedExtensions = new[] { ".gif", ".png", ".jpg", ".jpeg", ".webp", ".svg" };
                var extension = Path.GetExtension(FileName);
                if (!allowedExtensions.Contains(extension))
                {
                    _notyf.Warning(extension.ToUpper() + " File type is not Allowed");
                    return null;
                }
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

        //Edit Student Data By Admin
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditUserProfile(string userId)
        {
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                return View(user);
            }
            else
            {
                _notyf.Warning("User Does not Exist");
                return RedirectToAction("Index");
            }
        }

        //Edit Student Data By Admin
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UserProfileEdit(StudentModel model)
        {
            var StudentId = model.Id;

            if (!ModelState.IsValid)
            {             
                _notyf.Error("Something Went Wrong!");
                return RedirectToAction("UserDetails", new { id = StudentId });
            }

                // Retrieve the user being edited from the database
                var userEdit = await _userManager.FindByIdAsync(StudentId);
                    
                //Get User age
                var userAge = _userManager.FindByIdAsync(StudentId).Result.Age;

                //Get Profile Image
                string uniqueFileName = EditImage(model);

                if (uniqueFileName != null)
                {
                    //delete from root
                    if (userEdit.ProfileImagePath != null)
                    {
                        var ImageDel = Path.Combine(_hostEnvironment.WebRootPath, "images/Users", userEdit.ProfileImagePath);
                        FileInfo file = new FileInfo(ImageDel);
                        if (file != null)
                        {
                            System.IO.File.Delete(ImageDel);
                            file.Delete();
                            userEdit.ProfileImagePath = null;
                        }
                    }
                }

                if (uniqueFileName != null)
                {
                    userEdit.ProfileImagePath = uniqueFileName;
                }

                if (model.FirstName != null)
                {
                    userEdit.FirstName = model.FirstName.ToUpper();
                }

                if (model.LastName != null)
                {
                    userEdit.LastName = model.LastName.ToUpper();
                }

                if (model.FatherName != null)
                {
                    userEdit.FatherName = model.FatherName.ToUpper();
                }

              
                userEdit.DateOfBirth = model.DateOfBirth;
                

                if (model.RollNumber != null)
                {
                    userEdit.RollNumber = model.RollNumber.ToUpper();
                }

                if (model.Section != null)
                {
                    userEdit.Section = model.Section.ToUpper();
                }

                if (model.Semester != null)
                {
                    userEdit.Semester = model.Semester;
                }

                if (model.Session != null)
                {
                    userEdit.Session = model.Session.ToUpper(); ;
                }

                if (model.Age != userAge && model.Age > 0)
                {
                    userEdit.Age = model.Age;
                }

                if (model.DepartmentName != null)
                {
                    userEdit.DepartmentName = model.DepartmentName.ToUpper();
                }

                if (model.Gender != null)
                {
                    userEdit.Gender = model.Gender.ToUpper();
                }

                var isUpdated = _db.Update(userEdit);
                var isSuccess = await _db.SaveChangesAsync();

            _notyf.Success("User Details are Updated");
            return RedirectToAction("UserDetails", new { id = StudentId });
        }


        //Password Reset by Admin
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ResetUserPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _notyf.Error("Page not found");
                return RedirectToAction("Index");
                //return NotFound();
            }

            //Update Password
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user , "P@ssword1");

            //Update Password Count
            user.PasswordResetCount++;
            user.LastPasswordReset = DateTime.Now;


            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _notyf.Error("Something Went Wrong!");
                return RedirectToAction("EditUserProfile", new { userId = id });
            }

            _notyf.Success("Password has been updated");
            _notyf.Information("New Password is: P@ssword1" , 30);
            return RedirectToAction("EditUserProfile", new { userId = id });
        }

        /// <summary>
        /// Returns profile of the logged-in user
        /// </summary>
        /// <returns></returns>
        public IActionResult MyProfile()
        {
            //Get Current LoggedIn user id

            var UserID = _userManager.GetUserId(User);

            //We are accessing database directly here without any repository pattern
            var Students = _db.Students.FirstOrDefault(q => q.Id == UserID);

            //Student Details
            ViewBag.StudentName = Students.FirstName + " " + Students.LastName;
            ViewBag.PasswordResetCount = Students.PasswordResetCount;
            ViewBag.LastResetDate = Students.LastPasswordReset.ToString("dddd, dd MMMM yyyy h:mm tt");

            //Leaves
            ViewBag.TotalRequests = _leaverepo.GetLeaveRequestsByStudentID(UserID).Count();
            ViewBag.TotalApprovedRequests = _leaverepo.GetLeaveRequestsByStudentID(UserID).Where(q => q.Approved == true).Count();
            ViewBag.TotalRejectedRequests = _leaverepo.GetLeaveRequestsByStudentID(UserID).Where(q => q.Approved == false).Count();
            ViewBag.TotalPendingRequests = _leaverepo.GetLeaveRequestsByStudentID(UserID).Where(q => q.Approved == null).Count();



            //Equipment Allocations
            ViewBag.TotalAllocations = _equipmentRepo.GetEquipmentAllocationByStudentID(UserID).Count();
            ViewBag.TotalReturnedEquipment = _equipmentRepo.GetEquipmentAllocationByStudentID(UserID).Where(q => q.Returned == true).Count();
            ViewBag.TotalLostEquipment = _equipmentRepo.GetEquipmentAllocationByStudentID(UserID).Where(q => q.Returned == false).Count();
            ViewBag.TotalPendingEquipment = _equipmentRepo.GetEquipmentAllocationByStudentID(UserID).Where(q => q.Returned == null).Count();

            //Tournament Participations
            ViewBag.TotalParticipations = _participantRepo.FindAll().Where(q => q.RequestingStudentId == UserID).Count();
            ViewBag.Selectedin = _participantRepo.FindAll().Where(q => q.RequestingStudentId == UserID && q.isSelected == true).Count();


            //Blogs Written
            ViewBag.TotalBlogs = _blogRepo.GetBlogsByStudentID(UserID).Count();
            ViewBag.ApprovedBlogs = _blogRepo.GetBlogsByStudentID(UserID).Where(q => q.isApproved == true).Count();

            //Teams Student is part of
            ViewBag.TotalTeams = _repoPlayer.FindAll().Where(q => q.PlayerId == UserID).Count();

            return View(Students);
        }


        public IActionResult CreateHODS()
        {
            return View();
        }

        /// <summary>
        /// It creates a user with role of head of departmental sports
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateHODS(HODSVM model)
        {
            try
            {
                string ProfileImageFileName = UploadedFile(model);

                if (ProfileImageFileName == null)
                {
                    _notyf.Error("Please Upload an Image File Only ", 7);
                    return View(model);
                }

                if (ModelState.IsValid)
                {

                    var HODuser = new StudentModel
                    {
                        FirstName = model.DepartmentName.ToUpper(),
                        LastName = "HODS",
                        DepartmentName = model.DepartmentName.ToUpper(),
                        DateCreated = DateTime.Now,
                        ProfileImagePath = ProfileImageFileName,
                        UserName = model.Email,
                        Email = model.Email,
                    };

                    var result = await _userManager.CreateAsync(HODuser, model.Password);

                    if (result.Succeeded)
                    {
                        //Add HODS role to user
                        _userManager.AddToRoleAsync(HODuser, "HODS").Wait();

                        //Add Captain Claim to the user
                        _userManager.AddClaimAsync(HODuser, new Claim("Make Captain", "Make Captain")).Wait();
                        _notyf.Success("Head of Departmental Sports account has been created successfully");
                        _logger.LogInformation("Admin created a new account with password.");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        _notyf.Error(error.Description);
                    }
                }

                return View();
            }
            catch (Exception)
            {

                _notyf.Error("Something Went Wrong!");
                return View();
            }
        }

        private string UploadedFile(HODSVM model)
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
