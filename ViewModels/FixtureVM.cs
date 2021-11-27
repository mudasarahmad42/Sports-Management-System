using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GCUSMS.ViewModels
{
    public class FixtureVM
    {
        public int NumberOfTeams { get; set; }
        public int[,] ArrayOfFixtures { get; set; }
    }
}
