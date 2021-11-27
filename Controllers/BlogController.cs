using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using GCUSMS.Contracts;
using GCUSMS.Models;
using GCUSMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GCUSMS2.Controllers
{
    [Authorize]
    public class BlogController : Controller
    {
        private readonly IBlogRepository _repo;
        private readonly ICommentRepository _repoComment;
        private readonly ICkEditorImagesRepository _repoCkEditor;
        private readonly IMapper _mapper;
        private readonly UserManager<StudentModel> _userManager;
        private readonly SignInManager<StudentModel> _signInManager;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _hostEnvironment;

        public BlogController
        (
            IBlogRepository repo,
            ICommentRepository repoComment,
            ICkEditorImagesRepository repoCkEditor,
            IMapper mapper,
            UserManager<StudentModel> userManager,
            SignInManager<StudentModel> signInManager,
            INotyfService notyf,
            IWebHostEnvironment hostEnvironment
        )
        {
            _repo = repo;
            _repoComment = repoComment;
            _repoCkEditor = repoCkEditor;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _notyf = notyf;
            _hostEnvironment = hostEnvironment;
        }


        //Front Blog Page that is visible to everyone
        [AllowAnonymous]
        public IActionResult Index(string searchString, int? page)
        {
            var blogs = GetBlogsBySearchIndex(searchString, page);
            return View(blogs);
        }


        //One create method that is used to create a blog both for students and admin
        //Returns view
        [Authorize]
        public IActionResult Create()
        {
            var user = _userManager.GetUserAsync(User).Result;

            var NumberOfImages = _repoCkEditor.FindAll().Where(q => q.CkEditorAuthorId == user.Id).Count();
            var NumberOfUnusedImages = _repoCkEditor.FindAll().Where(q => q.CkEditorAuthorId == user.Id && q.IsActive == false).Count();

            ViewBag.NumberOfImages = NumberOfImages;
            ViewBag.NumberOfUnusedImages = NumberOfUnusedImages;

            return View();
        }


        //Main Index Page Search
        /// <summary>
        /// Returns a model with the blogs that contains the search string
        /// and are approved by admin |
        /// for main blog page
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public SearchBlogVM GetBlogsBySearchIndex(string searchString, int? page)
        {
            int pageSize = 9;
            int pageNumber = page ?? 1;
            var blogs = _repo.SearchBlogs(searchString ?? string.Empty).Where(q => q.isPublished == true && q.isApproved == true && q.Category != "News");

            return new SearchBlogVM
            {
                Blogs = new StaticPagedList<BlogModel>(blogs.Skip((pageNumber - 1) * pageSize).Take(pageSize), pageNumber, pageSize, blogs.Count()),
                SearchString = searchString,
                PageNumber = pageNumber
            };
        }


        //Main News Page Search
        /// <summary>
        /// Returns a model with the news that contains the search string
        /// and are made public by admin |
        /// for main news page
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public SearchBlogVM GetNewsBySearchIndex(string searchString, int? page)
        {
            int pageSize = 9;
            int pageNumber = page ?? 1;
            var blogs = _repo.SearchBlogs(searchString ?? string.Empty).Where(q => q.isPublished == true && q.isApproved == true && q.Category == "News");

            return new SearchBlogVM
            {
                Blogs = new StaticPagedList<BlogModel>(blogs.Skip((pageNumber - 1) * pageSize).Take(pageSize), pageNumber, pageSize, blogs.Count()),
                SearchString = searchString,
                PageNumber = pageNumber
            };
        }


        //Recommendation Page Search
        /// <summary>
        /// Returns blogs of the author whose id is passed as the parameter | for blog recommendation page
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public SearchBlogVM GetBlogsBySearchIndexRecommendation(string searchString, int? page, string id)
        {
            int pageSize = 9;
            int pageNumber = page ?? 1;
            var blogs = _repo.SearchBlogs(searchString ?? string.Empty).Where(q => q.isPublished == true && q.isApproved == true && q.Author.Id == id);

            return new SearchBlogVM
            {
                Blogs = new StaticPagedList<BlogModel>(blogs.Skip((pageNumber - 1) * pageSize).Take(pageSize), pageNumber, pageSize, blogs.Count()),
                SearchString = searchString,
                PageNumber = pageNumber
            };
        }


        //Search method for news in dashboard
        /// <summary>
        /// Returns news to admin, news have to be published by its author (admin) | for admin dashboard
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        public SearchBlogVM GetNewsBySearchAdmin(string searchString, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var blogs = _repo.SearchBlogs(searchString ?? string.Empty).Where(q => q.isPublished == true && q.Category == "News");

            return new SearchBlogVM
            {
                Blogs = new StaticPagedList<BlogModel>(blogs.Skip((pageNumber - 1) * pageSize).Take(pageSize), pageNumber, pageSize, blogs.Count()),
                SearchString = searchString,
                PageNumber = pageNumber
            };
        }


        //Get Blogs search in dashboard
        /// <summary>
        /// Returns blog to admin, blogs have to be published by its author | for admin dashboard
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        public SearchBlogVM GetBlogsBySearchAdmin(string searchString, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var blogs = _repo.SearchBlogs(searchString ?? string.Empty).Where(q => q.isPublished == true && q.Category == null);

            return new SearchBlogVM
            {
                Blogs = new StaticPagedList<BlogModel>(blogs.Skip((pageNumber - 1) * pageSize).Take(pageSize), pageNumber, pageSize, blogs.Count()),
                SearchString = searchString,
                PageNumber = pageNumber
            };
        }


        //Search for unapproved in dashboard
        /// <summary>
        /// Returns blogs that are published by its author but are not yet approved by admin | for admin dashboard
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        public SearchBlogVM GetBlogsBySearchAdminForUnapproved(string searchString, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var blogs = _repo.SearchBlogs(searchString ?? string.Empty).Where(q => q.isApproved == false && q.isPublished == true && q.Category == null);

            return new SearchBlogVM
            {
                Blogs = new StaticPagedList<BlogModel>(blogs.Skip((pageNumber - 1) * pageSize).Take(pageSize), pageNumber, pageSize, blogs.Count()),
                SearchString = searchString,
                PageNumber = pageNumber
            };
        }

        //Search unapproved news in dashboard
        /// <summary>
        /// Returns news that are published by its author (admin) but are not yet approved by admin | for admin dashboard
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        public SearchBlogVM GetNewsBySearchAdminForUnapproved(string searchString, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var blogs = _repo.SearchBlogs(searchString ?? string.Empty).Where(q => q.isApproved == false && q.isPublished == true && q.Category == "News");

            return new SearchBlogVM
            {
                Blogs = new StaticPagedList<BlogModel>(blogs.Skip((pageNumber - 1) * pageSize).Take(pageSize), pageNumber, pageSize, blogs.Count()),
                SearchString = searchString,
                PageNumber = pageNumber
            };
        }


        //Search Blog in dashboard by student
        /// <summary>
        /// Returns blogs that matches the search string | for student dashboard
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public SearchBlogVM GetBlogsBySearchStudent(string searchString, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var author = _userManager.GetUserAsync(User).Result;
            var blogs = _repo.SearchBlogs(searchString ?? string.Empty).Where(q => q.Author == author);

            return new SearchBlogVM
            {
                Blogs = new StaticPagedList<BlogModel>(blogs.Skip((pageNumber - 1) * pageSize).Take(pageSize), pageNumber, pageSize, blogs.Count()),
                SearchString = searchString,
                PageNumber = pageNumber
            };
        }


        /// <summary>
        /// Returns blogs that are not approved by admin
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public SearchBlogVM GetBlogsBySearchStudentForPrivateBlogs(string searchString, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;
            var author = _userManager.GetUserAsync(User).Result;
            var blogs = _repo.SearchBlogs(searchString ?? string.Empty).Where(q => q.Author == author && q.isApproved == false);

            return new SearchBlogVM
            {
                Blogs = new StaticPagedList<BlogModel>(blogs.Skip((pageNumber - 1) * pageSize).Take(pageSize), pageNumber, pageSize, blogs.Count()),
                SearchString = searchString,
                PageNumber = pageNumber
            };
        }


        [Authorize]
        [HttpPost]
        public IActionResult Create(CreateBlogVM model)
        {
            try
            {

                string uniqueFileName = UploadedBlogImage(model);

                if (uniqueFileName == null)
                {
                    _notyf.Error("Please Upload an Image File Only ", 7);
                    return RedirectToAction("Create");
                }

                var author = _userManager.GetUserAsync(User).Result;

                if (!ModelState.IsValid)
                {
                    _notyf.Information("Something Went Wrong!!");
                    return RedirectToAction("Create");
                }

                /*Creating blog model with complete author object
                 so i can access all details i have for that author
                 */

                var BlogModel = new BlogVM
                {
                    Title = model.Title,
                    Content = model.Content,
                    Author = author,
                    CreatedOn = DateTime.Now,
                    BlogImagePath = uniqueFileName,
                    Excerpt = model.Excerpt,
                    isPublished = model.isPublished,
                    Category = model.Category                
                };

                var Blog = _mapper.Map<BlogModel>(BlogModel);
                _repo.Create(Blog);
                _repo.Save();

                _notyf.Success("Blog Submitted Succesfully");
                return RedirectToAction("Create");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Something went wrong");
                _notyf.Error("Something went wrong!", 20);
                return View(model);
            }
        }

        [Authorize]
        private string UploadedBlogImage(CreateBlogVM model)
        {
            string uniqueFileName = null;

            //Check if the Upload file is Image?
            var FileName = model.Image.FileName;

            var allowedExtensions = new[] { ".gif", ".png", ".jpg", ".jpeg", ".webp", ".svg" };
            var extension = Path.GetExtension(FileName);
            if (!allowedExtensions.Contains(extension))
            {
                _notyf.Warning(extension.ToUpper() + " File type is not Allowed");
                return null;
            }

            if (model.Image != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images/Blogs");
                //Removing Extra Spaces in the Image Name Using Regular Expression
                var NameWithSpaces = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                uniqueFileName = Regex.Replace(NameWithSpaces, @" ", "");

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Image.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        [Authorize]
        private string EditBlogImage(EditBlogVM model)
        {
            string uniqueFileName = null;


            if (model.Image != null)
            {
                //Check if the Upload file is Image?
                var FileName = model.Image.FileName;

                var allowedExtensions = new[] { ".gif", ".png", ".jpg", ".jpeg", ".webp", ".svg" };
                var extension = Path.GetExtension(FileName);
                if (!allowedExtensions.Contains(extension))
                {
                    _notyf.Warning(extension.ToUpper() + " File type is not Allowed");
                    return null;
                }
            }

            if (model.Image != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images/Blogs");
                //Removing Extra Spaces in the Image Name Using Regular Expression
                var NameWithSpaces = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                uniqueFileName = Regex.Replace(NameWithSpaces, @" ", "");

                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Image.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }


        [Authorize]
        [HttpPost]
        public ActionResult<CommentModel> CreateComment(PostVM PostModel)
        {
            if (PostModel.Blog is null || PostModel.Blog.BlogID == 0)
                return new BadRequestResult();

            var post = _repo.FindbyId(PostModel.Blog.BlogID);

            if (post is null)
                return new NotFoundResult();

            var comment = PostModel.Comment;
            var author = _userManager.GetUserAsync(User).Result;

            comment.Author = author;
            comment.Blog = post;
            comment.CommentedOn = DateTime.Now;

            _repoComment.Create(comment);
            _repoComment.Save();

            _notyf.Success("Comment Added", 2);
            return RedirectToAction("Post", new { id = PostModel.Blog.BlogID });
        }

        /// <summary>
        /// Return a blog post with the given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public IActionResult Post(int id)
        {
            var IdExists = _repo.FindAll().Where(q => q.BlogID == id && q.isApproved == true && q.isPublished == true).Any();

            if (_signInManager.IsSignedIn(User))
            {
                ViewBag.UserName = _userManager.GetUserAsync(User).Result.FirstName + " " + _userManager.GetUserAsync(User).Result.LastName;
                ViewBag.UserId = _userManager.GetUserAsync(User).Result.Id;
            }

            if (IdExists)
            {
                try
                {
                    //Get the blog
                    var BlogPosts = _repo.FindbyId(id);

                    //Get Blog recommendation
                    var AuthorId = BlogPosts.Author.Id;

                    var BlogRecommended = _repo.FindAll().Where(q => q.Author.Id == AuthorId && q.isApproved == true).OrderByDescending(x => Guid.NewGuid()).Take(6).ToList();

                    var Post = new PostVM
                    {
                        Blog = BlogPosts,
                        BlogRecommendation = BlogRecommended
                    };
                    return View(Post);
                }
                catch (Exception e)
                {
                    _notyf.Information(e.Message);
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                _notyf.Information("Record with this ID does not exist");
                _notyf.Warning("Entering ID via URL is not recommended", 12);
                RedirectToAction("Index");
            }
            return View();
        }

        //Return view to admin
        //Shows all the blogs that are submitted by their authors for 
        //admin approval
        [Authorize(Roles = "Administrator")]
        public IActionResult AdminBlogIndex(string searchString, int? page)
        {
            var blogs = GetBlogsBySearchAdmin(searchString, page);

            if (blogs != null)
            {
                ViewBag.TotalBlogs = blogs.Blogs.Count();
                ViewBag.LiveBlogs = blogs.Blogs.Where(q => q.isApproved == true).Count();
                ViewBag.NotApproved = blogs.Blogs.Where(q => q.isApproved == false).Count();
            }
            return View(blogs);


            //Instead of returning just blogs i am using view model to pass additional 
            //information back to view

            //var Blogs = _repo.FindAll();
            //var BlogModel = _mapper.Map<List<BlogVM>>(Blogs);
            //return View(BlogModel);
        }


        //Display News To Admin
        [Authorize(Roles = "Administrator")]
        public IActionResult AdminNewsIndex(string searchString, int? page)
        {
            var blogs = GetNewsBySearchAdmin(searchString, page);

            if (blogs != null)
            {
                ViewBag.TotalBlogs = blogs.Blogs.Count();
                ViewBag.LiveBlogs = blogs.Blogs.Where(q => q.isApproved == true).Count();
                ViewBag.NotApproved = blogs.Blogs.Where(q => q.isApproved == false).Count();
            }
            return View(blogs);
        }


        //View for pending blogs
        [Authorize(Roles = "Administrator")]
        public IActionResult PendingBlogs(string searchString, int? page)
        {
            var blogs = GetBlogsBySearchAdminForUnapproved(searchString, page);

            if (blogs != null)
            {
                ViewBag.TotalBlogs = blogs.Blogs.Count();
            }
            return View(blogs);
        }

        //Pending news
        [Authorize(Roles = "Administrator")]
        public IActionResult PendingNews(string searchString, int? page)
        {
            var blogs = GetNewsBySearchAdminForUnapproved(searchString, page);

            if (blogs != null)
            {
                ViewBag.TotalBlogs = blogs.Blogs.Count();
            }
            return View(blogs);
        }

        /// <summary>
        /// Return all the blogs written by its author
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public IActionResult MyBlogs(string searchString, int? page)
        {
            var blogs = GetBlogsBySearchStudent(searchString, page);
            return View(blogs);
        }


        /// <summary>
        /// Returns all the blogs written by its author, same method as MyBlogs with different
        /// action name
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public IActionResult MyBlogsStudent(string searchString, int? page)
        {
            var blogs = GetBlogsBySearchStudent(searchString, page);
            return View(blogs);
        }



        [Authorize]
        public IActionResult MyPrivateBlogs(string searchString, int? page)
        {
            var blogs = GetBlogsBySearchStudentForPrivateBlogs(searchString, page);
            return View(blogs);
        }

        [Authorize]
        public IActionResult MyPrivateBlogsStudent(string searchString, int? page)
        {
            var blogs = GetBlogsBySearchStudentForPrivateBlogs(searchString, page);
            return View(blogs);
        }

        /// <summary>
        /// Returns all the blogs of the user having the id as passed in parameter
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public IActionResult StudentBlogs(string id)
        {
            if(id != null)
            {
                try
                {
                    var Author = _repo.FindAll().FirstOrDefault(q => q.Author.Id == id).Author;

                    if (Author != null)
                    {
                        var AuthorName = _repo.FindAll().FirstOrDefault(q => q.Author.Id == id).Author.FirstName;

                        if (AuthorName != null)
                        {
                            ViewBag.AuthorName = AuthorName;
                        }
                    }
                }
                catch (Exception)
                {
                    _notyf.Error("This user does not have any blogs");
                    return RedirectToAction("UserDetails", "Dashboard", new { id = id });
                }
                
            }

            var BlogList = _repo.FindAll().Where(q => q.Author.Id == id).ToList();
            var BlogListModel = _mapper.Map<List<BlogVM>>(BlogList);
            return View(BlogListModel);
        }


        /// <summary>
        /// Delete the blog post by id, only works if the blog author is same as the user who called the 
        /// method | for students
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public IActionResult DeleteBlogPost(int id)
        {
            var author = _userManager.GetUserAsync(User).Result;
            var IdExists = _repo.FindAll().Where(q => q.BlogID == id && q.Author == author).Any();
            if (IdExists)
            {

                var post = _repo.FindbyId(id);
                var postCommentsList = _repoComment.FindAll().Where(q => q.Blog.BlogID == id);

                var NumberOfComments = postCommentsList.Count();

                if (post.Author == author)
                {
                    if (NumberOfComments != 0)
                    {
                        var Counter = NumberOfComments;

                        do
                        {
                            var PostComments = _repoComment.FindAll().FirstOrDefault(q => q.Blog.BlogID == id);
                            if (PostComments != null)
                            {
                                var ResultDeleteComment = _repoComment.Delete(PostComments);
                            }
                            Counter--;
                        }
                        while (Counter != 0);
                    }

                    //Delete Image
                    var BlogHeaderImage = _repo.FindAll().FirstOrDefault(c => c.BlogID == id).BlogImagePath;
                    var ImageDel = Path.Combine(_hostEnvironment.WebRootPath, "images/Blogs", BlogHeaderImage);
                    FileInfo file = new FileInfo(ImageDel);
                    if (file != null)
                    {
                        System.IO.File.Delete(ImageDel);
                        file.Delete();
                    }


                    _repo.Delete(post);
                    _repo.Save();

                    _notyf.Information("Blog Deleted Successfully");
                    return RedirectToAction("MyBlogs");
                }
                else
                {
                    _notyf.Error("You are not Authorized to Delete this Post", 20);
                    return RedirectToAction("Index");
                }
            }
            _notyf.Error("Blog Does Not Exist");
            return RedirectToAction("MyBlogs");
        }


        /// <summary>
        /// Delete blog by id, will delete any blog without any restriction | for admin
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        public IActionResult DeleteBlogPostAdmin(int id)
        {
            var IdExists = _repo.FindAll().Where(q => q.BlogID == id).Any();
            if (IdExists)
            {
                var post = _repo.FindbyId(id);
                var postCommentsList = _repoComment.FindAll().Where(q => q.Blog.BlogID == id);

                var NumberOfComments = postCommentsList.Count();

                if (NumberOfComments != 0)
                {
                    var Counter = NumberOfComments;

                    do
                    {
                        var PostComments = _repoComment.FindAll().FirstOrDefault(q => q.Blog.BlogID == id);
                        if (PostComments != null)
                        {
                            var ResultDeleteComment = _repoComment.Delete(PostComments);
                        }
                        Counter--;
                    }
                    while (Counter != 0);
                }

                //Delete Image
                var BlogHeaderImage = _repo.FindAll().FirstOrDefault(c => c.BlogID == id).BlogImagePath;
                var ImageDel = Path.Combine(_hostEnvironment.WebRootPath, "images/Blogs", BlogHeaderImage);
                FileInfo file = new FileInfo(ImageDel);
                if (file != null)
                {
                    System.IO.File.Delete(ImageDel);
                    file.Delete();
                }


                _repo.Delete(post);
                _repo.Save();


                _notyf.Information("Blog Deleted Successfully");
            }
            else
            {
                _notyf.Error("Blog Does not Exist!");
            }

            return RedirectToAction("AdminBlogIndex");
        }

        /// <summary>
        /// Delete comment having id same as passed id,
        /// Students are only allowed to delete their own comment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public IActionResult DeleteComment(int id)
        {
            var author = _userManager.GetUserAsync(User).Result;
            var IdExists = _repoComment.FindAll().Where(q => q.Id == id && q.Author == author).Any();
            var comment = _repoComment.FindbyId(id);

            if (IdExists)
            {
                _repoComment.Delete(comment);
                _repoComment.Save();
                _notyf.Information("Comment Deleted", 2);

                return RedirectToAction("Post", new { id = comment.Blog.BlogID });
            }
            else
            {
                _notyf.Error("You are not allowed to perform this action", 2);
                return RedirectToAction("Post", new { id = comment.Blog.BlogID });
            }
        }

        /// <summary>
        /// Delete comment, Admin can delete any comment without any restriction
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        public IActionResult DeleteCommentAdmin(int id)
        {
            try
            { 
                var IdExists = _repoComment.FindAll().Where(q => q.Id == id).Any();
                var comment = _repoComment.FindbyId(id);

                if (IdExists)
                {
                    _repoComment.Delete(comment);
                    _repoComment.Save();
                    _notyf.Information("Comment Deleted", 2);

                    return RedirectToAction("Post", new { id = comment.Blog.BlogID });
                }
                else
                {
                    _notyf.Error("Something Went Wrong", 5);
                    return RedirectToAction("Post", new { id = comment.Blog.BlogID });
                }
            }
            catch (Exception e)
            {

                _notyf.Error(e.Message);
                return RedirectToAction("Index", "Home");
            }
        }


        //For Admin
        [Authorize(Roles = "Administrator")]
        public IActionResult EditBlog(int id)
        {
            try
            { 
                var IdExists = _repo.FindAll().Where(q => q.BlogID == id && q.isPublished == true).Any();

                if (IdExists)
                {
                    if (_signInManager.IsSignedIn(User))
                    {
                        ViewBag.User = _userManager.GetUserAsync(User).Result;
                        ViewBag.TotalComments = _repoComment.FindAll().Where(q => q.Blog.BlogID == id).ToList().Count();
                    }

                    var Blogs = _repo.FindbyId(id);
                    var model = _mapper.Map<EditBlogVM>(Blogs);
                    return View(model);
                }
                else
                {
                    _notyf.Information("Record with this ID does not exist");
                    return RedirectToAction("AdminBlogIndex");
                }
            }
            catch (Exception e)
            {

                _notyf.Error(e.Message);
                return RedirectToAction("Index", "Home");
            }
        }


        /// <summary>
        /// Admin are allowed to edit blogs that are submitted by their authors for approval to
        /// admin
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult EditBlog(EditBlogVM model, int id)
        {
            try
            {
                var IdExists = _repo.FindAll().Where(q => q.BlogID == id && q.isPublished == true).Any();

                if (IdExists)
                {

                    string uniqueFileName = EditBlogImage(model);

                    //var BlogHeader = _repo.FindAll().Where(q => q.BlogID == id).First();
                    var BlogHeader = _repo.FindbyId(id);

                    if (uniqueFileName != null)
                    {
                        //delete from root
                        if (BlogHeader.BlogImagePath != null)
                        {
                            var ImageDel = Path.Combine(_hostEnvironment.WebRootPath, "images/Blogs", BlogHeader.BlogImagePath);
                            FileInfo file = new FileInfo(ImageDel);
                            if (file != null)
                            {
                                System.IO.File.Delete(ImageDel);
                                file.Delete();
                                BlogHeader.BlogImagePath = null;
                            }
                        }
                    }

                    //if (uniqueFileName == null)
                    //{
                    //    _notyf.Error("Please Upload an Image File Only ", 7);
                    //    return RedirectToAction("EditBlog");
                    //}

                    if (!ModelState.IsValid)
                    {
                        _notyf.Information("Something Went Wrong!!");
                        return RedirectToAction("EditBlog");
                    }

                    var Editor = _userManager.GetUserAsync(User).Result;

                    BlogHeader.Title = model.Title;
                    BlogHeader.Content = model.Content;
                    BlogHeader.UpdatedOn = DateTime.Now;
                    BlogHeader.EditedBy = Editor.FirstName;
                    BlogHeader.Excerpt = model.Excerpt;
                    BlogHeader.Category = model.Category;

                    //Only let Administrator Approve the Blog
                    if (User.IsInRole("Administrator"))
                    {
                        BlogHeader.isApproved = model.isApproved;
                    }


                    if (uniqueFileName != null)
                    {
                        BlogHeader.BlogImagePath = uniqueFileName;
                    }

                    var isSuccess = _repo.Update(BlogHeader);
                    _repo.Save();

                    _notyf.Success("Blog Updated Succesfully");
                    return RedirectToAction("EditBlog");
                }
                else
                {
                    _notyf.Information("Record with this ID does not exist");
                    return RedirectToAction("AdminBlogIndex");
                }

            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Something went wrong");
                _notyf.Warning(e.Message, 20);
                return View(model);
            }
        }


        //For Students
        //Return view for editing blogs
        [Authorize]
        public IActionResult Edit(int id)
        {
            try
            { 
                var Editor = _userManager.GetUserAsync(User).Result;
                var IdExists = _repo.FindAll().Where(q => q.BlogID == id && q.Author == Editor).Any();

                if (IdExists)
                {
                    if (_signInManager.IsSignedIn(User))
                    {
                        ViewBag.User = _userManager.GetUserAsync(User).Result;
                        ViewBag.TotalComments = _repoComment.FindAll().Where(q => q.Blog.BlogID == id).ToList().Count();
                    }

                    var Blogs = _repo.FindbyId(id);
                    var model = _mapper.Map<EditBlogVM>(Blogs);
                    return View(model);
                }
                else
                {
                    _notyf.Information("Record with this ID does not exist");
                    return RedirectToAction("MyBlogs");
                }
            }
            catch (Exception e)
            {

                _notyf.Error(e.Message);
                return RedirectToAction("Index", "Home");
            }
        }


        /// <summary>
        /// Allows logged in user to edit their blogs | primarly for students
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult Edit(EditBlogVM model, int id)
        {
            try
            {

                string uniqueFileName = EditBlogImage(model);

                //var BlogHeader = _repo.FindAll().Where(q => q.BlogID == id).First();
                var Blog = _repo.FindbyId(id);

                if (uniqueFileName != null)
                {
                    //delete from root
                    if (Blog.BlogImagePath != null)
                    {
                        var ImageDel = Path.Combine(_hostEnvironment.WebRootPath, "images/Blogs", Blog.BlogImagePath);
                        FileInfo file = new FileInfo(ImageDel);
                        if (file != null)
                        {
                            System.IO.File.Delete(ImageDel);
                            file.Delete();
                            Blog.BlogImagePath = null;
                        }
                    }
                }

                //if (uniqueFileName == null)
                //{
                //    _notyf.Error("Please Upload an Image File Only ", 7);
                //    return RedirectToAction("EditBlog");
                //}

                if (!ModelState.IsValid)
                {
                    _notyf.Information("Something Went Wrong!!");
                    return RedirectToAction("Edit");
                }

                var Editor = _userManager.GetUserAsync(User).Result;

                Blog.Title = model.Title;
                Blog.Content = model.Content;
                Blog.isPublished = model.isPublished;
                Blog.UpdatedOn = DateTime.Now;
                Blog.EditedBy = Editor.FirstName;
                Blog.Excerpt = model.Excerpt;
                Blog.Category = model.Category;

                if (Blog.isApproved == true)
                {
                    Blog.isPublished = model.isPublished;
                    Blog.isApproved = false;
                }
                else
                {
                    Blog.isPublished = model.isPublished;
                }

                if (uniqueFileName != null)
                {
                    Blog.BlogImagePath = uniqueFileName;
                }

                var isSuccess = _repo.Update(Blog);
                _repo.Save();

                _notyf.Success("Blog Updated Succesfully");
                return RedirectToAction("Edit");

            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Something went wrong");
                _notyf.Warning(e.Message, 20);
                return View(model);
            }
        }
        

        /// <summary>
        /// Returns comments list of the current logged in user
        /// </summary>
        /// <returns></returns>
        public IActionResult MyComments()
        {
            try
            { 
                var Student = _userManager.GetUserAsync(User).Result;
                var StudentId = Student.Id;

                var CommentList = _repoComment.FindAll().Where(q => q.Author.Id == StudentId).ToList();
                var CommentListModel = _mapper.Map<List<CommentVM>>(CommentList);
                return View(CommentListModel);
            }
            catch (Exception e)
            {

                _notyf.Error(e.Message);
                return RedirectToAction("Index", "Home");
            }
        }


        /// <summary>
        /// Deletes comment from the list using id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public IActionResult DeleteCommentFromList(int id)
        {
            try 
            { 
                var author = _userManager.GetUserAsync(User).Result;
                var IdExists = _repoComment.FindAll().Where(q => q.Id == id && q.Author == author).Any();
                var comment = _repoComment.FindbyId(id);

                ViewBag.FullName = author.FirstName + " " + author.LastName;

                if (IdExists)
                {
                    _repoComment.Delete(comment);
                    _repoComment.Save();
                    _notyf.Information("Comment Deleted", 2);

                    return RedirectToAction("MyComments");
                }
                else
                {
                    _notyf.Error("Something Went Wrong!", 2);
                    return RedirectToAction("MyComments");
                }
            }
            catch (Exception e)
            {

                _notyf.Error(e.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        //Upload Ck Editor 5 Image

        public IActionResult UploadCkEditorImage(List<IFormFile> files)
        {
            var filepath = "";

            //Get Id of user who called the method
            var userId = _userManager.GetUserAsync(User).Result.Id;

            foreach(IFormFile photo in Request.Form.Files)
            {
                string serverMapPath = Path.Combine(_hostEnvironment.WebRootPath, "images/Blogs/CKEditorImages");

                //Removing Extra Spaces in the Image Name Using Regular Expression
                var NameWithSpaces = Guid.NewGuid().ToString() + "_" + photo.FileName;
                var uniqueFileName = Regex.Replace(NameWithSpaces, @" ", "");

                string uniquepath = Path.Combine(serverMapPath, uniqueFileName);

                using (var stream = new FileStream(uniquepath, FileMode.Create))
                {
                    photo.CopyTo(stream);
                }

                //Change this path in production with the correct domain
                filepath = "https://localhost:44353/" + "images/Blogs/CKEditorImages/" + uniqueFileName;

                //Keep Record of Image Uploaded
                //I have written a method(MyCkEditorImages) where i can keep track of what images are active in the blogs
                //and what images are removed from the blogs but are still on server

                var CkEditorRecord = new CkEditorImagesVM
                {
                    CkEditorAuthorId = userId,
                    CkEditorImagePath = filepath
                };

                var EditorImages = _mapper.Map<CkEditorImagesModel>(CkEditorRecord);
                _repoCkEditor.Create(EditorImages);
                _repoCkEditor.Save();
            }

            return Json(new { url = filepath });
        }

        [Authorize]
        public IActionResult MyBlogEditorImages()
        {
            try 
            { 
                var Student = _userManager.GetUserAsync(User).Result;
                var StudentId = Student.Id;

                //GetPath of the images uploaded by this user
                var CkImagePath = _repoCkEditor.FindAll().Where(c => c.CkEditorAuthorId == StudentId).Select(q => q.CkEditorImagePath);

                //Check Contents of user blogs
                var BlogContent = _repo.FindAll().Where(q => q.Author.Id == StudentId).Select(q => q.Content);

                //HOW THIS IS WORKING?
                /*We have enumrable of paths of all ckeditor images, we ran foreach loop on this enumrable
                 take first path and the run another foreach on enuramble of blogcontent of the student's blogs
            
                Now we check if the path of the images is in the content of the blog. 
                > Get the tuple with that path.
                > Set its status as Inactive.
                > Check if it is being used in some blog.
                >>IF YES
                > Set status to Active.
                >>ENDIF
                > Otherwise its status will remain as Inactive.
                 */

            
                foreach (var path in CkImagePath)
                {
                    var GetEntry = _repoCkEditor.FindAll().FirstOrDefault(q => q.CkEditorAuthorId == StudentId && q.CkEditorImagePath == path);
                    GetEntry.IsActive = false;

                    foreach (var content in BlogContent)
                    {

                        if (content.Contains(path))
                        {
                            GetEntry.IsActive = true;
               
                            _repoCkEditor.Update(GetEntry);

                        }
                        //else
                        //{
                        //    //var GetEntry = _repoCkEditor.FindAll().FirstOrDefault(q => q.CkEditorAuthorId == StudentId && q.CkEditorImagePath == path);
                        //    GetEntry.IsActive = false;

                        //    _repoCkEditor.Update(GetEntry);
                        //}
                    }
                }

                _repoCkEditor.Save();

                var CkEditorImages = _repoCkEditor.FindAll().Where(q => q.CkEditorAuthorId == StudentId);

                var CkEditorImagesModel = _mapper.Map<List<CkEditorImagesVM>>(CkEditorImages);

                return View(CkEditorImagesModel);
            }
            catch (Exception e)
            {

                _notyf.Error(e.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult DeleteCkEditorImage(int id)
        {
            try { 
                var image = _repoCkEditor.FindbyId(id);

                if (image == null)
                {
                    _notyf.Error("Image not found");
                    return RedirectToAction("Index", "Dashboard");
                    //return NotFound();
                }

                _repoCkEditor.Delete(image);

                //delete from root
                var CkEditorImageName = image.CkEditorImagePath.Replace("https://localhost:44353/images/Blogs/CKEditorImages/", "").Replace("  ", " ");
                var ImageDel = Path.Combine(_hostEnvironment.WebRootPath, "images/Blogs/CkEditorImages", CkEditorImageName);
                FileInfo file = new FileInfo(ImageDel);
                if (file != null)
                {
                    System.IO.File.Delete(ImageDel);
                    file.Delete();
                }

                _repoCkEditor.Save();

                _notyf.Success("Image Deleted Successfully");
                return RedirectToAction("MyBlogEditorImages");
            }
            catch (Exception e)
            {

                _notyf.Error(e.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        [AllowAnonymous]
        public IActionResult RecommendedBlogs(string searchString, int? page, string id)
        {
            try
            {
                var blogs = GetBlogsBySearchIndexRecommendation(searchString, page, id);

                if(blogs.Blogs.Count() == 0)
                {
                    _notyf.Error("This user does not have any blogs!!!");
                    return RedirectToAction("Index", "Home");
                }

                var authorName = _repo.FindAll().Where(q => q.Author.Id == id).First().Author.FirstName;

                ViewBag.authorName = authorName;

                return View(blogs);
            }
            catch (Exception e)
            {

                _notyf.Error(e.Message);
                return RedirectToAction("Index","Home");
            }
        }

        [AllowAnonymous]
        public IActionResult News(string searchString, int? page)
        {
            try
            {
                var news = GetNewsBySearchIndex(searchString, page);
                return View(news);
            }
            catch (Exception e)
            {

                _notyf.Error(e.Message);
                return RedirectToAction("Index", "Home");
            }
            
        }

    }
}

