using GCUSMS.Models;
using Microsoft.AspNetCore.Http;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.ViewModels
{
    public class GalleryVM
    {
        [Key]
        public int ImageId { get; set; }
        [Required]
        public string ImageName { get; set; }     
        [Required]
        public DateTime UploadedOn { get; set; }
        public string DepartmentName { get; set; }
        public string Tournament { get; set; }
        public string ImagePath { get; set; }
        public string Description { get; set; }
        //Added for Index Page
        public List<BlogVM> Blogs { get; set; }
        public List<GalleryVM> Images { get; set; }
        [Required]
        public IFormFile Image { get; set; }
    }

    public class SearchGalleryVM
    {
        public IPagedList<GalleryModel> ImagesForPagination { get; set; }
        public string SearchString { get; set; }
        public int? SearchYear { get; set; }
        public int PageNumber { get; set; }
    }
}