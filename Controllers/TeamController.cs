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
    [Authorize]
    public class TeamController : Controller
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

        public TeamController
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

        [Authorize(Roles = "Administrator")]
        public IActionResult CreateTeams()
        {
            return View();
        }

        // POST: Team/CreateTeams
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult CreateTeams(CreateTeamVM model)
        {
            try
            {

                string uniqueFileName = UploadTeamLogo(model);

                if (uniqueFileName == null)
                {
                    _notyf.Error("Please Upload an Image File ", 7);
                    return RedirectToAction("CreateTeams");
                }

                if (!ModelState.IsValid)
                {
                    _notyf.Information("Something Went Wrong!!");
                    return RedirectToAction("CreateTeams");
                }

                if (model.DepartmentName != null)
                {
                    var TeamModel = new CreateTeamVM
                    {
                        TeamName = model.TeamName.ToUpper(),
                        MaximumPlayers = model.MaximumPlayers,
                        TeamLevel = model.TeamLevel.ToUpper(),
                        DepartmentName = model.DepartmentName.ToUpper(),
                        TeamSports = model.TeamSports.ToUpper(),
                        LogoImagePath = uniqueFileName
                    };

                    var Teams = _mapper.Map<TeamModel>(TeamModel);
                    _repo.Create(Teams);
                    _repo.Save();
                }
                else
                {
                    var TeamModel = new CreateTeamVM
                    {
                        TeamName = model.TeamName.ToUpper(),
                        MaximumPlayers = model.MaximumPlayers,
                        TeamLevel = model.TeamLevel.ToUpper(),
                        TeamSports = model.TeamSports.ToUpper(),
                        LogoImagePath = uniqueFileName
                    };

                    var Teams = _mapper.Map<TeamModel>(TeamModel);
                    _repo.Create(Teams);
                    _repo.Save();
                }

                _notyf.Success("Team Created Successfully");
                return RedirectToAction("CreateTeams");

            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("CreateTeams");
                throw;
            }

        }

        //For Admin
        [Authorize(Roles = "Administrator")]
        public IActionResult EditTeam(int id)
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
                return RedirectToAction("TeamList");
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult EditTeam(EditTeamVM model, int id)
        {
            try
            {
                var IdExists = _repo.FindAll().Where(q => q.TeamId == id).Any();

                if (IdExists)
                {

                    string uniqueFileName = EditTeamLogo(model);

                    var Team = _repo.FindbyId(id);

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
                        return RedirectToAction("EditTeam");
                    }

                    var Editor = _userManager.GetUserAsync(User).Result;

                    Team.TeamName = model.TeamName.ToUpper();
                    Team.TeamSports = model.TeamSports.ToUpper();
                    Team.TeamLevel = model.TeamLevel.ToUpper();
                    Team.MaximumPlayers = model.MaximumPlayers;

                    if(model.DepartmentName != null)
                    {
                        Team.DepartmentName = model.DepartmentName.ToUpper();
                    }
        
                    if(Team.TeamLevel == "UNIVERSITY")
                    {
                        Team.DepartmentName = null;
                    }

                    if (uniqueFileName != null)
                    {
                        Team.LogoImagePath = uniqueFileName;
                    }

                    var isSuccess = _repo.Update(Team);
                    _repo.Save();

                    _notyf.Success("Team Updated Succesfully");
                    return RedirectToAction("EditTeam");
                }
                else
                {
                    _notyf.Information("Record with this ID does not exist");
                    return RedirectToAction("TeamList");
                }

            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Something went wrong");
                _notyf.Warning(e.Message, 20);
                return View(model);
            }
        }


        //For Admin
        [Authorize(Roles = "Administrator")]
        public IActionResult PlayerDetails(int id)
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

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public IActionResult PlayerDetails(EditTeamPlayerVM model, int id)
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
                        return RedirectToAction("PlayersList", new { id = TeamId });
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
                    return RedirectToAction("PlayerDetails", new { id = TeamPlayer.TeamPlayerId });
                }
                else
                {
                    _notyf.Information("Record with this ID does not exist");
                    return RedirectToAction("PlayerDetails", new { id = TeamPlayer.TeamPlayerId }); ;
                }

            }
            catch (Exception e)
            {
                ModelState.AddModelError("", "Something went wrong");
                _notyf.Warning(e.Message, 20);
                return View(model);
            }
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult TeamList()
        {
            //View Bags
            ViewBag.TotalCricketTeams = _repo.FindAll().Where(q => q.TeamSports == "CRICKET").Count();
            ViewBag.TotalFootballTeams = _repo.FindAll().Where(q => q.TeamSports == "FOOTBALL").Count();
            ViewBag.TotalHockeyTeams = _repo.FindAll().Where(q => q.TeamSports == "HOCKEY").Count();
            ViewBag.TotalBadmintonTeams = _repo.FindAll().Where(q => q.TeamSports == "BADMINTON").Count();
            ViewBag.TotalDepartmentalTeams = _repo.FindAll().Where(q => q.TeamLevel == "DEPARTMENTAL").Count();
            ViewBag.TotalUniversityTeams = _repo.FindAll().Where(q => q.TeamLevel == "UNIVERSITY").Count();

            //var TeamList = _repo.FindAll();

            var TeamList = _repo.FindAll().Where(q => q.MaximumPlayers > 2);
            var TeamListModel = _mapper.Map<List<TeamVM>>(TeamList);
            return View(TeamListModel);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult SinglePlayerTeamList()
        {
            var TeamList = _repo.FindAll().Where(q => q.MaximumPlayers == 1);
            var TeamListModel = _mapper.Map<List<TeamVM>>(TeamList);
            return View(TeamListModel);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult DoublePlayerTeamList()
        {
            var TeamList = _repo.FindAll().Where(q => q.MaximumPlayers == 2);
            var TeamListModel = _mapper.Map<List<TeamVM>>(TeamList);
            return View(TeamListModel);
        }




        [Authorize(Roles = "Administrator")]
        public IActionResult StudentTeamList(string id)
        {
            var StudentTeamList = _repoPlayer.FindAll().Where(q => q.PlayerId == id);
            var StudentTeamListModel = _mapper.Map<List<TeamPlayerVM>>(StudentTeamList);
            return View(StudentTeamListModel);
        }

        [Authorize(Roles = "Administrator")]
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

        [Authorize(Roles = "Administrator")]
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

        [Authorize(Roles = "Administrator")]
        public IActionResult PlayersList(int id)
        {
            try
            {
                var Team = _repo.FindbyId(id);
                var TeamId = Team.TeamId;
                var TeamName = Team.TeamName;

                if(TeamName != null)
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
                return RedirectToAction("EditTeam", new { id = id });
                throw;
            }
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult ActivePlayersList(int id)
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


        public IActionResult MyTeam(int id)
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

        /// <summary>
        /// Populate the details of the Team and Players
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>


        // GET: Team/AddPlayer/Id
        [Authorize(Roles = "Administrator")]
        public IActionResult AddPlayers(int id)
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
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPlayers(TeamPlayerVM model, int id)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    _notyf.Information("Something Went Wrong!!");
                    return RedirectToAction("AddPlayers");
                }

                if(model.PlayerId == null)
                {
                    _notyf.Error("Roll number does not exist");
                    return RedirectToAction("AddPlayers");
                }

                var Team = _repo.FindbyId(id);
                var PlayerExists = _repoPlayer.FindAll().FirstOrDefault(q => q.PlayerId == model.PlayerId && q.TeamId == Team.TeamId);
                

                if (PlayerExists != null)
                {
                    _notyf.Information("This Player is already in the team");
                    return RedirectToAction("AddPlayers");
                }

                var AddPlayerModel = new TeamPlayerVM
                {
                     PlayerId = model.PlayerId,
                     TeamId = Team.TeamId
                };

                var AddPlayer = _mapper.Map<TeamPlayerModel>(AddPlayerModel);
                _repoPlayer.Create(AddPlayer);
                _repoPlayer.Save();

                var PlayerDepartment = _repoPlayer.FindAll().FirstOrDefault(q => q.PlayerId == model.PlayerId).Player.DepartmentName;

                if (Team.DepartmentName != null)
                {
                    if (PlayerDepartment != Team.DepartmentName)
                    {
                        _notyf.Warning("Please only add students of " + Team.DepartmentName + " Department");
                    }
                }

                //Getting list of teams this player is already part of
                //var ListofTeams = _repoPlayer.FindAll().Where(q => q.PlayerId == model.PlayerId).Select(q => q.Team.TeamName);

                //if (ListofTeams != null)
                //{
                //    _notyf.Information("This players is also part of following teams");
                //    foreach (var item in ListofTeams)
                //    {
                //        _notyf.Information(item,2);
                //    }
                    
                //}

                _notyf.Success("Players Added Succesfully!");
                return RedirectToAction("AddPlayers");

            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("AddPlayers");
            }

        }

        //Ajax Calls

        [Authorize(Roles = "Administrator")]
        public IActionResult GetPlayerId()
        {
            //provides suggestions while you type into the field
            var number = HttpContext.Request.Query["term"].ToString();
            var RollNumber = _userManager.Users.Where(c => c.RollNumber.Contains(number)).Select(c => c.RollNumber).ToList();
            return Ok(RollNumber);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public JsonResult FillId(string rollnum)
        {
            //fill input fields when you select RollNumber
            var Id = _userManager.Users.Where(c => c.RollNumber == rollnum).Select(c => c.Id).ToList();
            return new JsonResult(Id);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public JsonResult FillName(string rollnum)
        {
            //fill input fields when you select RollNumber
            var Name = _userManager.Users.Where(c => c.RollNumber == rollnum).Select(c => c.FirstName + " " + c.LastName).ToList();
            return new JsonResult(Name);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public JsonResult FillSemester(string rollnum)
        {
            //fill input fields when you select RollNumber
            var Name = _userManager.Users.Where(c => c.RollNumber == rollnum).Select(c => c.Semester).ToList();
            return new JsonResult(Name);
        }


        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public JsonResult FillSession(string rollnum)
        {
            //fill input fields when you select RollNumber
            var Name = _userManager.Users.Where(c => c.RollNumber == rollnum).Select(c => c.Session).ToList();
            return new JsonResult(Name);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public JsonResult FillDepartment(string rollnum)
        {
            //fill input fields when you select RollNumber
            var Name = _userManager.Users.Where(c => c.RollNumber == rollnum).Select(c => c.DepartmentName).ToList();
            return new JsonResult(Name);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult DeletePlayerFromList(int id)
        {
            var Player = _repoPlayer.FindbyId(id);

            if (Player == null)
            {
                _notyf.Error("Player Does not Exist");
                return NotFound();
            }

            _repoPlayer.Delete(Player);
            _repoPlayer.Save();

            _notyf.Success("Player Deleted Successfully");
            return RedirectToAction("PlayersList", new { id = Player.TeamId });
        }


        //Adding Teams to Tournaments
        /// <summary>
        /// This method use ajax calls to populate list and add teams
        /// to particular tournament
        /// </summary>
        /// <param></param>
        /// <returns></returns>

        [Authorize(Roles = "Administrator")]
        // GET: Team/AddTeamToTournament/id
        public IActionResult AddTeamToTournament(int id)
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

        [Authorize(Roles = "Administrator")]
        // POST: Team/AddTeamToTournament/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddTeamToTournament(TournamentTeamVM model, int id)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    _notyf.Information("Something Went Wrong!!");
                    return RedirectToAction("AddTeamToTournament");
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

                if (TournamentIsDepartmental == true)
                {
                    if (TeamLevel != "DEPARTMENTAL" || TournamentDepartmentName != TeamDepartmentName)
                    {
                        _notyf.Information("This is " + TournamentDepartmentName + " Department Tournament",10);
                        _notyf.Warning("Only add Teams from the same department as this tournament department");
                        return RedirectToAction("AddTeamToTournament");
                    }
                }

                if (TeamSports.ToUpper() != TournamentSports.ToUpper())
                {
                    _notyf.Warning("You can not add " + TeamSports + " team in " + TournamentSports + " tournament");
                    return RedirectToAction("AddTeamToTournament");
                }


                if (TeamAlreadyExists)
                {
                    _notyf.Warning("Team is already added to the tournament");
                    return RedirectToAction("AddTeamToTournament");
                }

                var TournamentTeamModel = new TournamentTeamVM
                {
                    TournamentId = id,
                    TeamId = model.TeamId
                };

                var TournamentTeam = _mapper.Map<TournamentTeamModel>(TournamentTeamModel);
                _repoTournamentTeam.Create(TournamentTeam);
                _repoTournamentTeam.Save();

                var TournamentName = _repoTournament.FindbyId(id).TournamentName;

                _notyf.Success("Team Added to " + TournamentName);
                //if(TeamSports.ToUpper() != TournamentSports.ToUpper())
                //{
                //    _notyf.Warning("You have added " + TeamSports + " team in " + TournamentSports + " tournament");
                //}
                return RedirectToAction("AddTeamToTournament");

            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("AddTeamToTournament");
                throw;
            }

        }

        ///////////////////// AJAX ////////////////////////////////////


        [Authorize(Roles = "Administrator")]
        public IActionResult SearchTeams()
        {
            //provides suggestions while you type into the field
            var name = HttpContext.Request.Query["term"].ToString().ToLower();
            var TeamName = _repo.FindAll().Where(c => c.TeamName.ToLower().Contains(name)).Select(c => c.TeamName).ToList();
            return Ok(TeamName);
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public JsonResult FillTeamId(string name)
        {
            //fill input fields when you select RollNumber
            var Id = _repo.FindAll().Where(c => c.TeamName == name).Select(c => c.TeamId).ToList();
            return new JsonResult(Id);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public JsonResult FillTeamName(string name)
        {
            //fill input fields when you select RollNumber
            var Name = _repo.FindAll().Where(c => c.TeamName == name).Select(c => c.TeamName).ToList();
            return new JsonResult(Name);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public JsonResult FillTeamLevel(string name)
        {
            //fill input fields when you select RollNumber
            var Name = _repo.FindAll().Where(c => c.TeamName == name).Select(c => c.TeamLevel).ToList();
            return new JsonResult(Name);
        }


        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public JsonResult FillTeamSports(string name)
        {
            //fill input fields when you select RollNumber
            var Name = _repo.FindAll().Where(c => c.TeamName == name).Select(c => c.TeamSports).ToList();
            return new JsonResult(Name);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public JsonResult FillTeamDepartment(string name)
        {
            //fill input fields when you select RollNumber
            var Name = _repo.FindAll().Where(c => c.TeamName == name).Select(c => c.DepartmentName).ToList();
            return new JsonResult(Name);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult ManageTournamentTeam(int id)
        {
            try
            {
                var TournamentTeam = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == id);
                var model = _mapper.Map<List<TournamentTeamVM>>(TournamentTeam);

                var TotalTeams = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == id).Count();
                var TournamentName = _repoTournament.FindbyId(id).TournamentName;

                if (TournamentName != null)
                {
                    ViewBag.TournamentName = TournamentName;
                }

                ViewBag.TotalTeams = TotalTeams;

                ViewBag.TournamentId = id;


                return View(model);
            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("Index");
            }
            
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult EditTournamentTeams(int Teamid , int TournamentId)
        {
            try
            {
                var MatchesPlayed = _repoTournamentTeam.FindTeamByTournamentIdandTeamId(Teamid ,TournamentId).MatchesPlayed;
                var MatchesWon = _repoTournamentTeam.FindTeamByTournamentIdandTeamId(Teamid, TournamentId).MatchesWon;
                var MatchesLost = _repoTournamentTeam.FindTeamByTournamentIdandTeamId(Teamid, TournamentId).MatchesLost;
                var MatchesDraw = _repoTournamentTeam.FindTeamByTournamentIdandTeamId(Teamid, TournamentId).MatchesDrawed;

                var MaximumMatchesPlayed = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == TournamentId).Max(m => m.MatchesPlayed);

                var MaximumMatchesPlayedTeam = _repoTournamentTeam.FindAll().Where(q => q.TournamentId == TournamentId).OrderByDescending(q => q.MatchesPlayed).DefaultIfEmpty().First().Team.TeamName;

                //View Bags
                ViewBag.MaximumMatchesPlayed = MaximumMatchesPlayed;


                ViewBag.MaximumMatchesPlayedTeam = MaximumMatchesPlayedTeam;

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
        [Authorize(Roles = "Administrator")]
        public IActionResult EditTournamentTeams(TournamentTeamVM model)
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
            }
            else
            {
                _notyf.Error("Something Went Wrong!");
                return RedirectToAction("EditTournamentTeams", new { id = model.TeamId });
            }

            _notyf.Success("Team stats for this tournament are updated");
            return RedirectToAction("EditTournamentTeams", new { Teamid = model.TeamId , TournamentId = model.TournamentId });
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult RemoveTeamFromTournament(int id)
        {
            var TeamFromTournament = _repoTournamentTeam.FindbyId(id);
            var TournamentId = TeamFromTournament.TournamentId;

            if (TeamFromTournament == null)
            {
                _notyf.Error("Team Does not Exist");
                return NotFound();
            }

            _repoTournamentTeam.Delete(TeamFromTournament);
            _repoTournamentTeam.Save();

            _notyf.Success("Team deleted successfully from the tournament");
            return RedirectToAction("ManageTournamentTeam", new { id = TournamentId });
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult ParticipatedTournaments(int id)
        {
            try
            {
                var TeamName = _repo.FindbyId(id).TeamName;
                var TeamId = _repo.FindbyId(id).TeamId;

                @ViewBag.TeamId = TeamId;

                if(TeamName != null)
                {
                    @ViewBag.TeamName = TeamName;
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
                var TeamId = _repo.FindbyId(id).TeamId;
                _notyf.Error(e.Message);
                return RedirectToAction("EditTeam", new { id = TeamId });
            }
            
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteTeam(int id)
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

                _notyf.Custom("Team is deleted permanently, this action is not reversible" , 30 , "#29a847", "fas fa-exclamation-triangle");
                _notyf.Information("Admin Deleted team with ID: " + team.TeamId + " successfully ", 20);
                _notyf.Information("Team Name: " + team.TeamName, 10);
                _notyf.Information("Team Level: " + team.TeamLevel, 7);

            }
            else
            {
                _notyf.Error("Team does not exist", 20);
            }

            return RedirectToAction("TeamList");
        }

        [Authorize]
        public IActionResult MyTeams()
        {
            //Get Logged in User
            var Player = _userManager.GetUserAsync(User).Result;

            var PlayerName = Player.FirstName + " " + Player.LastName;
            var PlayerRollNo = Player.RollNumber;

            if (PlayerRollNo != null)
            {
                ViewBag.PlayerRollNo = PlayerRollNo;
            }

            if (PlayerName != null)
            {
                ViewBag.PlayerName = PlayerName;
            }

            var Teams = _repoPlayer.FindAll().Where(q => q.PlayerId == Player.Id);
            var model = _mapper.Map<List<TeamPlayerVM>>(Teams);
            return View(model);
        }

        [Authorize]
        //Show player what tournament their team is part of
        public IActionResult TeaminTournament(int id, TeamTournamentFilterVM model)
        {
            try
            {
                //Get Team
                var Team = _repo.FindbyId(id);

                var TeamName = Team.TeamName;
                var TeamDepartment = Team.DepartmentName;

                if (TeamName != null)
                {
                    ViewBag.TeamName = TeamName;
                }

                if (TeamDepartment != null)
                {
                    ViewBag.TeamDepartment = TeamDepartment;
                }

                //If date filter is being used display filtered values else show unfiltered value
                if(model.StartDate.Year != 1)
                {

                    //Get the list of tournaments this team is part of
                    var TournamentListFilter = _repoTournamentTeam.FindAll().Where(q => q.TeamId == id && (q.Tournament.EndDate >= model.StartDate && q.Tournament.EndDate <= model.EndDate));
                    var TournamentMapped = _mapper.Map<List<TournamentTeamVM>>(TournamentListFilter);


                    var TournamentFilterModel = new TeamTournamentFilterVM
                    {
                        EndDate = model.EndDate,
                        StartDate = model.StartDate,
                        TournamentTeamList = TournamentMapped
                    };

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

                    return View(TournamentFilterModel);
                }
                else
                {
                    //Get Tournament List this team is part of
                    var TournamentList = _repoTournamentTeam.FindAll().Where(q => q.TeamId == id);
                    var TournamentListModel = _mapper.Map<List<TournamentTeamVM>>(TournamentList);

                    var TournamentFilterModel = new TeamTournamentFilterVM
                    {
                        EndDate = model.EndDate,
                        StartDate = model.StartDate,
                        TournamentTeamList = TournamentListModel
                    };

                    //Get Count of Matches
                    if (TournamentList != null)
                    {
                        var TotalMatchesPlayed = TournamentList.Sum(q => q.MatchesPlayed);
                        var TotalMatchesWon = TournamentList.Sum(q => q.MatchesWon);
                        var TotalMatchesLost = TournamentList.Sum(q => q.MatchesLost);
                        var TotalMatchesDrawed = TournamentList.Sum(q => q.MatchesDrawed);

                        ViewBag.TotalMatchesPlayed = TotalMatchesPlayed;
                        ViewBag.TotalMatchesWon = TotalMatchesWon;
                        ViewBag.TotalMatchesLost = TotalMatchesLost;
                        ViewBag.TotalMatchesDrawed = TotalMatchesDrawed;
                    }
                    return View(TournamentFilterModel);
                }

            }
            catch (Exception e)
            {
                _notyf.Error(e.Message);
                return RedirectToAction("MyTeams");
            }


            ////Get Team
            //var Team = _repo.FindbyId(id);

            //var TeamName = Team.TeamName;
            //var TeamDepartment = Team.DepartmentName;

            //if (TeamName != null)
            //{
            //    ViewBag.TeamName = TeamName;
            //}

            //if (TeamDepartment != null)
            //{
            //    ViewBag.TeamDepartment = TeamDepartment;
            //}

            ////Get Tournament List this team is part of
            //var TournamentList = _repoTournamentTeam.FindAll().Where(q => q.TeamId == id);
            //var model = _mapper.Map<List<TournamentTeamVM>>(TournamentList);
            //return View(model);
        }
    }
}
