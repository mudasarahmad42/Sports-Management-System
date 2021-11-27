using Microsoft.AspNetCore.Http;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GCUSMS.Models;

namespace GCUSMS.ViewModels
{
    public class BlogVM
    {
        [Key]
        public int BlogID { get; set; }
        public StudentModel Author { get; set; }
        [Required]
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool isPublished { get; set; } = false;
        public bool isApproved { get; set; } = false;
        public string EditedBy { get; set; }
        public string Excerpt { get; set; }
        public string Category { get; set; }
        public string BlogImagePath { get; set; }
        public virtual IEnumerable<CommentModel> Comments { get; set; }
    }

    public class PostVM
    {
        public BlogModel Blog { get; set; }
        public List<BlogModel> BlogRecommendation { get; set; }
        public CommentModel Comment { get; set; }
    }

    public class CreateBlogVM
    {
        [Key]
        public int BlogID { get; set; }
        public StudentModel Author { get; set; }
        [Required]
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool isPublished { get; set; } = false;
        public bool isApproved { get; set; } = false;
        public string EditedBy { get; set; }
        public string Excerpt { get; set; }
        public string Category { get; set; }
        public string BlogImagePath { get; set; }
        public IFormFile Image { get; set; }
    }

    public class EditBlogVM
    {
        [Key]
        public int BlogID { get; set; }
        public StudentModel Author { get; set; }
        [Required]
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool isPublished { get; set; } = false;
        public bool isApproved { get; set; } = false;
        public string EditedBy { get; set; }
        public string Excerpt { get; set; }
        public string Category { get; set; }
        public string BlogImagePath { get; set; }
        public IFormFile Image { get; set; }
    }

    public class SearchBlogVM
    {
        public IPagedList<BlogModel> Blogs { get; set; }
        public string SearchString { get; set; }
        public int PageNumber { get; set; }
    }
}
