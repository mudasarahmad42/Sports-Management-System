using Microsoft.AspNetCore.Identity;
using GCUSMS.Models;

namespace GCUSMS
{
    public static class SeedData
    {
        public static void Seed(UserManager<StudentModel> userManager, RoleManager<IdentityRole> roleManager)
        {
            SeedRoles(roleManager);
            SeedUsers(userManager);
        }

        private static void SeedUsers(UserManager<StudentModel> userManager)
        {

            if (userManager.FindByNameAsync("Admin").Result == null)
            {
                var user = new StudentModel
                {
                    UserName = "admin@gcu.edu.pk",
                    Email = "admin@gcu.edu.pk",
                    FirstName = "Admin",
                    ProfileImagePath = "admin.jpg"
                };

                var result = userManager.CreateAsync(user, "P@ssword1").Result;

                if(result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Administrator").Wait();
                }
            }
        }

        private static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExistsAsync("Administrator").Result)
            {
                var role = new IdentityRole
                {
                    Name = "Administrator"
                };

                var result = roleManager.CreateAsync(role).Result;
            }

            if (!roleManager.RoleExistsAsync("Student").Result)
            {
                var role = new IdentityRole
                {
                    Name = "Student"
                };

                var result = roleManager.CreateAsync(role).Result;
            }

            if (!roleManager.RoleExistsAsync("HODS").Result)
            {
                var role = new IdentityRole
                {
                    Name = "HODS"
                };

                var result = roleManager.CreateAsync(role).Result;
            }
        }
    }
}
