using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GCUSMS.Data;

namespace GCUSMS.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class GalleryListController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly INotyfService _notyf;

        public GalleryListController(ApplicationDbContext db, IMapper mapper, IWebHostEnvironment hostEnvironment, INotyfService notyf)
        {
            _db = db;
            _mapper = mapper;
            _hostEnvironment = hostEnvironment;
            _notyf = notyf;
        }
        // GET: GalleryListController
        public ActionResult Index()
        {
            var images = _db.Images.ToList();
            return View(images);
        }

        [HttpGet]
        public async Task<IActionResult> Index(string DptName, int yearName)
        {
            ViewData["GetImages"] = DptName;
            if(yearName > 1800)
            {
                ViewData["GetYear"] = yearName;
            }
            else
            {
                ViewData["GetYear"] = "";
            }
            //ViewData["GetYear"] = yearName;

            var imgQuery = from x in _db.Images select x;

            if (!String.IsNullOrEmpty(DptName) || yearName >= 1800)
            {
                imgQuery = imgQuery.Where(x => x.DepartmentName.Contains(DptName) || x.UploadedOn.Year == yearName);
            }

            if (!String.IsNullOrEmpty(DptName) && yearName >= 1800)
            {
                imgQuery = imgQuery.Where(x => x.DepartmentName.Contains(DptName) && x.UploadedOn.Year == yearName);
            }

            return View(await imgQuery.AsNoTracking().ToListAsync());
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _db.Images.FindAsync(id);
            if (image == null)
            {
                return NotFound();
            }

            _db.Images.Remove(image);
            //delete from root
            var ImageDel = Path.Combine(_hostEnvironment.WebRootPath, "images", image.ImagePath);
            FileInfo file = new FileInfo(ImageDel);
            if (file != null)
            {
                System.IO.File.Delete(ImageDel);
                file.Delete();
            }
            await _db.SaveChangesAsync();
            _notyf.Success("Image Deleted Successfully");
            return RedirectToAction("Index");
        }
    }
}
