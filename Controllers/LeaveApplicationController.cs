using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GCUSMS.Contracts;
using GCUSMS.Models;
using GCUSMS.ViewModels;

namespace GCUSMS.Controllers
{
    [Authorize]
    public class LeaveApplicationController : Controller
    {
        private readonly ILeaveApplicationRepository _repo;
        private readonly IMapper _mapper;
        private readonly UserManager<StudentModel> _userManager;
        private readonly SignInManager<StudentModel> _signInManager;
        private readonly INotyfService _notyf;

        public LeaveApplicationController
        (
            ILeaveApplicationRepository repo,
            IMapper mapper,
            UserManager<StudentModel> userManager,
            SignInManager<StudentModel> signInManager,
            INotyfService notyf
        )
        {
            _repo = repo;
            _mapper = mapper;
           _userManager = userManager;
            _signInManager = signInManager;
            _notyf = notyf;
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Index()
        {
            var leaveApplications = _repo.FindAll().OrderByDescending(x => x.DateRequested);

            var leaveApplicationsModel = _mapper.Map<List< LeaveApplicationVM >>(leaveApplications);
            var model = new AdminLeaveApplicationVM
            {
                TotalRequests = leaveApplicationsModel.Count(),
                ApprovedRequests = leaveApplications.Count(q => q.Approved == true),
                RejectedRequests = leaveApplications.Count(q => q.Approved == false),
                PendingRequests = leaveApplications.Count(q => q.Approved == null),
                LeaveApplications = leaveApplicationsModel
            };
            return View(model);
        }

        // GET: LeaveRequest/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LeaveRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateLeaveApplicationsVM model)
        {

            try
            {
                var startDate = model.StartDate;
                var endDate = model.EndDate;

                var student = _userManager.GetUserAsync(User).Result;

                var StudentApplications = _repo.GetLeaveRequestsByStudentID(student.Id).Where(q => q.Approved == null).Any();

                if (StudentApplications)
                {
                    _notyf.Information("You can not Submit Another Application");
                    _notyf.Information("Your Previous Application is already pending!! Visit Department office in case of any issue!",10);
                    return RedirectToAction("Create");
                }

                int daysRequested = (int)(endDate - startDate).TotalDays;

                if(startDate < DateTime.Today)
                {
                    _notyf.Warning("Enter a Valid start Date");
                    return RedirectToAction("Create");
                }

                if (daysRequested > 15)
                {
                    _notyf.Warning("You can not submit online application for leave of more than 15 days,");
                    _notyf.Information("Please Visit Department Office for further assistance");
                    ModelState.AddModelError("", "Start Date cannot be further in the future than the End Date");
                    return RedirectToAction("Create");
                }

                if (daysRequested < 0)
                {
                    _notyf.Warning("Start Date should be less than Ending Date");
                    ModelState.AddModelError("", "Start Date cannot be further in the future than the End Date");
                    return RedirectToAction("Create");
                }




                if (!ModelState.IsValid)
                {
                    _notyf.Warning("Something Went Wrong!!");
                    return RedirectToAction("Create");
                }

                var leaveApplicationModel = new LeaveApplicationVM
                {
                    RequestingStudentId = student.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    //DateActioned = DateTime.Now,
                    Message = model.Message,
                    LeaveSubject = model.LeaveSubject
                };


                var leaveapplication = _mapper.Map<LeaveApplicationModel>(leaveApplicationModel);
                _repo.Create(leaveapplication);
                _repo.Save();

                _notyf.Success("Application Submitted Succesfully");
                return RedirectToAction("Create");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Details(int id)
        {
            var IdExists = _repo.FindAll().Where(q => q.Id == id).Any();

            if (IdExists)
            {

                var leaveRequest = _repo.FindbyId(id);
                var model = _mapper.Map<LeaveApplicationVM>(leaveRequest);

                var StudentId = leaveRequest.RequestingStudentId;
                var LoggedInUser = _userManager.GetUserAsync(User).Result.Id;

                ViewBag.CheckId = LoggedInUser;

                ViewBag.TotalRequests = _repo.GetLeaveRequestsByStudentID(StudentId).Count();
                ViewBag.TotalApprovedRequests = _repo.GetLeaveRequestsByStudentID(StudentId).Where(q => q.Approved == true).Count();
                ViewBag.TotalRejectedRequests = _repo.GetLeaveRequestsByStudentID(StudentId).Where(q => q.Approved == false).Count();
                ViewBag.TotalPendingRequests = _repo.GetLeaveRequestsByStudentID(StudentId).Where(q => q.Approved == null).Count();


                return View(model);
            }
            else
            {
                _notyf.Information("Record with this ID does not exist");
                _notyf.Warning("Entering ID via URL is not recommended", 12);
                return View();
            }
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult ApproveRequest(int id)
        {
            var IdExists = _repo.FindAll().Where(q => q.Id == id).Any();

            if (IdExists)
            {
                try
                {
                    var user = _userManager.GetUserAsync(User).Result;
                    var leaveApplications = _repo.FindbyId(id);
                    leaveApplications.Approved = true;
                    leaveApplications.DateActioned = DateTime.Now;

                    var isSuccess = _repo.Update(leaveApplications);
                    _notyf.Success("Leave Application is Approved!");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                _notyf.Information("Record with this ID does not exist");
                _notyf.Warning("Entering ID via URL is not recommended", 12);
                return View();
            }
        }


        [Authorize(Roles = "Administrator")]
        public IActionResult RejectRequest(int id)
        {
            try
            {
                var user = _userManager.GetUserAsync(User).Result;

                var leaveApplications = _repo.FindbyId(id);
                leaveApplications.Approved = false;
                leaveApplications.DateActioned = DateTime.Now;

                var isSuccess = _repo.Update(leaveApplications);
                _notyf.Warning("Leave Application is Rejected");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult MyLeaves()
        {
            var Student = _userManager.GetUserAsync(User).Result;
            var StudentId = Student.Id;

            var leaveApplications = _repo.FindAll().Where(q => q.RequestingStudentId == StudentId);

            var leaveApplicationsModel = _mapper.Map<List<LeaveApplicationVM>>(leaveApplications);

            var model = new StudentLeaveApplicationVM
            {
                TotalRequests = leaveApplicationsModel.Count(),
                ApprovedRequests = leaveApplications.Count(q => q.Approved == true),
                RejectedRequests = leaveApplications.Count(q => q.Approved == false),
                PendingRequests = leaveApplications.Count(q => q.Approved == null),
                LeaveApplications = leaveApplicationsModel,
                RequestingStudentId = StudentId,
                RequestingStudent = Student

            };
            return View(model);
        }


        public IActionResult StudentLeaves(string id)
        {
            var Student = _userManager.FindByIdAsync(id).Result;
            var StudentId = Student.Id;

            var leaveApplications = _repo.FindAll().Where(q => q.RequestingStudentId == id);

            var leaveApplicationsModel = _mapper.Map<List<LeaveApplicationVM>>(leaveApplications);

            var model = new StudentLeaveApplicationVM
            {
                TotalRequests = leaveApplicationsModel.Count(),
                ApprovedRequests = leaveApplications.Count(q => q.Approved == true),
                RejectedRequests = leaveApplications.Count(q => q.Approved == false),
                PendingRequests = leaveApplications.Count(q => q.Approved == null),
                LeaveApplications = leaveApplicationsModel,
                RequestingStudent = Student,
                RequestingStudentId = StudentId
            };
            return View(model);
        }

        public IActionResult StudentLeaveDetails(int id)
        {
            var IdExists = _repo.FindAll().Where(q => q.Id == id).Any();

            if (IdExists)
            {
                var leaveRequest = _repo.FindbyId(id);
            var model = _mapper.Map<LeaveApplicationVM>(leaveRequest);

            if (id != 0)
            {
                var StudentId = leaveRequest.RequestingStudentId;



                ViewBag.TotalRequests = _repo.GetLeaveRequestsByStudentID(StudentId).Count();
                ViewBag.TotalApprovedRequests = _repo.GetLeaveRequestsByStudentID(StudentId).Where(q => q.Approved == true).Count();
                ViewBag.TotalRejectedRequests = _repo.GetLeaveRequestsByStudentID(StudentId).Where(q => q.Approved == false).Count();
                ViewBag.TotalPendingRequests = _repo.GetLeaveRequestsByStudentID(StudentId).Where(q => q.Approved == null).Count();
            }

            return View(model);
            }
            else
            {
                _notyf.Information("Record with this ID does not exist");
                _notyf.Warning("Entering ID via URL is not recommended", 12);
                return View();
            }
        }


        public IActionResult DeleteLeave(int id)
        {
            var StudentID = _userManager.GetUserAsync(User).Result.Id;

            var IdExists = _repo.FindAll().Where(q => q.Id == id && q.RequestingStudentId == StudentID).Any();

            if (IdExists)
            {
                var leaveAllocations = _repo.FindAll().FirstOrDefault(c => c.Id == id && c.RequestingStudentId == StudentID);
                if (leaveAllocations != null)
                {
                    var resultLeave = _repo.Delete(leaveAllocations);
                }

                _notyf.Success("Leave Application Deleted Succesfully");

                return RedirectToAction("MyLeaves");
            }
            else
            {
                _notyf.Information("Record with this ID does not exist");
                _notyf.Warning("Entering ID via URL is not recommended", 12);
                return View();
            }
        }
    }

}
