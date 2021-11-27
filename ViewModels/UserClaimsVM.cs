using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GCUSMS.Models;

namespace GCUSMS.ViewModels
{
    public class UserClaimsVM
    {
        public UserClaimsVM()
        {
            Claims = new List<UserClaims>();
        }
        public string UserId { get; set; }
        public List<UserClaims> Claims { get; set; }
    }
}
