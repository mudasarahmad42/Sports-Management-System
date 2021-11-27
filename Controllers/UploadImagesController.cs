using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GCUSMS.Data;
using GCUSMS.Models;
using GCUSMS.ViewModels;

namespace GCUSMS.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UploadImagesController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ApplicationDbContext _options;
        private readonly INotyfService _notyf;

        public UploadImagesController(IWebHostEnvironment hostEnvironment, ApplicationDbContext options, INotyfService notyf)
        {
            _hostEnvironment = hostEnvironment;
            _options = options;
            _notyf = notyf;
        }
        // GET: UploadImagesController
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImages(GalleryVM model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = UploadedFile(model);

                if (uniqueFileName == null)
                {
                    _notyf.Error("Please Upload an Image File Only " , 7);
                    return RedirectToAction("Index");
                }

                GalleryModel gallery = new GalleryModel
                {
                    ImageName = model.ImageName,
                    //UploadedOn = model.UploadedOn,
                    UploadedOn = DateTime.Now,
                    Description = model.Description,
                    DepartmentName = model.DepartmentName,
                    Tournament = model.Tournament,
                    ImagePath = uniqueFileName,

                };
                _options.Add(gallery);
                await _options.SaveChangesAsync();
                _notyf.Success("Image Uploaded Succesfully");
                return RedirectToAction("Index");
            }
            
            return View();
        }

        private string UploadedFile(GalleryVM model)
        {
            string uniqueFileName = null;

            //Check if the Upload file is Image?
            var FileName = model.Image.FileName;

            var allowedExtensions = new[] {".jpg", ".jpeg" };
            var extension = Path.GetExtension(FileName);
            if (!allowedExtensions.Contains(extension))
            {
                _notyf.Warning(extension.ToUpper() + " File types are not Allowed");
                return null;
            }

            if (model.Image != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Image.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
    }
}
