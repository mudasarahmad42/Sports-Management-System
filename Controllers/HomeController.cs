using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GCUSMS.Data;
using GCUSMS.Models;
using GCUSMS.ViewModels;
using AspNetCoreHero.ToastNotification.Abstractions;
using GCUSMS.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace GCUSMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IBlogRepository _repo;
        private readonly IFeedbackRepository _repoFeedback;
        private readonly IMapper _mapper;
        private readonly INotyfService _notyf;

        public HomeController(ILogger<HomeController> logger,
            ApplicationDbContext db, 
            IBlogRepository repo,
            IFeedbackRepository repoFeedback,
            IMapper mapper,
            INotyfService notyf)
        {
            _logger = logger;
            _db = db;
            _repo = repo;
            _repoFeedback = repoFeedback;
            _mapper = mapper;
            _notyf = notyf;
        }

        public IActionResult Index()
        {
            //Get Images from Gallery
            var GalleryImages = _db.Images.ToList();

            var ImageModel = GalleryImages.OrderByDescending(x => Guid.NewGuid()).Take(9).ToList();

            var Gallerymodel = _mapper.Map<List<GalleryModel>, List<GalleryVM>>(ImageModel);


            //Get Blogs
            var Blogs = _repo.FindAll().Where(q => q.isApproved == true && q.Category != "News");

            var ListOfBlogs = Blogs.OrderByDescending(x => Guid.NewGuid()).Take(9).ToList();

            var BlogModel = _mapper.Map<List<BlogModel>, List<BlogVM>>(ListOfBlogs);

            var model = new HomeIndexVM
            {
                Gallery = Gallerymodel,
                Blog = BlogModel
            };

            
            return View(model);
        }

        public IActionResult SubmitFeedback(HomeIndexVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var FeedbackModel = new FeedbackVM
                    {
                        Name = model.Feedback.Name,
                        Email = model.Feedback.Email,
                        FeedbackMessage = model.Feedback.FeedbackMessage,
                        DateSubmitted = DateTime.Now
                    };

                    var Feedback = _mapper.Map<FeedbackModel>(FeedbackModel);
                    _repoFeedback.Create(Feedback);
                    _repoFeedback.Save();

                    _notyf.Success("Thank You for your feedback");
                    return RedirectToAction("Index");
                }
                else
                {
                    _notyf.Error("Something Went Wrong!");
                    return RedirectToAction("Index");
                }
            }
            catch (Exception e)
            {

                _notyf.Error(e.Message);
                return RedirectToAction("Index");
            }
        }

        public IActionResult DeleteFeedback(int id)
        {
            try
            {
                var IdExists = _repoFeedback.FindAll().Where(q => q.FeedbackID == id).Any();

                if (IdExists)
                {
                    var Feedback = _repoFeedback.FindAll().FirstOrDefault(c => c.FeedbackID == id);
                   
                    if (Feedback != null)
                    {
                        var DeleteFeedback = _repoFeedback.Delete(Feedback);
                    }

                    _notyf.Success("Feedback Deleted");

                    return RedirectToAction("Feedback");
                }
                else
                {
                    _notyf.Information("Record with this ID does not exist");
                    _notyf.Warning("Entering ID via URL is not recommended", 12);
                    return View();
                }
            }
            catch (Exception e)
            {

                _notyf.Error(e.Message);
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Feedback()
        {
            try
            {
                var Feedback = _repoFeedback.FindAll().OrderByDescending(x => x.DateSubmitted);

                var FeedbackVM = _mapper.Map<List<FeedbackVM>>(Feedback);

                return View(FeedbackVM);
            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("Index");
            }
            
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult FeedbackDetails(int id)
        {
            try
            {
                var IdExists = _repoFeedback.FindAll().Where(q => q.FeedbackID == id).Any();

                if (IdExists)
                {

                    var Feedback = _repoFeedback.FindbyId(id);
                    var model = _mapper.Map<FeedbackVM>(Feedback);

                    return View(model);
                }
                else
                {
                    _notyf.Information("Record with this ID does not exist");
                    _notyf.Warning("Entering ID via URL is not recommended", 12);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception e)
            {

                _notyf.Error(e.Message);
                return RedirectToAction("Index");
            }
            
        }

        public IActionResult AboutMe()
        {
            return View();
        }

        //Not Using It anymore
        //public IActionResult FAQs()
        //{ 
        //    return View(); 
        //}

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
            
    }
}
