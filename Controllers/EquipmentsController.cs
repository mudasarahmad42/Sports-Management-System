using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GCUSMS.Contracts;
using GCUSMS.Data;
using GCUSMS.Models;
using GCUSMS.ViewModels;

namespace GCUSMS.Controllers
{
    [Authorize]
    public class EquipmentsController : Controller
    {

        private readonly IEquipmentRepository _repo;
        private readonly IMapper _mapper;
        private readonly INotyfService _notyf;
        private readonly IEquipmentAllocationRepository _repoEquipmentAllocation;
        private readonly UserManager<StudentModel> _userManager;
        private readonly ApplicationDbContext _db;

        public EquipmentsController
        (
            IEquipmentRepository repo,
            IMapper mapper,
            INotyfService notyf,
            IEquipmentAllocationRepository repoEquipmentAllocation,
            UserManager<StudentModel> userManager,
            ApplicationDbContext db
        )
        {
            _repo = repo;
            _mapper = mapper;
            _notyf = notyf;
            _repoEquipmentAllocation = repoEquipmentAllocation;
            _userManager = userManager;
            _db = db;
        }

        [Authorize(Roles = "Administrator")]
        // GET: EquipmentsController
        public ActionResult Index()
        {
            var EquipmentList = _repo.FindAll();
            var EquipmentListModel = _mapper.Map<List<EquipmentVM>>(EquipmentList);
            return View(EquipmentListModel);
        }

        [Authorize(Roles = "Administrator")]
        // GET: Equipments/Allocations/5
        public ActionResult Allocations(int id)
        {
            var EqAllocations = _repoEquipmentAllocation.FindAll().Where(q => q.RequestedEquipmentId == id);
            var model = _mapper.Map<List<EquipmentAllocationVM>>(EqAllocations);
            return View(model);
        }

