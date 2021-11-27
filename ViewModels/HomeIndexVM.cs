using GCUSMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.ViewModels
{
    public class HomeIndexVM
    {
        public List<GalleryVM> Gallery { get; set; }
        public List<BlogVM> Blog { get; set; }
        public FeedbackVM Feedback { get; set; }
    }
}
