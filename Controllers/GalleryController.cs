using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GCUSMS.Data;
using GCUSMS.Models;
using GCUSMS.ViewModels;
using PagedList.Core;
using Microsoft.AspNetCore.Authorization;

namespace GCUSMS.Controllers
{
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public GalleryController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // GET: GalleryController
        //public ActionResult Index()
        //{
        //    var GalleryImages = _db.Images.ToList();

        //    var ImageModel = GalleryImages.OrderByDescending(x => Guid.NewGuid()).ToList();

        //    return View(ImageModel);
        //}

        //[HttpGet]
        //public async Task<IActionResult> Index(string DptName, int yearName)
        //{
        //    ViewData["GetImages"] = DptName;

        //    if (yearName > 1800)
        //    {
        //        ViewData["GetYear"] = yearName;
        //    }
        //    else
        //    {
        //        ViewData["GetYear"] = "";
        //    }
        //    //ViewData["GetYear"] = yearName;

        //    var imgQuery = from x in _db.Images select x;

        //    if (!String.IsNullOrEmpty(DptName) || yearName >= 1800)
        //    {
        //        imgQuery = imgQuery.Where(x => x.DepartmentName.Contains(DptName) || x.UploadedOn.Year == yearName);
        //    }

        //    if (!String.IsNullOrEmpty(DptName) && yearName >= 1800)
        //    {
        //        imgQuery = imgQuery.Where(x => x.DepartmentName.Contains(DptName) && x.UploadedOn.Year == yearName);
        //    }

        //    return View(await imgQuery.AsNoTracking().ToListAsync());
        //}


        // GET: GalleryController
        [Authorize]
        public ActionResult GallerySlideshow()
        {
            var GalleryImages = _db.Images.ToList();
            var GalleryImagesCount = GalleryImages.Count();
            int GalleryImagesCountStart = (int)(GalleryImagesCount * 0.25);
            int GalleryImagesCountEnd = (int)(GalleryImagesCount * 0.5);

           
            Random rnd = new Random();

            var Random = rnd.Next(GalleryImagesCountStart, GalleryImagesCountEnd);
            
            ViewBag.Number = Random;

           var ImageModel = GalleryImages.OrderByDescending(x => Guid.NewGuid()).Take(Random).ToList();


            var model = _mapper.Map<List<GalleryModel>, List<GalleryVM>>(ImageModel);
            return View(model);
        }

        public SearchGalleryVM GetImagesBySearch(string searchString, int? page)
        {
            int pageSize = 15;
            int pageNumber = page ?? 1;
            var SearchImages = _db.Images
                .OrderByDescending(q => q.UploadedOn)
                .Where(q => q.ImageName.Contains(searchString ?? string.Empty) || q.DepartmentName.Contains(searchString ?? string.Empty) || q.Description.Contains(searchString ?? string.Empty) || q.UploadedOn.Year.ToString() == (searchString ?? string.Empty));

            return new SearchGalleryVM
            {
                ImagesForPagination = new StaticPagedList<GalleryModel>(SearchImages.Skip((pageNumber - 1) * pageSize).Take(pageSize), pageNumber, pageSize, SearchImages.Count()),
                SearchString = searchString,
                PageNumber = pageNumber
            };
        }

        //public SearchGalleryVM GetImagesByYear(int? searchYear, int? page)
        //{
        //    int pageSize = 1;
        //    int pageNumber = page ?? 1;
        //    var SearchImages = _db.Images
        //        .OrderByDescending(q => q.UploadedOn)
        //        .Where(q => q.UploadedOn.Year == searchYear);

        //    return new SearchGalleryVM
        //    {
        //        ImagesForPagination = new StaticPagedList<GalleryModel>(SearchImages.Skip((pageNumber - 1) * pageSize).Take(pageSize), pageNumber, pageSize, SearchImages.Count()),
        //        SearchYear = searchYear,
        //        PageNumber = pageNumber
        //    };
        //}


        public IActionResult Index(string searchString, int? page)
        {
            var Images = GetImagesBySearch(searchString, page);

            return View(Images);
        }
    }
}
