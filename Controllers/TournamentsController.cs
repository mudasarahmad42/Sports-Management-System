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
    [Authorize]
    public class TournamentsController : Controller
    {
        private readonly UserManager<StudentModel> _userManager;
        private readonly IMapper _mapper;
        private readonly INotyfService _notyf;
        private readonly ITournamentRepository _repo;
        private readonly IParticipantRepository _participant_Repo;
        private readonly ITeamPlayerRepository _teamPlayer_Repo;
        private readonly ITournamentTeamRepository _repoTournamentTeam;
        private readonly ApplicationDbContext _db;

        public TournamentsController
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

        public IActionResult Index()
        {
            return View();
        }


        [Authorize(Roles = "Administrator")]
        public IActionResult CreateTournament()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult TournamentList()
        {
            var TournamentList = _repo.FindAll();
            var TournamentListModel = _mapper.Map<List<TournamentVM>>(TournamentList);
            return View(TournamentListModel);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult OngoingTournamentList()
        {
            var TournamentList = _repo.FindAll().Where(q => q.Status == "ONGOING");
            var TournamentListModel = _mapper.Map<List<TournamentVM>>(TournamentList);
            return View(TournamentListModel);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult ConcludedTournamentList()
        {
            var TournamentList = _repo.FindAll().Where(q => q.Status == "CONCLUDED");
            var TournamentListModel = _mapper.Map<List<TournamentVM>>(TournamentList);
            return View(TournamentListModel);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult PostponedTournamentList()
        {
            var TournamentList = _repo.FindAll().Where(q => q.Status == "POSTPONED");
            var TournamentListModel = _mapper.Map<List<TournamentVM>>(TournamentList);
            return View(TournamentListModel);
        }


        [Authorize(Roles = "Administrator")]
        public IActionResult CancelledTournamentList()
        {
            var TournamentList = _repo.FindAll().Where(q => q.Status == "CANCELLED");
            var TournamentListModel = _mapper.Map<List<TournamentVM>>(TournamentList);
            return View(TournamentListModel);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult NotSetTournamentList()
        {
            var TournamentList = _repo.FindAll().Where(q => q.Status == null);
            var TournamentListModel = _mapper.Map<List<TournamentVM>>(TournamentList);
            return View(TournamentListModel);
        }

        /// <summary>
        /// Returns details of students
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        public IActionResult TournamentDetails(int id)
        {

            var Details = _repo.FindbyId(id);

            var model = _mapper.Map<TournamentVM>(Details);

            return View(model);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult TournamentParticipants(int id)
        {
            var ParticipantDetails = _participant_Repo.FindAll().Where(q => q.AppliedTournamentId == id);
            var model = _mapper.Map<List<ParticipantVM>>(ParticipantDetails);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public IActionResult TournamentDetailsEdit(TournamentVM model)
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
                TournamentEdit.isIntraDepartmental = model.isIntraDepartmental;
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

                

                if (model.DepartmentName != null)
                {
                    TournamentEdit.DepartmentName = model.DepartmentName.ToUpper();
                }

                if (TournamentEdit.isIntraDepartmental == false)
                {
                    TournamentEdit.DepartmentName = null;
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
                return RedirectToAction("TournamentDetails", new { id = Trnid });
            }

            _notyf.Success("Tournament Details are Updated");
            return RedirectToAction("TournamentDetails", new { id = Trnid });
        }

        //This method apply pagination from Model Pagination
        [Authorize]
        public async Task<IActionResult> TournamentAvailable
        (
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber
        )
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            //View Bags
            if(_userManager.GetUserAsync(User).Result.DepartmentName != null)
            {
                ViewBag.UserDepartment = _userManager.GetUserAsync(User).Result.DepartmentName.ToUpper();
            }

            var UserGender = _userManager.GetUserAsync(User).Result.Gender;
            //View Bags
            if (UserGender != null)
            {
                if (UserGender == "MALE" || UserGender == "NOT TO BE SPECIFIED")
                {
                    ViewBag.UserGender = "MEN";
                }

                if (UserGender == "FEMALE" || UserGender == "NOT TO BE SPECIFIED")
                {
                    ViewBag.UserGender = "WOMEN";
                }

            }

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var tournaments = from s in _db.Tournaments
                              select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                tournaments = tournaments.Where(s => s.TournamentName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    tournaments = tournaments.OrderBy(s => s.DueDate);
                    break;
                case "Date":
                    tournaments = tournaments.OrderBy(s => s.StartDate);
                    break;
                case "date_desc":
                    tournaments = tournaments.OrderByDescending(s => s.StartDate);
                    break;
                default:
                    tournaments = tournaments.OrderBy(s => s.TournamentName);
                    break;
            }

            int pageSize = 5;

            return View(await PaginatedList<TournamentModel>.CreateAsync(tournaments.AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        // POST: EquipmentsController/Allocations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult CreateTournament(TournamentVM model)
        {
            try
            {
                //Date Variables
                var startDate = model.StartDate;
                var endDate = model.EndDate;
                int daysRequested = (int)(endDate - startDate).TotalDays;


                //var student = _userManager.GetUserAsync(User).Result;

                if (startDate < DateTime.Today)
                {
                    _notyf.Warning("Enter a Valid start Date");
                    return RedirectToAction("CreateTournament");
                }


                if (daysRequested < 0)
                {
                    _notyf.Warning("Start Date should be less than Ending Date");
                    ModelState.AddModelError("", "Start Date cannot be further in the future than the End Date");
                    return RedirectToAction("CreateTournament");
                }

                if (!ModelState.IsValid)
                {
                    _notyf.Information("Something Went Wrong!!");
                    return RedirectToAction("CreateTournament");
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
                return RedirectToAction("CreateTournament");

            }
            catch (Exception)
            {
                return RedirectToAction("CreateTournament");
                throw;
            }

        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteTournament(int id)
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
                _notyf.Information("Admin Deleted tournament with ID: " + Tournament.TournamentId + " successfully ", 20);
                _notyf.Information("Tournament Name: " + Tournament.TournamentName, 10);

            }
            else
            {
                _notyf.Error("Team does not exist", 20);
            }

            return RedirectToAction("TournamentList");
        }

        public IActionResult PointsTable(int id)
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

        public IActionResult Fixtures(int id)
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
                return RedirectToAction("TournamentList");
            }

        }

        public IActionResult DoubleRoundRobinTournament(int id)
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

                ViewBag.NumberOfTeams = NumberOfTeams;

                 ViewBag.TournamentStartDate = TournamentStartDate;
                

                
                ViewBag.TournamentEndDate = TournamentEndDate;
                

                string[] myArray = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == id).Select(q => q.Team.TeamName).ToArray();
                ViewData["TeamNames"] = myArray;


                return View();
            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("TournamentList");
            }
        }

        public IActionResult KnockoutTournament(int id)
        {
            try
            {
                //View Bags
                var TournamentName = _repo.FindbyId(id).TournamentName;

                var NumberOfTeams = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == id).Count();

                if (NumberOfTeams == 4 || NumberOfTeams == 8 || NumberOfTeams == 16 || NumberOfTeams == 32)
                {


                    if (TournamentName != null)
                    {
                        ViewBag.TournamentName = TournamentName;
                    }

                    ViewBag.NumberOfTeams = NumberOfTeams;


                    string[] myArray = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == id).Select(q => q.Team.TeamName).ToArray();
                    ViewData["TeamNames"] = myArray;


                    return View();

                }
                else
                {
                    _notyf.Warning("Number of Teams should be 4, 8, 16 or 32");
                    return RedirectToAction("TournamentDetails", new { id = id });
                }
            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("TournamentList");
            }
            
        }

        public IActionResult AllPlayers()
        {
            try
            {
                //Get the list of IDs of all teams currently in tournament's table
                var ListOfTeams = _repoTournamentTeam.FindAll().Select(q => q.TeamId).Distinct();

                //Now use these id to get all players in these teams
                var ListOfAllPlayers = _teamPlayer_Repo.FindAll().Where(q => ListOfTeams.Contains(q.TeamId));
                var AllPlayersModel = _mapper.Map<List<TeamPlayerVM>>(ListOfAllPlayers);
                return View(AllPlayersModel);
            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("Index", "Dashboard");
            }

        }

        /// <summary>
        /// Getting the tournament where signed-in user has submitted partcipation form and
        /// where their team has played in
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult MyTournaments(string id)
        {
            try
            {
                var Student = _userManager.GetUserAsync(User).Result;
                var StudentId = Student.Id;

                //List of id of teams student is part of
                var ListofTeams = _teamPlayer_Repo.FindAll().Where(q => q.Player.Id == StudentId).Select(c => c.TeamId);

                //Get list of partcipants
                var ListofParticipationsTournaments = _participant_Repo.FindAll().Where(q => q.RequestingStudentId == StudentId && q.isSelected == true).Select(c => c.TournamentApplied.TournamentId);

                //Get Tournaments having teams with the ids we retrieved earlier
                var ListofTouranments = _repoTournamentTeam.FindAll().Where(q => ListofTeams.Contains(q.TeamId) && ListofParticipationsTournaments.Contains(q.TournamentId));

                var ListofTournamentModel = _mapper.Map<List<TournamentTeamVM>>(ListofTouranments);


                //var participantList = _participant_Repo.FindAll().Where(q => q.RequestingStudentId == StudentId && q.isSelected == true);
                //var PartcipationVModel = _mapper.Map<List<ParticipantVM>>(participantList);

                return View(ListofTournamentModel);
            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("Index", "Dashboard");
            }

        }

        public IActionResult TournamentDetail(int id)
        {
            try
            {
                //Get the tournament from id
                var Tournaments = _repo.FindbyId(id);
                var TournamentModel = _mapper.Map<TournamentVM>(Tournaments);
                return View(TournamentModel);
            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("Index", "Dashboard");
            }
        }

    }
}
