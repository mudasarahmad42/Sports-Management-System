using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GCUSMS.Contracts;
using GCUSMS.Models;
using GCUSMS.ViewModels;

namespace GCUSMS.Controllers
{
    public class ParticipantsController : Controller
    {
        private readonly IParticipantRepository _repo;
        private readonly ITournamentRepository _tourna_Repo;
        private readonly ITeamPlayerRepository _teamPlayer_Repo;
        private readonly ITournamentTeamRepository _tournamentTeam_Repo;
        private readonly IMapper _mapper;
        private readonly INotyfService _notyf;
        private readonly UserManager<StudentModel> _userManager;
        private readonly SignInManager<StudentModel> _signInManager;

        public ParticipantsController
        (
            IParticipantRepository repo,
            ITournamentRepository tourna_repo,
            ITeamPlayerRepository teamPlayer_repo,
            ITournamentTeamRepository tournamentTeam_repo,
            IMapper mapper,
            INotyfService notyf,
            UserManager<StudentModel> userManager,
            SignInManager<StudentModel> signInManager
        )
        {
            _repo = repo;
            _tourna_Repo = tourna_repo;
            _teamPlayer_Repo = teamPlayer_repo;
            _tournamentTeam_Repo = tournamentTeam_repo;
            _mapper = mapper;
            _notyf = notyf;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Participate(int id)
        {

            var IdExists = _tourna_Repo.FindAll().Where(q => q.TournamentId == id).Any();


            if (IdExists)
            {
                var Tournament = _tourna_Repo.FindbyId(id);

                var student = _userManager.GetUserAsync(User).Result;

                if (student.DepartmentName != null)
                {
                    ViewBag.StudentDepartment = student.DepartmentName.ToUpper();
                }

                ViewBag.CheckDate = Tournament.DueDate;

                var TournamentIsActive = _tourna_Repo.FindbyId(id).isActive;

                if (TournamentIsActive == false)
                {
                    _notyf.Information("This Tournament does not exist anymore");
                    return RedirectToAction("TournamentAvailable", "Tournaments", new { area = "" });
                }

                var TournamentName = _tourna_Repo.FindbyId(id).TournamentName;
                var TournamentVenue = _tourna_Repo.FindbyId(id).TournamentVenue;

                if (TournamentName != null)
                {
                    ViewBag.TournamentName = TournamentName;
                }

                if (Tournament.isIntraDepartmental == true)
                {
                    ViewBag.IsIntra = Tournament.isIntraDepartmental;
                }

                if (Tournament.DepartmentName != null)
                {
                    ViewBag.DepartmentName = Tournament.DepartmentName.ToUpper();
                }

                if (TournamentVenue != null)
                {
                    ViewBag.TournamentVenue = TournamentVenue;
                }

                if (DateTime.Now > Tournament.DueDate)
                {
                    ViewBag.TournamentDueDate = "Application Submission is Closed";
                }
                else
                {
                    ViewBag.TournamentDueDate = Tournament.DueDate.ToString("dddd, dd MMMM yyyy");
                }

                ////Checking Gender of user

                ////Get Gender Allowed in tournament
                //var GetTournamentGenderAllowed = Tournament.GenderAllowed;


                ////Get user gender
                //var UserGender = _userManager.GetUserAsync(User).Result.Gender;

                ////View Bags
                //if (UserGender != null)
                //{
                //    if (UserGender == "MALE" || UserGender == "NOT TO BE SPECIFIED")
                //    {
                //        var Men = "MEN";
                //    }

                //    if (UserGender == "FEMALE" || UserGender == "NOT TO BE SPECIFIED")
                //    {
                //        var Women = "WOMEN";
                //    }

                //}


                return View();
            }
            else
            {
                _notyf.Information("Record with this ID does not exist");
                _notyf.Warning("Entering ID via URL is not recommended", 12);
                return RedirectToAction("TournamentAvailable", "Tournaments", new { area = "" });
            }


        }


        // POST: LeaveRequest/Participate
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Participate(ParticipantVM model, int id)
        {

            try
            {
                var student = _userManager.GetUserAsync(User).Result;

                var Tournament = _tourna_Repo.FindbyId(id);

                if (!ModelState.IsValid)
                {
                    _notyf.Information("Something Went Wrong!!");
                    return RedirectToAction("Participate");
                }

                var StudentParticipatedBefore = _repo.FindAll().Where(q => q.RequestingStudentId == student.Id && q.AppliedTournamentId == id).Any();

                if (StudentParticipatedBefore)
                {
                    _notyf.Information("You have already submitted your participation form for this tournament!");
                    return RedirectToAction("Participate");
                }

                var ParticipantModel = new ParticipantVM
                {
                    RequestingStudentId = student.Id,
                    AppliedTournamentId = Tournament.TournamentId,
                    DateApplied = DateTime.Now,
                    ValidEmail = model.ValidEmail
                };


                var ParticipantForm = _mapper.Map<ParticipantModel>(ParticipantModel);
                _repo.Create(ParticipantForm);
                _repo.Save();

                _notyf.Success("Form Submitted Successfully");
                return RedirectToAction("Participate");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult ParticipantListAdmin()
        {
            var ParticipantList = _repo.FindAll();
            var ParticipantListModel = _mapper.Map<List<ParticipantVM>>(ParticipantList);
            return View(ParticipantListModel);
        }


        [Authorize(Policy = "MakeCaptain")]
        public IActionResult ParticipantListCaptain()
        {
            var UserDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;

            var ParticipantList = _repo.FindAll().Where(q => q.RequestingStudent.DepartmentName == UserDepartment);
            var ParticipantListModel = _mapper.Map<List<ParticipantVM>>(ParticipantList);
            return View(ParticipantListModel);
        }

        public IActionResult ParticipantDetails(int id)
        {
            var participants = _repo.FindbyId(id);

            if (participants != null)
            {
                var LoggedinUser = _userManager.GetUserAsync(User).Result;
                var isAdmin = _userManager.GetRolesAsync(LoggedinUser).Result;

                if (participants.RequestingStudent.DepartmentName == LoggedinUser.DepartmentName || isAdmin.Contains("Administrator"))
                {
                    var model = _mapper.Map<ParticipantVM>(participants);
                    return View(model);
                }
            }
            else
            {
                _notyf.Warning("User does not exist");
                return RedirectToAction("Index");
            }
            //var model = _mapper.Map<ParticipantVM>(participants);
            //return View(model);

            _notyf.Warning("You are not allowed to perform this action");
            return RedirectToAction("ParticipantListCaptain");
        }


        [Authorize]
        public IActionResult ApproveRequestParticipant(int id)
        {
            try
            {
                var Participant = _repo.FindbyId(id);
                Participant.isSelected = true;
                Participant.DateSelected = DateTime.Now;

                var isSuccess = _repo.Update(Participant);
                _notyf.Success("Participant Has been Selected");
                return RedirectToAction("ParticipantDetails", new { id = id });
            }
            catch (Exception)
            {
                return RedirectToAction("ParticipantDetails", new { id = id });
            }
        }

        [Authorize]
        public IActionResult RejectRequestParticipant(int id)
        {
            try
            {
                var Participant = _repo.FindbyId(id);
                Participant.isSelected = false;
                Participant.DateSelected = DateTime.Now;

                var isSuccess = _repo.Update(Participant);
                _notyf.Warning("Participant has been rejected!");
                return RedirectToAction("ParticipantDetails", new { id = id });
            }
            catch (Exception)
            {
                return RedirectToAction("ParticipantDetails", new { id = id });
            }
        }


        [Authorize]
        public IActionResult UserParticipations()
        {
            var Student = _userManager.GetUserAsync(User).Result;
            var StudentId = Student.Id;

            var participantList = _repo.FindAll().Where(q => q.RequestingStudentId == StudentId);

            var leaveApplicationsModel = _mapper.Map<List<ParticipantVM>>(participantList);

            return View(leaveApplicationsModel);
        }

        [Authorize]
        public IActionResult UserParticipationsDetails(int id, int tournamentId)
        {
            try
            {
                var participants = _repo.FindbyId(id);
                var model = _mapper.Map<ParticipantVM>(participants);

                //Get the team user is part of in this tournament
                var Student = _userManager.GetUserAsync(User).Result;
                var StudentId = Student.Id;

                //Get the list of IDs of all teams currently in tournament's table
                var ListOfTeamIDs = _teamPlayer_Repo.FindAll().Where(q => q.PlayerId == StudentId).Select(q => q.TeamId).Distinct();

                //Now use these id to get all teams in these teams
                var ListOfAllTeams = _tournamentTeam_Repo.FindAll().Where(q => ListOfTeamIDs.Contains(q.TeamId) && q.TournamentId == tournamentId).FirstOrDefault();
               
                if(ListOfAllTeams != null)
                {
                    ViewBag.TeamName = ListOfAllTeams.Team.TeamName;
                }

                return View(model);
            }
            catch (Exception e)
            {

                _notyf.Warning(e.Message);
                return RedirectToAction("MyTournaments","Tournaments");
            }

        }

        [Authorize]
        public IActionResult StudentParticipations(string id)
        {
            var IdExists = _repo.FindAll().Where(q => q.RequestingStudentId == id).Any();

            //PASSING DATA VIA VIEWBAG
            if (IdExists)
            {
                var StudentName = _repo.FindAll().FirstOrDefault(q => q.RequestingStudentId == id).RequestingStudent.FirstName;

                var StudentId = _repo.FindAll().FirstOrDefault(q => q.RequestingStudentId == id).RequestingStudentId;

                if (StudentName != null)
                {
                    ViewBag.StudentName = StudentName;
                }

                if (StudentId != null)
                {
                    ViewBag.StudentId = StudentId;
                }

                var participantList = _repo.FindAll().Where(q => q.RequestingStudentId == id);

                var leaveApplicationsModel = _mapper.Map<List<ParticipantVM>>(participantList);

                return View(leaveApplicationsModel);
            }
            else
            {
                _notyf.Information("Record with this ID may not exist");
                _notyf.Warning("Entering ID via URL is not recommended", 12);
                return RedirectToAction("UserDetails", "Dashboard", new { id = id });
            }

        }
    }
}
