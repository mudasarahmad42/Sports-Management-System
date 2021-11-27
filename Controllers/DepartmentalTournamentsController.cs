using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using GCUSMS.Contracts;
using GCUSMS.Data;
using GCUSMS.Models;
using GCUSMS.ViewModels;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GCUSMS.Controllers
{
    [Authorize(Roles = "Administrator,HODS")]
    public class DepartmentalTournamentsController : Controller
    {
        private readonly UserManager<StudentModel> _userManager;
        private readonly IMapper _mapper;
        private readonly INotyfService _notyf;
        private readonly ITournamentRepository _repo;
        private readonly IParticipantRepository _participant_Repo;
        private readonly ITeamPlayerRepository _teamPlayer_Repo;
        private readonly ITournamentTeamRepository _repoTournamentTeam;
        private readonly ApplicationDbContext _db;

        public DepartmentalTournamentsController
        (
            UserManager<StudentModel> userManager,
            IMapper mapper,
            INotyfService notyf,
            ITournamentRepository repo,
            IParticipantRepository Participant_repo,
            ITeamPlayerRepository teamPlayer_repo,
            ITournamentTeamRepository repoTournamentTeam,
            ApplicationDbContext db
        )
        {
            _userManager = userManager;
            _mapper = mapper;
            _notyf = notyf;
            _repo = repo;
            _participant_Repo = Participant_repo;
            _teamPlayer_Repo = teamPlayer_repo;
            _repoTournamentTeam = repoTournamentTeam;
            _db = db;
        }

        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult Index()
        {
            return View();
        }


        //Returns View
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult CreateDepartmentalTournament()
        {
            var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;

            //Send current logged-in user department to the select as an only option
            //when asked for the department of tournament
            ViewBag.userDepartment = userDepartment;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,HODS")]
        public ActionResult CreateDepartmentalTournament(TournamentVM model)
        {
            try
            {
                //Date Variables
                var startDate = model.StartDate;
                var endDate = model.EndDate;
                int daysRequested = (int)(endDate - startDate).TotalDays;


                //var student = _userManager.GetUserAsync(User).Result;

                //Date Validations at the back end
                if (startDate < DateTime.Today)
                {
                    _notyf.Warning("Enter a Valid start Date");
                    return RedirectToAction("CreateDepartmentalTournament");
                }


                if (daysRequested < 0)
                {
                    _notyf.Warning("Start Date should be less than Ending Date");
                    ModelState.AddModelError("", "Start Date cannot be further in the future than the End Date");
                    return RedirectToAction("CreateDepartmentalTournament");
                }

                if (!ModelState.IsValid)
                {
                    _notyf.Information("Something Went Wrong!!");
                    return RedirectToAction("CreateDepartmentalTournament");
                }


                var Tournamentmodel = new TournamentVM
                {
                    TournamentName = model.TournamentName.ToUpper(),
                    TournamentVenue = model.TournamentVenue.ToUpper(),
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    DueDate = model.DueDate,
                    isActive = model.isActive,
                    TeamsAllowed = model.TeamsAllowed,
                    TotalDaysofTournament = daysRequested,
                    SportsCategory = model.SportsCategory.ToUpper(),
                    GenderAllowed = model.GenderAllowed.ToUpper(),
                    TournamentType = model.TournamentType.ToUpper(),
                    isIntraDepartmental = model.isIntraDepartmental,
                    DepartmentName = model.DepartmentName ?? null,
                };

                var Tournament = _mapper.Map<TournamentModel>(Tournamentmodel);
                _repo.Create(Tournament);
                _repo.Save();

                _notyf.Success("Tournament Created Successfully");
                return RedirectToAction("CreateDepartmentalTournament");

            }
            catch (Exception)
            {
                return RedirectToAction("CreateDepartmentalTournament");
            }

        }

        //LISTS OF TOURNAMENTS
        /// <summary>
        /// Returns the list of tournaments having same department as the logged in
        /// user
        /// </summary>
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult DepartmentalTournamentList()
        {
            var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;

            var TournamentList = _repo.FindAll().Where(q => q.DepartmentName == userDepartment);
            var TournamentListModel = _mapper.Map<List<TournamentVM>>(TournamentList);
            return View(TournamentListModel);
        }

        /// <summary>
        /// Returns the list of tournaments having status "ONGOING" and have the same department as the logged in
        /// user
        /// </summary>
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult OngoingDepartmentalTournamentList()
        {
            var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;

            var TournamentList = _repo.FindAll().Where(q => q.Status == "ONGOING" && q.DepartmentName == userDepartment);
            var TournamentListModel = _mapper.Map<List<TournamentVM>>(TournamentList);
            return View(TournamentListModel);
        }

        /// <summary>
        /// Returns the list of tournaments having status "CONCLUDED" and have the same department as the logged in
        /// user
        /// </summary>
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult ConcludedDepartmentalTournamentList()
        {
            var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;

            var TournamentList = _repo.FindAll().Where(q => q.Status == "CONCLUDED" && q.DepartmentName == userDepartment);
            var TournamentListModel = _mapper.Map<List<TournamentVM>>(TournamentList);
            return View(TournamentListModel);
        }

        /// <summary>
        /// Returns the list of tournaments having status "POSTPONED" and have the same department as the logged in
        /// user
        /// </summary>
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult PostponedDepartmentalTournamentList()
        {
            var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;

            var TournamentList = _repo.FindAll().Where(q => q.Status == "POSTPONED" && q.DepartmentName == userDepartment);
            var TournamentListModel = _mapper.Map<List<TournamentVM>>(TournamentList);
            return View(TournamentListModel);
        }


        /// <summary>
        /// Returns the list of tournaments having status "CANCELLED" and have the same department as the logged in
        /// user
        /// </summary>
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult CancelledDepartmentalTournamentList()
        {
            var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;

            var TournamentList = _repo.FindAll().Where(q => q.Status == "CANCELLED" && q.DepartmentName == userDepartment);
            var TournamentListModel = _mapper.Map<List<TournamentVM>>(TournamentList);
            return View(TournamentListModel);
        }

        /// <summary>
        /// Returns the list of tournaments having status "NULL" and have the same department as the logged in
        /// user
        /// </summary>
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult NotSetDepartmentalTournamentList()
        {
            var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;

            var TournamentList = _repo.FindAll().Where(q => q.Status == null && q.DepartmentName == userDepartment);
            var TournamentListModel = _mapper.Map<List<TournamentVM>>(TournamentList);
            return View(TournamentListModel);
        }


        //Returns View
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult DepartmentalTournamentDetails(int id)
        {

            var Details = _repo.FindbyId(id);

            var model = _mapper.Map<TournamentVM>(Details);

            return View(model);
        }

        /// <summary>
        /// Returns the list of partcipants in the tournament
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult DepartmentalTournamentParticipants(int id)
        {
            var ParticipantDetails = _participant_Repo.FindAll().Where(q => q.AppliedTournamentId == id);
            var model = _mapper.Map<List<ParticipantVM>>(ParticipantDetails);
            return View(model);
        }

        /// <summary>
        /// Edit the details of the departmental tournaments
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult DepartmentalTournamentDetailsEdit(TournamentVM model)
        {
            var Trnid = model.TournamentId;

            if (ModelState.IsValid)
            {
                // Retrieve the employee being edited from the database
                var TournamentEdit = _repo.FindbyId(model.TournamentId);

                TournamentEdit.DueDate = model.DueDate;
                TournamentEdit.StartDate = model.StartDate;
                TournamentEdit.EndDate = model.EndDate;
                TournamentEdit.isActive = model.isActive;
                TournamentEdit.TeamsAllowed = model.TeamsAllowed;
                TournamentEdit.TournamentName = model.TournamentName.ToUpper();
                TournamentEdit.PointsForWinning = model.PointsForWinning;
                TournamentEdit.PointsForLosing = model.PointsForLosing;
                TournamentEdit.PointsForDraw = model.PointsForDraw;


                //Get All the teams in the tournament team and update their scores
                var TeamsList = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == model.TournamentId);
                var NumberofTeams = TeamsList.Count();

                foreach (var item in TeamsList)
                {
                    //Get Match Stats
                    var TeamMatchesWon = item.MatchesWon;
                    var TeamMatchesLost = item.MatchesLost;
                    var TeamMatchesDraw = item.MatchesDrawed;

                    //Total Points
                    var TotalPoints = (TeamMatchesWon * TournamentEdit.PointsForWinning) + (TeamMatchesDraw * TournamentEdit.PointsForDraw) + (TeamMatchesLost * TournamentEdit.PointsForLosing);

                    item.TotalPoints = TotalPoints;

                    var isSuccessTournament = _repoTournamentTeam.Update(item);
                }

                if (model.Message != null)
                {
                    TournamentEdit.Message = model.Message;
                }
                else
                {
                    TournamentEdit.Message = null;
                }

                if (model.Winner != null)
                {
                    TournamentEdit.Winner = model.Winner.ToUpper();
                }
                else
                {
                    TournamentEdit.Winner = null;
                }

                if (model.RunnerUp != null)
                {
                    TournamentEdit.RunnerUp = model.RunnerUp.ToUpper();
                }
                else
                {
                    TournamentEdit.RunnerUp = null;
                }

                if (model.Status != null)
                {
                    TournamentEdit.Status = model.Status.ToUpper();
                }
                else
                {
                    TournamentEdit.Status = null;
                }

                if (model.SportsCategory != null)
                {
                    TournamentEdit.SportsCategory = model.SportsCategory.ToUpper();
                }
                else
                {
                    TournamentEdit.SportsCategory = null;
                }


                if (model.GenderAllowed != null)
                {
                    TournamentEdit.GenderAllowed = model.GenderAllowed.ToUpper();
                }
                else
                {
                    TournamentEdit.GenderAllowed = null;
                }

                if (model.TournamentType != null)
                {
                    TournamentEdit.TournamentType = model.TournamentType.ToUpper();
                }
                else
                {
                    TournamentEdit.TournamentType = null;
                }

                var isSuccess = _repo.Update(TournamentEdit);
            }
            else
            {
                _notyf.Error("Something Went Wrong!");
                return RedirectToAction("DepartmentalTournamentDetails", new { id = Trnid });
            }

            _notyf.Success("Tournament Details are Updated");
            return RedirectToAction("DepartmentalTournamentDetails", new { id = Trnid });
        }

        /// <summary>
        /// Returns the scores and standings of teams in the tournament
        /// </summary>
        /// <param name="id"></param>
        /// <returns>View</returns>
        public IActionResult DepartmentalPointsTable(int id)
        {
            //View Bags
            var TournamentName = _repo.FindbyId(id).TournamentName;
            var TournamentId = _repo.FindbyId(id).TournamentId;
            ViewBag.TournamentId = TournamentId;

            if (TournamentName != null)
            {
                ViewBag.TournamentName = TournamentName;
            }

            var TeamList = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == id).OrderByDescending(q => q.TotalPoints);
            var TeamListModel = _mapper.Map<List<TournamentTeamVM>>(TeamList);
            return View(TeamListModel);
        }


        //Fixtures View
        public IActionResult DepartmentalFixtures(int id)
        {
            try
            {
                //View Bags
                var TournamentName = _repo.FindbyId(id).TournamentName;
                var TournamentStartDate = _repo.FindbyId(id).StartDate;
                var TournamentEndDate = _repo.FindbyId(id).EndDate;

                var NumberOfTeams = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == id).Count();

                if (TournamentName != null)
                {
                    ViewBag.TournamentName = TournamentName;
                }


                ViewBag.TournamentStartDate = TournamentStartDate;



                ViewBag.TournamentEndDate = TournamentEndDate;


                ViewBag.NumberOfTeams = NumberOfTeams;


                string[] myArray = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == id).Select(q => q.Team.TeamName).ToArray();
                ViewData["TeamNames"] = myArray;


                return View();
            }
            catch (Exception e)
            {

                _notyf.Error(e.Message);
                return RedirectToAction("DepartmentalTournamentList");
            }

        }

        /// <summary>
        /// Delete the tournament, takes tournament id as a parameter
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns appropriate response</returns>
        [Authorize(Roles = "Administrator,HODS")]
        public async Task<IActionResult> DeleteDepartmentalTournament(int id)
        {
            //Getting the Tournament
            var Tournament = _repo.FindbyId(id);
            //Check if ID Exists
            var IfIdExists = _repo.FindAll().Where(q => q.TournamentId == id).Any();

            if (IfIdExists)
            {
                //Getting the Participants of this tournament
                var Participants = _participant_Repo.FindAll().Where(c => c.AppliedTournamentId == id);
                var NumberOfParticipants = Participants.Count();

                if (NumberOfParticipants != 0)
                {
                    var ParticipantCounter = NumberOfParticipants;

                    do
                    {
                        var Participant = _participant_Repo.FindAll().FirstOrDefault(c => c.AppliedTournamentId == id);
                        if (Participant != null)
                        {
                            var resultParticipant = _participant_Repo.Delete(Participant);
                        }
                        ParticipantCounter--;
                    }
                    while (ParticipantCounter != 0);
                }

                //Getting the team record of this tournament
                var TeamList = _repoTournamentTeam.FindAll().Where(c => c.TournamentId == id);
                var NumberOfTeams = TeamList.Count();


                if (NumberOfTeams != 0)
                {
                    var TeamsCounter = NumberOfTeams;

                    do
                    {
                        var Team = _repoTournamentTeam.FindAll().FirstOrDefault(c => c.TournamentId == id);
                        if (Team != null)
                        {
                            var resultLeave = _repoTournamentTeam.Delete(Team);
                        }
                        TeamsCounter--;

                    } while (TeamsCounter != 0);
                }

                //Delete Tournament Now
                var GetTeam = _repo.Delete(Tournament);
                var changes = await _db.SaveChangesAsync();

                if (changes < 0)
                {
                    _notyf.Error("Unexpected error occurred deleting team with ID: " + Tournament.TournamentId, 10);
                    throw new InvalidOperationException($"Unexpected error occurred deleting team with ID '{Tournament.TournamentId}'.");
                }

                _notyf.Custom("Tournament is deleted permanently, this action is not reversible", 30, "#29a847", "fas fa-exclamation-triangle");
                _notyf.Information("HOD Deleted tournament with ID: " + Tournament.TournamentId + " successfully ", 20);
                _notyf.Information("Tournament Name: " + Tournament.TournamentName, 10);

            }
            else
            {
                _notyf.Error("Tournament does not exist", 20);
            }

            return RedirectToAction("DepartmentalTournamentList");
        }
    }
}
