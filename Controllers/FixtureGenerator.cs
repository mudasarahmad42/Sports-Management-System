using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class FixtureGenerator : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AllPlayAll()
        {
            return View();
        }

        public IActionResult DoubleRoundRobin()
        {
            return View();
        }

        public IActionResult Knockout()
        {
            return View();
        }
    }
}
