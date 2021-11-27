using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GCUSMS.Models;

namespace GCUSMS.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<GalleryModel> Images { get; set; }
        public DbSet<StudentModel> Students { get; set; }
        public DbSet<LeaveApplicationModel> LeaveApplications { get; set; }
        public DbSet<EquipmentModel> Equipments { get; set; }
        public DbSet<EquipmentAllocationModel> EquipmentAllocations { get; set; }
        public DbSet<TournamentModel> Tournaments { get; set; }
        public DbSet<ParticipantModel> Participants { get; set; }
        public DbSet<BlogModel> Blogs { get; set; }
        public DbSet<CommentModel> Comments { get; set; }
        public DbSet<TeamModel> Teams { get; set; }
        public DbSet<TeamPlayerModel> TeamPlayer { get; set; }
        public DbSet<TournamentTeamModel> TournamentTeam { get; set; }
        public DbSet<CkEditorImagesModel> CkEditorImages { get; set; }
        public DbSet<FeedbackModel> Feedbacks { get; set; }

    }
}