        [Authorize(Roles = "Administrator")]
        // GET: Equipments/StudentAllocations
        public ActionResult StudentAllocations(string id)
        {
            var IdExists = _repoEquipmentAllocation.FindAll().Where(q => q.RequestingStudentId == id).Any();

            //PASSING DATA VIA VIEWBAG
            if (IdExists)
            {
                var StudentName = _repoEquipmentAllocation.FindAll().FirstOrDefault(q => q.RequestingStudentId == id).RequestingStudent.FirstName;

                var StudentId = _repoEquipmentAllocation.FindAll().FirstOrDefault(q => q.RequestingStudentId == id).RequestingStudentId;

                if (StudentName != null)
                {
                    ViewBag.StudentName = StudentName;
                }

                if (StudentId != null)
                {
                    ViewBag.StudentId = StudentId;
                }

                var EqAllocations = _repoEquipmentAllocation.FindAll().Where(q => q.RequestingStudentId == id);
                var model = _mapper.Map<List<EquipmentAllocationVM>>(EqAllocations);
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
        // GET: LeaveRequest/Create
        public IActionResult Allocate(int id)
        {
            var IdExists = _repo.FindAll().Where(q => q.Id == id).Any();

            //PASSING DATA VIA VIEWBAG
            if (IdExists)
            {


                var EquipmentName = _repo.FindbyId(id).EquipmentName;
                var EquipmentType = _repo.FindbyId(id).EquipmentType;
             
                if (EquipmentName != null)
                {
                    ViewBag.EquipmentName = EquipmentName;
                }

                if (EquipmentType != null)
                {
                    ViewBag.EquipmentType = EquipmentType;
                }

                ViewBag.EquipmentID = id;

                ViewBag.AvailableEquipment = _repo.FindbyId(id).Quantity;


                return View();
            }
            else
            {
                _notyf.Information("Record with this ID does not exist");
                _notyf.Warning("Entering ID via URL is not recommended",12);
                return View();
            }     
        }

        [Authorize(Roles = "Administrator")]
        // POST: EquipmentsController/Allocations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Allocate(EquipmentAllocationVM model, int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _notyf.Error("Something Went Wrong!!");
                    return RedirectToAction("Allocate");
                }

                if (model.RequestingStudentId == null)
                {
                    _notyf.Error("Roll number does not exist");
                    return RedirectToAction("Allocate");
                }

                //Date Variables
                var startDate = model.StartDate;
                var endDate = model.EndDate;
                int daysRequested = (int)(endDate - startDate).TotalDays;


                //var student = _userManager.GetUserAsync(User).Result;

                //Update Quantity
                var QuantityAllocated = model.QuantityAllocated;
                var QuantityAvailable = _repo.FindbyId(id).Quantity;
                var newQuantity = QuantityAvailable - QuantityAllocated;

                //If new quantity is less than zero 
                //check if the any stock is available to allocate
                if (QuantityAvailable == 0)
                {
                    _notyf.Warning("No Item is left to allocate!", 10);
                    return RedirectToAction("Allocate");
                }

                if (newQuantity < 0)
                {
                    //Remove it if you want to Availablity is already checked
                    //If new quantity is less than zero 
                    //check if the any stock is available to allocate
                    if(QuantityAvailable == 0)
                    {
                        _notyf.Warning("No Item is left to allocate!" , 10);
                        return RedirectToAction("Allocate");
                    }

                    _notyf.Warning("Only " + QuantityAvailable + " Items are available" , 10);
                    return RedirectToAction("Allocate");
                }



                if (startDate < DateTime.Today)
                {
                    _notyf.Warning("Enter a Valid start Date");
                    return RedirectToAction("Allocate");
                }


                if (daysRequested < 0)
                {
                    _notyf.Warning("Start Date should be less than Ending Date");
                    ModelState.AddModelError("", "Start Date cannot be further in the future than the End Date");
                    return RedirectToAction("Allocate");
                }

                var EquipmentUpdate = _repo.FindbyId(id);

                EquipmentUpdate.Quantity = newQuantity;

                var isSuccess = _repo.Update(EquipmentUpdate);


                var EquipmentAllocationModel = new EquipmentAllocationVM
                {
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    AllocatedBy = model.AllocatedBy,
                    DateAllocated = DateTime.Now,
                    QuantityAllocated = model.QuantityAllocated,
                    RequestingStudentId = model.RequestingStudentId,
                    RequestedEquipmentId = id     
                };

                //Code Edited
                ////RequestedEquipmentId = model.RequestedEquipmentId
                //
                var equipmentAllocation = _mapper.Map<EquipmentAllocationModel>(EquipmentAllocationModel);
                _repoEquipmentAllocation.Create(equipmentAllocation);
                _repoEquipmentAllocation.Save();

                _notyf.Success( EquipmentUpdate.EquipmentName +" Allocated Succesfully!");
                return RedirectToAction("Allocate");

            }
            catch (Exception)
            {
                return RedirectToAction("Allocate");
                throw;
            }
            
        }

        //////////////////////////////////// AJAX ////////////////////////////////////
        

