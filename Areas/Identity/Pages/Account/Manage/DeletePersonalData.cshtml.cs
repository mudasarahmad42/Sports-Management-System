using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using GCUSMS.Models;
using GCUSMS.Contracts;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace GCUSMS.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly INotyfService _notyf;
        private readonly UserManager<StudentModel> _userManager;
        private readonly SignInManager<StudentModel> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;
        private readonly ILeaveApplicationRepository _leaveRepo;
        private readonly IEquipmentAllocationRepository _equipmentRepo;
        private readonly ITeamPlayerRepository _repoPlayer;
        private readonly IBlogRepository _blogRepo;
        private readonly ICommentRepository _commentRepo;
        private readonly IParticipantRepository _participantRepo;
        private readonly ICkEditorImagesRepository _repoCkEditorImages;
        private readonly IWebHostEnvironment _hostEnvironment;

        public DeletePersonalDataModel
        (
            INotyfService notyf,
            UserManager<StudentModel> userManager,
            SignInManager<StudentModel> signInManager,
            ILogger<DeletePersonalDataModel> logger,
            ILeaveApplicationRepository leaveRepo,
            IEquipmentAllocationRepository equipmentRepo,
            ITeamPlayerRepository repoPlayer,
            IBlogRepository BlogRepo,
            ICommentRepository CommentRepo,
            IParticipantRepository ParticipantRepo,
            ICkEditorImagesRepository repoCkEditorImages,
            IWebHostEnvironment hostEnvironment
        )
        {
            _notyf = notyf;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _leaveRepo = leaveRepo;
            _equipmentRepo = equipmentRepo;
            _repoPlayer = repoPlayer;
            _blogRepo = BlogRepo;
            _commentRepo = CommentRepo;
            _participantRepo = ParticipantRepo;
            _repoCkEditorImages = repoCkEditorImages;
            _hostEnvironment = hostEnvironment;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password.");
                    return Page();
                }
            }

            try
            {
                //Added Logic for Deleting Relations first before deleting User
                //Getting the Equipment allocation record of this user
                var equipmentAllocationsList = _equipmentRepo.FindAll().Where(c => c.RequestingStudentId == user.Id);
                var NumberofEqAllocations = equipmentAllocationsList.Count();

                if (NumberofEqAllocations != 0)
                {
                    var allocationCounter = NumberofEqAllocations;

                    do
                    {
                        var equipmentAllocations = _equipmentRepo.FindAll().FirstOrDefault(c => c.RequestingStudentId == user.Id);
                        if (equipmentAllocations != null)
                        {
                            var resultEqAllocations = _equipmentRepo.Delete(equipmentAllocations);
                        }
                        allocationCounter--;
                    }
                    while (allocationCounter != 0);
                }

                //Getting the Leave allocation record of this user
                var leaveAllocationsList = _leaveRepo.FindAll().Where(c => c.RequestingStudentId == user.Id);
                var NumberofLeaveAllocations = leaveAllocationsList.Count();


                if (NumberofLeaveAllocations != 0)
                {
                    var leaveAllocationCounter = NumberofLeaveAllocations;

                    do
                    {
                        var leaveAllocations = _leaveRepo.FindAll().FirstOrDefault(c => c.RequestingStudentId == user.Id);
                        if (leaveAllocations != null)
                        {
                            var resultLeave = _leaveRepo.Delete(leaveAllocations);
                        }
                        leaveAllocationCounter--;

                    } while (leaveAllocationCounter != 0);
                }

                //Delete all the ckeditor images
                var CkEditorImages = _repoCkEditorImages.FindAll().Where(q => q.CkEditorAuthorId == user.Id);
                var NumberofCkEditorImages = CkEditorImages.Count();

                if (NumberofCkEditorImages != 0)
                {
                    var ckImageCounter = NumberofCkEditorImages;

                    do
                    {
                        var ckImage = _repoCkEditorImages.FindAll().FirstOrDefault(q => q.CkEditorAuthorId == user.Id);
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
                var CommentsList = _commentRepo.FindAll().Where(q => q.Author.Id == user.Id);
                var NumberofComments = CommentsList.Count();

                if (NumberofComments != 0)
                {
                    var CommentCounter = NumberofComments;

                    do
                    {
                        var Comment = _commentRepo.FindAll().FirstOrDefault(c => c.Author.Id == user.Id);
                        if (Comment != null)
                        {
                            var resultComment = _commentRepo.Delete(Comment);
                        }
                        CommentCounter--;

                    } while (CommentCounter != 0);
                }


                //Getting all the blogs by this user
                var BlogsList = _blogRepo.FindAll().Where(q => q.Author.Id == user.Id);
                var NumberofBlogs = BlogsList.Count();


                if (NumberofBlogs != 0)
                {
                    var BlogCounter = NumberofBlogs;

                    do
                    {
                        var Blog = _blogRepo.FindAll().FirstOrDefault(c => c.Author.Id == user.Id);
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
                var ParticipationList = _participantRepo.FindAll().Where(q => q.RequestingStudentId == user.Id);
                var NumberofParticipations = ParticipationList.Count();


                if (NumberofParticipations != 0)
                {
                    var ParticipationCounter = NumberofParticipations;

                    do
                    {
                        var Participant = _participantRepo.FindAll().FirstOrDefault(c => c.RequestingStudentId == user.Id);
                        if (Participant != null)
                        {
                            var resultComment = _participantRepo.Delete(Participant);
                        }
                        ParticipationCounter--;

                    } while (ParticipationCounter != 0);
                }

                //Getting all the team this user is part of
                var TeamList = _repoPlayer.FindAll().Where(q => q.PlayerId == user.Id);
                var NumberOfTeams = TeamList.Count();


                if (NumberOfTeams != 0)
                {
                    var TeamsCounter = NumberOfTeams;

                    do
                    {
                        var Player = _repoPlayer.FindAll().FirstOrDefault(c => c.PlayerId == user.Id);
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



                var result = await _userManager.DeleteAsync(user);
                var userId = await _userManager.GetUserIdAsync(user);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");
                }

                await _signInManager.SignOutAsync();

                _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

                _notyf.Success("Goodbye, We will miss you! " + user.FirstName + " " + user.LastName, 20);
                return Redirect("~/");
            }
            catch (Exception)
            {
                _notyf.Error("Something went wrong");
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
