using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using GCUSMS.Contracts;
using GCUSMS.Data;
using GCUSMS.Models;
using GCUSMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace GCUSMS.Controllers
{
    //View for creating departmental teams
    [Authorize(Roles = "Administrator,HODS")]
    public class DepartmentalTeamController : Controller
    {
        private readonly UserManager<StudentModel> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly INotyfService _notyf;
        private readonly ITeamRepository _repo;
        private readonly ITeamPlayerRepository _repoPlayer;
        private readonly ITournamentRepository _repoTournament;
        private readonly ITournamentTeamRepository _repoTournamentTeam;
        private readonly IWebHostEnvironment _hostEnvironment;

        public DepartmentalTeamController
        (
            UserManager<StudentModel> userManager,
            ApplicationDbContext db,
            IMapper mapper,
            INotyfService notyf,
            ITeamRepository repo,
            ITeamPlayerRepository repoPlayer,
            ITournamentRepository repoTournament,
            ITournamentTeamRepository repoTournamentTeam,
            IWebHostEnvironment hostEnvironment
        )
        {
            _userManager = userManager;
            _db = db;
            _mapper = mapper;
            _notyf = notyf;
            _repo = repo;
            _repoPlayer = repoPlayer;
            _repoTournament = repoTournament;
            _repoTournamentTeam = repoTournamentTeam;
            _hostEnvironment = hostEnvironment;
        }

        //View for creating departmental teams
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult Index()
        {
            return View();
        }

        //View for creating departmental teams
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult CreateDepartmentalTeams()
        {
            //Get the department of current logged-in user.
            var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;

            //Send this department to view to only allow to create team of user department
            ViewBag.userDepartment = userDepartment;

            return View();
        }

        /// <summary>
        /// Allow the currently logged in user to create a team for their department
        /// </summary>
        /// <returns>Creates a departmental team for a currently logged in head of department</returns>
        // POST: Team/CreateTeams
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,HODS")]
        public ActionResult CreateDepartmentalTeams(CreateTeamVM model)
        {
            try
            {

                string uniqueFileName = UploadTeamLogo(model);

                if (uniqueFileName == null)
                {
                    _notyf.Error("Please Upload an Image File ", 7);
                    return RedirectToAction("CreateDepartmentalTeams");
                }

                if (!ModelState.IsValid)
                {
                    _notyf.Information("Something Went Wrong!!");
                    return RedirectToAction("CreateDepartmentalTeams");
                }


                var TeamModel = new CreateTeamVM
                {
                    TeamName = model.TeamName.ToUpper(),
                    MaximumPlayers = model.MaximumPlayers,
                    TeamLevel = "DEPARTMENTAL",
                    DepartmentName = model.DepartmentName.ToUpper(),
                    TeamSports = model.TeamSports.ToUpper(),
                    LogoImagePath = uniqueFileName
                };

                var Teams = _mapper.Map<TeamModel>(TeamModel);
                _repo.Create(Teams);
                _repo.Save();

                _notyf.Success("Team Created Successfully");
                return RedirectToAction("CreateDepartmentalTeams");

            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("CreateDepartmentalTeams");
                throw;
            }

        }

        /// <summary>
        /// Uploads a image to the server after validating its extension.
        /// </summary>
        /// <returns>Name of the image file uploaded to the server, if image is not of specific format returns null</returns>
        [Authorize(Roles = "Administrator,HODS")]
        private string UploadTeamLogo(CreateTeamVM model)
        {
            string uniqueFileName = null;

            //Check if the Upload file is Image?
            var FileName = model.Image.FileName;

            var allowedExtensions = new[] { ".gif", ".png", ".jpg", ".jpeg", ".webp", ".svg" };
            var extension = Path.GetExtension(FileName);
            if (!allowedExtensions.Contains(extension))
            {
                _notyf.Warning(extension.ToUpper() + " File type is not Allowed");
                return null;
            }

            if (model.Image != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images/TeamsLogo");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Image.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }

        //For Admin and Head of Departmental Sports
        //Edit View
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult EditDepartmentalTeam(int id)
        {
            var IdExists = _repo.FindAll().Where(q => q.TeamId == id).Any();

            var PlayersSelected = _repoPlayer.FindAll().Where(q => q.TeamId == id).Count();

            var ActivePlayers = _repoPlayer.FindAll().Where(q => q.TeamId == id && q.IsActive == true).Count();

            ViewBag.PlayersSelected = PlayersSelected;
            ViewBag.ActivePlayers = ActivePlayers;


            if (IdExists)
            {
                var TeamById = _repo.FindbyId(id);
                var model = _mapper.Map<EditTeamVM>(TeamById);
                return View(model);
            }
            else
            {
                _notyf.Information("Record with this ID does not exist");
                return RedirectToAction("Index","Dashboard");
            }
        }


        /// <summary>
        /// Updates the team details, takes TeamId and edit model as a parameter
        /// </summary>
        /// <returns>Returns a appropriate response</returns>
        [Authorize(Roles = "Administrator,HODS")]
        [HttpPost]
        public IActionResult EditDepartmentalTeam(EditTeamVM model, int id)
        {
            try
            {
                var IdExists = _repo.FindAll().Where(q => q.TeamId == id).Any();

                if (IdExists)
                {

                    string uniqueFileName = EditTeamLogo(model);

                    var Team = _repo.FindbyId(id);


                    //Delete the previous logo from the server if a new logo is updated.
                    if (uniqueFileName != null)
                    {
                        //delete from root
                        if (Team.LogoImagePath != null)
                        {
                            var ImageDel = Path.Combine(_hostEnvironment.WebRootPath, "images/TeamsLogo", Team.LogoImagePath);
                            FileInfo file = new FileInfo(ImageDel);
                            if (file != null)
                            {
                                System.IO.File.Delete(ImageDel);
                                file.Delete();
                                Team.LogoImagePath = null;
                            }
                        }
                    }

                    if (!ModelState.IsValid)
                    {
                        _notyf.Information("Something Went Wrong!!");
                        return RedirectToAction("EditDepartmentalTeam");
                    }

                    //Gets current logged-in user
                    //Remove if not required anymore
                    var Editor = _userManager.GetUserAsync(User).Result;

                    Team.TeamName = model.TeamName.ToUpper();
                    Team.TeamSports = model.TeamSports.ToUpper();
                    Team.MaximumPlayers = model.MaximumPlayers;


                    if (uniqueFileName != null)
                    {
                        Team.LogoImagePath = uniqueFileName;
                    }

                    var isSuccess = _repo.Update(Team);
                    _repo.Save();

                    _notyf.Success("Team Updated Succesfully");
                    return RedirectToAction("EditDepartmentalTeam");
                }
                else
                {
                    _notyf.Information("Record with this ID does not exist");
                    return RedirectToAction("DepartmentalTeamList");
                }

            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Something went wrong");
                _notyf.Warning(e.Message, 20);
                return View(model);
            }
        }


        /// <summary>
        /// Uploads a image to the server after validating its extension. This method is used if team already has a logo
        /// </summary>
        /// <returns>Name of the image file uploaded to the server, if image is not of specific format returns null</returns>
        [Authorize(Roles = "Administrator,HODS")]
        private string EditTeamLogo(EditTeamVM model)
        {
            string uniqueFileName = null;


            if (model.Image != null)
            {
                //Check if the Upload file is Image?
                var FileName = model.Image.FileName;

                var allowedExtensions = new[] { ".gif", ".png", ".jpg", ".jpeg", ".webp", ".svg" };
                var extension = Path.GetExtension(FileName);
                if (!allowedExtensions.Contains(extension))
                {
                    _notyf.Warning(extension.ToUpper() + " File type is not Allowed");
                    return null;
                }
            }

            if (model.Image != null)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images/TeamsLogo");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.Image.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }


        //Team list where more than two players are allowed per team.
        //Returns view
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult DepartmentalTeamList()
        {
            var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;
            //View Bags
            ViewBag.TotalCricketTeams = _repo.FindAll().Where(q => q.TeamSports == "CRICKET" && q.DepartmentName == userDepartment).Count();
            ViewBag.TotalFootballTeams = _repo.FindAll().Where(q => q.TeamSports == "FOOTBALL" && q.DepartmentName == userDepartment).Count();
            ViewBag.TotalHockeyTeams = _repo.FindAll().Where(q => q.TeamSports == "HOCKEY" && q.DepartmentName == userDepartment).Count();
            ViewBag.TotalBadmintonTeams = _repo.FindAll().Where(q => q.TeamSports == "BADMINTON" && q.DepartmentName == userDepartment).Count();

            ViewBag.TotalTeams = _repo.FindAll().Where(q => q.DepartmentName == userDepartment).Count();

            var TeamList = _repo.FindAll().Where(q => q.MaximumPlayers > 2 && q.DepartmentName == userDepartment);
            var TeamListModel = _mapper.Map<List<TeamVM>>(TeamList);
            return View(TeamListModel);
        }

        //Team list where only one player is allowed per team.
        //Returns view
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult SinglePlayerDepartmentalTeamList()
        {
            var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;

            var TeamList = _repo.FindAll().Where(q => q.MaximumPlayers == 1 && q.DepartmentName == userDepartment);
            var TeamListModel = _mapper.Map<List<TeamVM>>(TeamList);
            return View(TeamListModel);
        }


        //Team list where only two players are allowed per team.
        //Returns view
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult DoublePlayerDepartmentalTeamList()
        {
            var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;

            var TeamList = _repo.FindAll().Where(q => q.MaximumPlayers == 2 && q.DepartmentName == userDepartment);
            var TeamListModel = _mapper.Map<List<TeamVM>>(TeamList);
            return View(TeamListModel);
        }



        //Delete
        /// <summary>
        /// Deletes the team, takes team id as a parameter
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Return appropriate response</returns>
        [Authorize(Roles = "Administrator,HODS")]
        public async Task<IActionResult> DeleteDepartmentalTeam(int id)
        {
            //Getting the Team
            var team = _repo.FindbyId(id);
            //Check if ID Exists
            var IfIdExists = _repo.FindAll().Where(q => q.TeamId == id).Any();

            if (IfIdExists)
            {
                //Getting the Players in this team
                var TeamPlayers = _repoPlayer.FindAll().Where(c => c.TeamId == id);
                var NumberOfPlayers = TeamPlayers.Count();


                if (NumberOfPlayers != 0)
                {
                    var PlayerCounter = NumberOfPlayers;

                    do
                    {
                        var Player = _repoPlayer.FindAll().FirstOrDefault(c => c.TeamId == id);
                        if (Player != null)
                        {
                            var resultPlayer = _repoPlayer.Delete(Player);
                        }
                        PlayerCounter--;
                    }
                    while (PlayerCounter != 0);
                }

                //Getting the tournaments this team participated in
                var TournamentList = _repoTournamentTeam.FindAll().Where(c => c.TeamId == id);
                var NumberOfTournament = TournamentList.Count();


                if (NumberOfTournament != 0)
                {
                    var TournamentCounter = NumberOfTournament;

                    do
                    {
                        var Tournament = _repoTournamentTeam.FindAll().FirstOrDefault(c => c.TeamId == id);
                        if (Tournament != null)
                        {
                            var resultLeave = _repoTournamentTeam.Delete(Tournament);
                        }
                        TournamentCounter--;

                    } while (TournamentCounter != 0);
                }

                //Delete Team Logo
                var TeamLogo = _repo.FindbyId(id).LogoImagePath;
                var ImageDel = Path.Combine(_hostEnvironment.WebRootPath, "images/TeamsLogo", TeamLogo);
                FileInfo file = new FileInfo(ImageDel);
                if (file != null)
                {
                    System.IO.File.Delete(ImageDel);
                    file.Delete();
                }

                //Delete Team Now
                var GetTeam = _repo.Delete(team);
                var changes = await _db.SaveChangesAsync();

                if (changes < 0)
                {
                    _notyf.Error("Unexpected error occurred deleting team with ID: " + team.TeamId, 10);
                    throw new InvalidOperationException($"Unexpected error occurred deleting team with ID '{team.TeamId}'.");
                }

                _notyf.Custom("Team is deleted permanently, this action is not reversible", 30, "#29a847", "fas fa-exclamation-triangle");
                _notyf.Information("HOD Deleted team with ID: " + team.TeamId + " successfully ", 20);
                _notyf.Information("Team Name: " + team.TeamName, 10);
                _notyf.Information("Team Level: " + team.TeamLevel, 7);

            }
            else
            {
                _notyf.Error("Team does not exist", 20);
            }

            return RedirectToAction("DepartmentalTeamList");
        }



        //-------------------------------------- Add Players HOD ----------------------------------------
        //Returns View
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult DepartmentalAddPlayers(int id)
        {
            var IdExists = _repo.FindAll().Where(q => q.TeamId == id).Any();

            //PASSING DATA VIA VIEWBAG
            if (IdExists)
            {


                var TeamName = _repo.FindbyId(id).TeamName;
                var TeamLevel = _repo.FindbyId(id).TeamLevel;
                var TeamSports = _repo.FindbyId(id).TeamSports;
                var MaximumPlayers = _repo.FindbyId(id).MaximumPlayers;

                //Check how many players are already in team
                var PlayersInTeam = _repoPlayer.FindAll().Where(q => q.TeamId == id).Count();
                ViewBag.PlayersInTeam = PlayersInTeam;

                ViewBag.TeamId = id;
                ViewBag.MaximumPlayers = MaximumPlayers;

                if (TeamName != null)
                {
                    ViewBag.TeamName = TeamName;
                }

                if (TeamLevel != null)
                {
                    ViewBag.TeamLevel = TeamLevel;
                }

                if (TeamSports != null)
                {
                    ViewBag.TeamSports = TeamSports;
                }

                return View();
            }
            else
            {
                _notyf.Information("Record with this ID does not exist");
                _notyf.Warning("Entering ID via URL is not recommended", 12);
                return View();
            }
        }

        // POST: Team/AddPlayers/Id
        /// <summary>
        /// Add players to the team, takes team id as parameter
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator,HODS")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DepartmentalAddPlayers(TeamPlayerVM model, int id)
        {
            try
            {
                //Checks
                if (!ModelState.IsValid)
                {
                    _notyf.Information("Something Went Wrong!!");
                    return RedirectToAction("DepartmentalAddPlayers");
                }

                if (model.PlayerId == null)
                {
                    _notyf.Error("Roll number does not exist");
                    return RedirectToAction("DepartmentalAddPlayers");
                }

                //Check if player is already in the team
                var Team = _repo.FindbyId(id);
                var PlayerExists = _repoPlayer.FindAll().FirstOrDefault(q => q.PlayerId == model.PlayerId && q.TeamId == Team.TeamId);


                if (PlayerExists != null)
                {
                    _notyf.Information("This Player is already in the team");
                    return RedirectToAction("DepartmentalAddPlayers");
                }

                //Create a model
                var AddPlayerModel = new TeamPlayerVM
                {
                    PlayerId = model.PlayerId,
                    TeamId = Team.TeamId
                };

                //Add Player to the team
                var AddPlayer = _mapper.Map<TeamPlayerModel>(AddPlayerModel);
                _repoPlayer.Create(AddPlayer);
                _repoPlayer.Save();

                //Throws warning if the players department is different from teams department
                var PlayerDepartment = _repoPlayer.FindAll().FirstOrDefault(q => q.PlayerId == model.PlayerId).Player.DepartmentName;

                if (Team.DepartmentName != null)
                {
                    if (PlayerDepartment != Team.DepartmentName)
                    {
                        _notyf.Warning("Please only add students of " + Team.DepartmentName + " Department");
                    }
                }

                _notyf.Success("Players Added Succesfully!");
                return RedirectToAction("DepartmentalAddPlayers");

            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("DepartmentalAddPlayers");
            }

        }

        //Ajax Calls
        /// <summary>
        /// Returns the roll numbers of players that belong to the department of current logged in user
        /// </summary>
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult GetDepartmentalPlayerId()
        {
            var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;
            //provides suggestions while you type into the field
            var number = HttpContext.Request.Query["term"].ToString();
            var RollNumber = _userManager.Users.Where(c => c.RollNumber.Contains(number) && c.DepartmentName == userDepartment).Select(c => c.RollNumber).ToList();
            return Ok(RollNumber);
        }

        [Authorize(Roles = "Administrator,HODS")]
        [HttpPost]
        public JsonResult DepartmentalFillId(string rollnum)
        {
            //fill input fields when you select RollNumber
            var Id = _userManager.Users.Where(c => c.RollNumber == rollnum).Select(c => c.Id).ToList();
            return new JsonResult(Id);
        }

        [Authorize(Roles = "Administrator,HODS")]
        [HttpGet]
        public JsonResult DepartmentalFillName(string rollnum)
        {
            //fill input fields when you select RollNumber
            var Name = _userManager.Users.Where(c => c.RollNumber == rollnum).Select(c => c.FirstName + " " + c.LastName).ToList();
            return new JsonResult(Name);
        }

        [Authorize(Roles = "Administrator,HODS")]
        [HttpGet]
        public JsonResult DepartmentalFillSemester(string rollnum)
        {
            //fill input fields when you select RollNumber
            var Name = _userManager.Users.Where(c => c.RollNumber == rollnum).Select(c => c.Semester).ToList();
            return new JsonResult(Name);
        }


        [Authorize(Roles = "Administrator,HODS")]
        [HttpGet]
        public JsonResult DepartmentalFillSession(string rollnum)
        {
            //fill input fields when you select RollNumber
            var Name = _userManager.Users.Where(c => c.RollNumber == rollnum).Select(c => c.Session).ToList();
            return new JsonResult(Name);
        }

        [Authorize(Roles = "Administrator,HODS")]
        [HttpGet]
        public JsonResult DepartmentalFillDepartment(string rollnum)
        {
            //fill input fields when you select RollNumber
            var Name = _userManager.Users.Where(c => c.RollNumber == rollnum).Select(c => c.DepartmentName).ToList();
            return new JsonResult(Name);
        }


        //Players List
        //Returns View
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult DepartmentalPlayersList(int id)
        {
            try
            {
                var Team = _repo.FindbyId(id);
                var TeamId = Team.TeamId;
                var TeamName = Team.TeamName;

                if (TeamName != null)
                {
                    ViewBag.TeamName = TeamName;
                }

                ViewBag.TeamId = TeamId;

                var TeamPlayers = _repoPlayer.FindAll().Where(q => q.TeamId == id);
                var TeamPlayerModel = _mapper.Map<List<TeamPlayerVM>>(TeamPlayers);
                return View(TeamPlayerModel);
            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("EditDepartmentalTeam", new { id = id });
                throw;
            }
        }

        //Return view with list of active players
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult DepartmentalActivePlayersList(int id)
        {
            try
            {
                var Team = _repo.FindbyId(id);
                var TeamId = Team.TeamId;
                var TeamName = Team.TeamName;

                if (TeamName != null)
                {
                    ViewBag.TeamName = TeamName;
                }

                ViewBag.TeamId = TeamId;

                var TeamPlayers = _repoPlayer.FindAll().Where(q => q.TeamId == id && q.IsActive == true);
                var TeamPlayerModel = _mapper.Map<List<TeamPlayerVM>>(TeamPlayers);
                return View(TeamPlayerModel);
            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("EditTeam", new { id = id });
                throw;
            }
        }

        //---------------------Player Details------------------------------
        //For Admin and Head of departmental sports
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult DepartmentalPlayerDetails(int id)
        {
            var IdExists = _repoPlayer.FindAll().Where(q => q.TeamPlayerId == id).Any();

            if (IdExists)
            {
                var TeamPlayerById = _repoPlayer.FindbyId(id);
                var model = _mapper.Map<EditTeamPlayerVM>(TeamPlayerById);
                return View(model);
            }
            else
            {
                _notyf.Information("Record with this ID does not exist");
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Updates details of players in the team
        /// </summary>
        /// <param name="model"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator,HODS")]
        [HttpPost]
        public IActionResult DepartmentalPlayerDetails(EditTeamPlayerVM model, int id)
        {
            try
            {
                var IdExists = _repoPlayer.FindAll().Where(q => q.TeamPlayerId == id).Any();
                var TeamPlayer = _repoPlayer.FindbyId(id);
                var TeamId = _repoPlayer.FindAll().FirstOrDefault(q => q.TeamPlayerId == id).TeamId;


                if (IdExists)
                {

                    if (!ModelState.IsValid)
                    {
                        _notyf.Information("Something Went Wrong!!");
                        return RedirectToAction("DepartmentalPlayersList", new { id = TeamId });
                    }

                    TeamPlayer.PlayerHeight = model.PlayerHeight;
                    TeamPlayer.PlayerWeight = model.PlayerWeight;
                    TeamPlayer.PlayerFitnessPoints = model.PlayerFitnessPoints;
                    TeamPlayer.PlayerRole = model.PlayerRole;
                    TeamPlayer.CaptainType = model.CaptainType;
                    TeamPlayer.IsActive = model.IsActive;

                    var isSuccess = _repoPlayer.Update(TeamPlayer);
                    _repo.Save();

                    _notyf.Success("Player Details Updated Succesfully");
                    return RedirectToAction("DepartmentalPlayerDetails", new { id = TeamPlayer.TeamPlayerId });
                }
                else
                {
                    _notyf.Information("Record with this ID does not exist");
                    return RedirectToAction("DepartmentalPlayerDetails", new { id = TeamPlayer.TeamPlayerId }); ;
                }

            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Something went wrong");
                _notyf.Warning(e.Message, 20);
                return View(model);
            }
        }

        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult DepartmentalDeletePlayerFromList(int id)
        {
            var Player = _repoPlayer.FindbyId(id);

            if (Player == null)
            {
                _notyf.Error("Player Does not Exist");
                return RedirectToAction("Index","Dashboard");
                //return NotFound();
            }

            _repoPlayer.Delete(Player);
            _repoPlayer.Save();

            _notyf.Success("Player Deleted Successfully");
            return RedirectToAction("DepartmentalPlayersList", new { id = Player.TeamId });
        }


        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult DepartmentalParticipatedTournaments(int id)
        {
            try
            {

                var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;

                var TeamName = _repo.FindbyId(id).TeamName;
                var TeamId = _repo.FindbyId(id).TeamId;

                @ViewBag.TeamId = TeamId;

                if (TeamName != null)
                {
                    @ViewBag.TeamName = TeamName;
                }

                if (userDepartment != null)
                {
                    @ViewBag.userDepartment = userDepartment;
                }

                var Tournament = _repoTournamentTeam.FindAll().Where(q => q.TeamId == id);
                var model = _mapper.Map<List<TournamentTeamVM>>(Tournament);


                //EDIT------------------------------
                //Get Tournament List this team is part of
                var TournamentListFilter = _repoTournamentTeam.FindAll().Where(q => q.TeamId == id);

                //Get Count of Matches
                if (TournamentListFilter != null)
                {
                    var TotalMatchesPlayed = TournamentListFilter.Sum(q => q.MatchesPlayed);
                    var TotalMatchesWon = TournamentListFilter.Sum(q => q.MatchesWon);
                    var TotalMatchesLost = TournamentListFilter.Sum(q => q.MatchesLost);
                    var TotalMatchesDrawed = TournamentListFilter.Sum(q => q.MatchesDrawed);

                    ViewBag.TotalMatchesPlayed = TotalMatchesPlayed;
                    ViewBag.TotalMatchesWon = TotalMatchesWon;
                    ViewBag.TotalMatchesLost = TotalMatchesLost;
                    ViewBag.TotalMatchesDrawed = TotalMatchesDrawed;
                }

                return View(model);
            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("Index");
            }

        }

        //----------------------- ADD TEAM TO TOURNAMENT ------------------------//
        // GET: Team/AddTeamToTournament/id
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult AddTeamToTournamentDepartmental(int id)
        {
            var IdExists = _repoTournament.FindAll().Where(q => q.TournamentId == id).Any();

            //PASSING DATA VIA VIEWBAG
            if (IdExists)
            {
                var TournamentName = _repoTournament.FindbyId(id).TournamentName;
                var TournamentVenue = _repoTournament.FindbyId(id).TournamentVenue;
                var TournamentType = _repoTournament.FindbyId(id).isIntraDepartmental;
                var TournamentDueDate = _repoTournament.FindbyId(id).DueDate;
                var TournamentMaximumTeams = _repoTournament.FindbyId(id).TeamsAllowed;

                //Check how many teams are already in teamtournament
                var TeamsinTournament = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == id).Count();


                if (TournamentName != null)
                {
                    ViewBag.TournamentName = TournamentName;
                }

                if (TournamentVenue != null)
                {
                    ViewBag.TournamentVenue = TournamentVenue;
                }


                ViewBag.TournamentDueDate = TournamentDueDate;

                ViewBag.TournamentMaximumTeams = TournamentMaximumTeams;
                ViewBag.TeamsinTournament = TeamsinTournament;

                ViewBag.TournamentType = TournamentType;

                ViewBag.TournamentId = id;

                return View();
            }
            else
            {
                _notyf.Information("Record with this ID does not exist");
                _notyf.Warning("Entering ID via URL is not recommended", 12);
                return View();
            }
        }
        
        
        // POST: Team/AddTeamToTournament/id
        [Authorize(Roles = "Administrator,HODS")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddTeamToTournamentDepartmental(TournamentTeamVM model, int id)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    _notyf.Information("Something Went Wrong!!");
                    return RedirectToAction("AddTeamToTournamentDepartmental");
                }

                //Check if tournament exist
                var TournamentId = _repoTournament.FindbyId(id).TournamentId;
                var TeamAlreadyExists = _repoTournamentTeam.FindAll().Where(c => c.TournamentId == TournamentId && c.TeamId == model.TeamId).Any();

                var TournamentIsDepartmental = _repoTournament.FindbyId(id).isIntraDepartmental;
                var TournamentDepartmentName = _repoTournament.FindbyId(id).DepartmentName;
                var TournamentSports = _repoTournament.FindbyId(id).SportsCategory;

                //Check if Team is Departmental Level
                var TeamLevel = _repo.FindbyId(model.TeamId).TeamLevel;
                var TeamDepartmentName = _repo.FindbyId(model.TeamId).DepartmentName;
                var TeamSports = _repo.FindbyId(model.TeamId).TeamSports;


                //If team is from different department than the department allowed to partcipate in tournament
                //Do not add team instead return a warning message and return to the page
                if (TournamentIsDepartmental == true)
                {
                    if (TeamLevel != "DEPARTMENTAL" || TournamentDepartmentName != TeamDepartmentName)
                    {
                        _notyf.Information("This is " + TournamentDepartmentName + " Department Tournament", 10);
                        _notyf.Warning("Only add Teams from the same department as this tournament department");
                        return RedirectToAction("AddTeamToTournamentDepartmental");
                    }
                }

                //If team is from different sports than the team allowed in tournament
                //Do not create team and return back to the page.
                if (TeamSports.ToUpper() != TournamentSports.ToUpper())
                {
                    _notyf.Warning("You can not add " + TeamSports + " team in " + TournamentSports + " tournament");
                    return RedirectToAction("AddTeamToTournamentDepartmental");
                }


                //If team already exist in the tournament
                //Do not re add the team and return to the page
                if (TeamAlreadyExists)
                {
                    _notyf.Warning("Team is already added to the tournament");
                    return RedirectToAction("AddTeamToTournamentDepartmental");
                }

                //Create model
                var TournamentTeamModel = new TournamentTeamVM
                {
                    TournamentId = id,
                    TeamId = model.TeamId
                };


                //Add team to the tournament
                var TournamentTeam = _mapper.Map<TournamentTeamModel>(TournamentTeamModel);
                _repoTournamentTeam.Create(TournamentTeam);
                _repoTournamentTeam.Save();

                var TournamentName = _repoTournament.FindbyId(id).TournamentName;

                _notyf.Success("Team Added to " + TournamentName);

                //Uncomment if you want to only throw error to if team and tournament has different sports
                //Currently above we are not allowing this and returning back to the page instead
                //of just showing the warning

                //if(TeamSports.ToUpper() != TournamentSports.ToUpper())
                //{
                //    _notyf.Warning("You have added " + TeamSports + " team in " + TournamentSports + " tournament");
                //}
                return RedirectToAction("AddTeamToTournamentDepartmental");

            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("AddTeamToTournamentDepartmental");
                throw;
            }

        }


        //Departmental Team AJAX
        //////////////////////////////////// AJAX ////////////////////////////////////


        /// <summary>
        /// Returns team that belong to the same department as the logged-in user
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult SearchDepartmentalTeams()
        {
            var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;
            //provides suggestions while you type into the field
            var name = HttpContext.Request.Query["term"].ToString().ToLower();
            var TeamName = _repo.FindAll().Where(c => c.TeamName.ToLower().Contains(name) && c.DepartmentName == userDepartment).Select(c => c.TeamName).ToList();
            return Ok(TeamName);
        }

        [Authorize(Roles = "Administrator,HODS")]
        [HttpPost]
        public JsonResult FillTeamIdDepartmental(string name)
        {
            //fill input fields when you select RollNumber
            var Id = _repo.FindAll().Where(c => c.TeamName == name).Select(c => c.TeamId).ToList();
            return new JsonResult(Id);
        }

        [Authorize(Roles = "Administrator,HODS")]
        [HttpGet]
        public JsonResult FillTeamNameDepartmental(string name)
        {
            //fill input fields when you select RollNumber
            var Name = _repo.FindAll().Where(c => c.TeamName == name).Select(c => c.TeamName).ToList();
            return new JsonResult(Name);
        }

        [Authorize(Roles = "Administrator,HODS")]
        [HttpGet]
        public JsonResult FillTeamLevelDepartmental(string name)
        {
            //fill input fields when you select RollNumber
            var Name = _repo.FindAll().Where(c => c.TeamName == name).Select(c => c.TeamLevel).ToList();
            return new JsonResult(Name);
        }


        [Authorize(Roles = "Administrator,HODS")]
        [HttpGet]
        public JsonResult FillTeamSportsDepartmental(string name)
        {
            //fill input fields when you select RollNumber
            var Name = _repo.FindAll().Where(c => c.TeamName == name).Select(c => c.TeamSports).ToList();
            return new JsonResult(Name);
        }

        [Authorize(Roles = "Administrator,HODS")]
        [HttpGet]
        public JsonResult FillTeamDepartmentDepartmental(string name)
        {
            //fill input fields when you select RollNumber
            var Name = _repo.FindAll().Where(c => c.TeamName == name).Select(c => c.DepartmentName).ToList();
            return new JsonResult(Name);
        }

        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult ManageDepartmentalTournamentTeam(int id)
        {
            try
            {
                var userDepartment = _userManager.GetUserAsync(User).Result.DepartmentName;

                var TournamentTeam = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == id && q.Team.DepartmentName == userDepartment);
                var model = _mapper.Map<List<TournamentTeamVM>>(TournamentTeam);

                var TotalTeams = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == id).Count();
                var TotalDepartmentalTeams = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == id && q.Team.DepartmentName == userDepartment).Count();
                var TournamentName = _repoTournament.FindbyId(id).TournamentName;

                if (TournamentName != null)
                {
                    ViewBag.TournamentName = TournamentName;
                }

                if (userDepartment != null)
                {
                    ViewBag.userDepartment = userDepartment;
                }

                ViewBag.TotalTeams = TotalTeams;

                ViewBag.TotalDepartmentalTeams = TotalDepartmentalTeams;

                ViewBag.TournamentId = id;


                return View(model);
            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("Index");
            }

        }

        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult EditDepartmentalTournamentTeams(int Teamid, int TournamentId)
        {
            try
            {
                var MatchesPlayed = _repoTournamentTeam.FindTeamByTournamentIdandTeamId(Teamid, TournamentId).MatchesPlayed;
                var MatchesWon = _repoTournamentTeam.FindTeamByTournamentIdandTeamId(Teamid, TournamentId).MatchesWon;
                var MatchesLost = _repoTournamentTeam.FindTeamByTournamentIdandTeamId(Teamid, TournamentId).MatchesLost;
                var MatchesDraw = _repoTournamentTeam.FindTeamByTournamentIdandTeamId(Teamid, TournamentId).MatchesDrawed;

                ViewBag.MatchesPlayed = MatchesPlayed;
                ViewBag.TotalTeamStats = MatchesWon + MatchesLost + MatchesDraw;

                var Teams = _repoTournamentTeam.FindTeamByTournamentIdandTeamId(Teamid, TournamentId);
                var model = _mapper.Map<TournamentTeamVM>(Teams);

                return View(model);
            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("Index");
            }

        }

        [HttpPost]
        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult EditDepartmentalTournamentTeams(TournamentTeamVM model)
        {

            if (ModelState.IsValid)
            {
                //var TournamentId = _repoTournamentTeam.FindTeamByTournamentIdandTeamId(model.TeamId , model.TournamentId).TournamentId;
                var TournamentId = model.TournamentId;

                //Get Points Rule for This Tournament
                var PointsForWin = _repoTournament.FindbyId(TournamentId).PointsForWinning;
                var PointsForLoss = _repoTournament.FindbyId(TournamentId).PointsForLosing;
                var PointsForDraw = _repoTournament.FindbyId(TournamentId).PointsForDraw;

                // Retrieve the Team Object being edited from the database
                var TournamentTeamEdit = _repoTournamentTeam.FindTeamByTournamentIdandTeamId(model.TeamId, model.TournamentId);

                TournamentTeamEdit.MatchesPlayed = model.MatchesPlayed;
                TournamentTeamEdit.MatchesWon = model.MatchesWon;
                TournamentTeamEdit.MatchesLost = model.MatchesLost;
                TournamentTeamEdit.MatchesDrawed = model.MatchesDrawed;

                //Get Match Stats
                var TeamMatchesWon = _repoTournamentTeam.FindTeamByTournamentIdandTeamId(model.TeamId, model.TournamentId).MatchesWon;
                var TeamMatchesLost = _repoTournamentTeam.FindTeamByTournamentIdandTeamId(model.TeamId, model.TournamentId).MatchesLost;
                var TeamMatchesDraw = _repoTournamentTeam.FindTeamByTournamentIdandTeamId(model.TeamId, model.TournamentId).MatchesDrawed;

                //Total Points
                var TotalPoints = (TeamMatchesWon * PointsForWin) + (TeamMatchesDraw * PointsForDraw) + (TeamMatchesLost * PointsForLoss);

                //Update Total Points
                TournamentTeamEdit.TotalPoints = TotalPoints;

                var isSuccess = _repoTournamentTeam.Update(TournamentTeamEdit);
                //Add Save method here too
                _repoTournamentTeam.Save();
            }
            else
            {
                _notyf.Error("Something Went Wrong!");
                return RedirectToAction("EditDepartmentalTournamentTeams", new { id = model.TeamId });
            }

            _notyf.Success("Team stats for this tournament are updated");
            return RedirectToAction("EditDepartmentalTournamentTeams", new { Teamid = model.TeamId, TournamentId = model.TournamentId });
        }


        [Authorize(Roles = "Administrator,HODS")]
        public IActionResult RemoveDepartmentalTeamFromTournament(int id)
        {
            var TeamFromTournament = _repoTournamentTeam.FindbyId(id);
            var TournamentId = TeamFromTournament.TournamentId;

            if (TeamFromTournament == null)
            {
                _notyf.Error("Team Does not Exist");
                return RedirectToAction("Index", "Dashboard");
                //return NotFound();
            }

            _repoTournamentTeam.Delete(TeamFromTournament);
            _repoTournamentTeam.Save();

            _notyf.Success("Team deleted successfully from the tournament");
            return RedirectToAction("ManageDepartmentalTournamentTeam", new { id = TournamentId });
        }

    }
}