        [Authorize(Roles = "Administrator")]
        public IActionResult Test()
        {
            //provides suggestions while you type into the field
            var number = HttpContext.Request.Query["term"].ToString();
            var RollNumber = _userManager.Users.Where(c => c.RollNumber.Contains(number)).Select(c => c.RollNumber).ToList();
            return Ok(RollNumber);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult TestForEquipment()
        {
            //provides suggestions while you type into the field
            var number = HttpContext.Request.Query["term"].ToString();
            var EquipmentIdNumber = _repo.FindAll().Where(c => c.Id.ToString().Contains(number)).Select(c => c.Id).ToList();
            return Ok(EquipmentIdNumber);
        }


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

        //////////// Equipments Ajax /////////////////////

        //[HttpGet]
        //public JsonResult FillEquipmentName(int EqId)
        //{
        //    //fill input fields when you select RollNumber
        //    var Name = _repo.FindbyId(EqId).RequestedEquipment.EquipmentName.ToList();
        //    return new JsonResult(Name);
        //}

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public JsonResult FillEquipmentType(int EqId)
        {
            //fill input fields when you select RollNumber
            var Type = _repo.FindbyId(EqId).EquipmentType.ToString();
            return new JsonResult(Type);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public JsonResult FillEquipmentName(int EqId)
        {
            //fill input fields when you select RollNumber
            var Name = _repo.FindbyId(EqId).EquipmentName.ToString();
            return new JsonResult(Name);
        }



        /////////// AJAX /////////////////

        [Authorize(Roles = "Administrator")]
        // GET: EquipmentsController/Create
        public ActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        // POST: EquipmentsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EquipmentVM model)
        {
            try
            {
                var Date = DateTime.Now;

                var EquipmentModel = new EquipmentVM
                {
                    EquipmentName = model.EquipmentName,
                    EquipmentType = model.EquipmentType,
                    Condition = model.Condition,
                    Quantity = model.Quantity,
                    Description = model.Description,
                    DateEntered = Date
                };

                var Equipment = _mapper.Map<EquipmentModel>(EquipmentModel);
                _repo.Create(Equipment);
                _repo.Save();

                _notyf.Success("Record Entered Successfully");
                return RedirectToAction("Create");
            }
            catch
            {
                ModelState.AddModelError("", "Something went wrong");
                _notyf.Warning("Something went wrong");
                return View(model);
            }
        }

        [Authorize(Roles = "Administrator")]
        // GET: EquipmentsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        // POST: EquipmentsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [Authorize(Roles = "Administrator")]
        // GET: EquipmentsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        // POST: EquipmentsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult AllocatedEquipmentDetail(int id)
        {
            var EquipmentAllocationDetail = _repoEquipmentAllocation.FindbyId(id);
            var EquipmentModel = _mapper.Map<EquipmentAllocationVM>(EquipmentAllocationDetail);

            ViewData["EquipmentAllocated"] = EquipmentAllocationDetail.QuantityAllocated;

            return View(EquipmentModel);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult MarkReturn(int id, EquipmentAllocationVM model)
        {
            try
            {
                //Find the Equipment
                var EquipmentAllocations = _repoEquipmentAllocation.FindbyId(id);
                EquipmentAllocations.Returned = true;
                EquipmentAllocations.DateReturned = DateTime.Now;

                //Find the Quantity Allocated to this entry
                var QuantityAllocated = _repoEquipmentAllocation.FindbyId(id).QuantityAllocated;

                //Make Changes to Quantity
                var EquipmentUpdate = _repo.FindbyId(EquipmentAllocations.RequestedEquipmentId);

                if(model.QuantityAccepted > 0 && model.QuantityAccepted <= QuantityAllocated)
                {
                    EquipmentUpdate.Quantity += model.QuantityAccepted;
                }
                else
                {
                    _notyf.Error("Please enter returned quantity carefully!");
                    return RedirectToAction("AllocatedEquipmentDetail", new { id = EquipmentAllocations.Id });
                }

                //Update Entry in Table
                var AllocationEntry = _repoEquipmentAllocation.FindbyId(id);
                AllocationEntry.QuantityAccepted = model.QuantityAccepted;
                AllocationEntry.Comments = model.Comments;


                var isSuccessInventory = _repo.Update(EquipmentUpdate);
                var isSuccessReturned = _repoEquipmentAllocation.Update(EquipmentAllocations);

                _notyf.Success("Equipment is Returned");
                return RedirectToAction("AllocatedEquipmentDetail", new { id = EquipmentAllocations.Id });
            }
            catch (Exception)
            {
                return RedirectToAction("AllocatedEquipmentDetail");
            }
        }


        //
        /// <summary>
        /// These methods were used before to mark equipment as returned or not returned
        /// but they were returning it in the same amount as it was allocated.
        /// New method is written now that takes input of accepted equipment and only add them back to the 
        /// inventory. See MarkReturn()
        /// </summary>
        /// <returns></returns>
        //[Authorize(Roles = "Administrator")]
        //public IActionResult ItemReturned(int id)
        //{
        //    try
        //    {
        //        var EquipmentAllocations = _repoEquipmentAllocation.FindbyId(id);
        //        EquipmentAllocations.Returned = true;
        //        EquipmentAllocations.DateReturned = DateTime.Now;


        //        var QuantityAllocated = _repoEquipmentAllocation.FindbyId(id).QuantityAllocated;

        //        var EquipmentUpdate = _repo.FindbyId(EquipmentAllocations.RequestedEquipmentId);
        //        EquipmentUpdate.Quantity += QuantityAllocated;

        //        var isSuccessInventory = _repo.Update(EquipmentUpdate);
        //        var isSuccessReturned = _repoEquipmentAllocation.Update(EquipmentAllocations);

        //        _notyf.Success("Equipment is Returned");
        //        return RedirectToAction("AllocatedEquipmentDetail" , new { id = EquipmentAllocations.Id });
        //    }
        //    catch (Exception)
        //    {
        //        return RedirectToAction("AllocatedEquipmentDetail");
        //    }
        //}

        [Authorize(Roles = "Administrator")]
        public IActionResult ItemNotReturned(int id)
        {
            try
            {
                var EquipmentAllocations = _repoEquipmentAllocation.FindbyId(id);
                EquipmentAllocations.Returned = false;
                EquipmentAllocations.DateReturned = DateTime.Now;

                var isSuccessReturned = _repoEquipmentAllocation.Update(EquipmentAllocations);

                _notyf.Error("Equipment is Not Returned");
                _notyf.Warning("This allocation is now considered lost", 5);
                return RedirectToAction("AllocatedEquipmentDetail", new { id = EquipmentAllocations.Id });
            }
            catch (Exception)
            {
                return RedirectToAction("AllocatedEquipmentDetail");
            }
        }

        [Authorize]
        public IActionResult StudentEquipmentList()
        {
            var Student = _userManager.GetUserAsync(User).Result;
            var StudentId = Student.Id;

            var Equipment = _repoEquipmentAllocation.FindAll().Where(q => q.RequestingStudentId == StudentId);

            var EquipmentModel = _mapper.Map<List<EquipmentAllocationVM>>(Equipment);

            var model = new StudentEquipmentAllocationVM
            {
                 TotalEquipmentAllocationss = EquipmentModel.Count(),
                ReturnedEquipment = EquipmentModel.Count(q => q.Returned == true),
                LostEquipment = EquipmentModel.Count(q => q.Returned == false),
                PendingEquipment = EquipmentModel.Count(q => q.Returned == null),
                EquipmentAllocations = EquipmentModel,
                RequestingStudentId = StudentId,
                RequestingStudent = Student,
            };
            return View(model);
        }

        [Authorize]
        public IActionResult StudentEquipmentDetails(int id)
        {
            try
            {
                var Equipment = _repoEquipmentAllocation.FindbyId(id);
                var model = _mapper.Map<EquipmentAllocationVM>(Equipment);

                var StudentId = Equipment.RequestingStudentId;

                ViewBag.TotalAllocations = _repoEquipmentAllocation.GetEquipmentAllocationByStudentID(StudentId).Count();
                ViewBag.TotalReturnedEquipment = _repoEquipmentAllocation.GetEquipmentAllocationByStudentID(StudentId).Where(q => q.Returned == true).Count();
                ViewBag.TotalLostEquipment = _repoEquipmentAllocation.GetEquipmentAllocationByStudentID(StudentId).Where(q => q.Returned == false).Count();
                ViewBag.TotalPendingEquipment = _repoEquipmentAllocation.GetEquipmentAllocationByStudentID(StudentId).Where(q => q.Returned == null).Count();


                return View(model);
            }
            catch (Exception)
            {
                _notyf.Error("Requested Id does not exist");
                return RedirectToAction("StudentEquipmentList");
            }

        }


        [Authorize(Roles = "Administrator")]
        public IActionResult DeleteEquipment(int id)
        {
            //Getting the equipment
            var equipment = _repo.FindbyId(id);

            if (equipment != null)
            {
                //Getting the Equipment allocation record of this equipment
                var equipmentAllocationsList = _repoEquipmentAllocation.FindAll().Where(c => c.RequestedEquipmentId == id);
                var NumberofEqAllocations = equipmentAllocationsList.Count();

                if (NumberofEqAllocations != 0)
                {
                    var allocationCounter = NumberofEqAllocations;

                    do
                    {
                        var equipmentAllocations = _repoEquipmentAllocation.FindAll().FirstOrDefault(c => c.RequestedEquipmentId == id);
                        if (equipmentAllocations != null)
                        {
                            var resultEqAllocations = _repoEquipmentAllocation.Delete(equipmentAllocations);
                        }
                        allocationCounter--;
                    }
                    while (allocationCounter != 0);
                }

                //Delete Equipment Now
                var resultEquipment = _repo.Delete(equipment);
                _repo.Save();

                _notyf.Information("Admin Deleted equipment with ID: " + id + " successfully ", 20);
                _notyf.Information("Equipment Name " + equipment.EquipmentName, 10);
                _notyf.Information("Quantity: " + equipment.Quantity, 7);
            }
            else
            {
                _notyf.Error("Equipment does not exist does not exist", 20);
            }

            return RedirectToAction("Index");
        }
    }
}
