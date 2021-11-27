using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.IO;
using GCUSMS.Contracts;
using GCUSMS.Data;
using GCUSMS.Mappings;
using GCUSMS.Models;
using GCUSMS.Repository;

namespace GCUSMS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            //Repository References
            services.AddScoped<ILeaveApplicationRepository, LeaveApplicationRepository>();
            services.AddScoped<IEquipmentRepository, EquipmentRepository>();
            services.AddScoped<IEquipmentAllocationRepository, EquipmentAllocationRepository>();
            services.AddScoped<ITournamentRepository, TournamentRepository>();
            services.AddScoped<IParticipantRepository, ParticipantRepository>();
            services.AddScoped<IBlogRepository, BlogRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<ITeamPlayerRepository, TeamPlayerRepository>();
            services.AddScoped<ITournamentTeamRepository, TournamentTeamRepository>();
            services.AddScoped<ICkEditorImagesRepository, CkEditorImagesRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            
            //Notifications
            services.AddNotyf(config => { config.DurationInSeconds = 5; config.IsDismissable = true; config.Position = NotyfPosition.TopRight; });
            //services.AddToastify(config => { config.DurationInSeconds = 5; config.Position = Position.Right; config.Gravity = Gravity.Top; });
            
            //AutoMapper
            services.AddAutoMapper(typeof(Maps));
            
            services.AddDefaultIdentity<StudentModel>(options => {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("MakeCaptain",
                    policy => policy.RequireClaim("Make Captain"));
            });

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            UserManager<StudentModel> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            SeedData.Seed(userManager, roleManager);

            //Notification
            app.UseNotyf();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
