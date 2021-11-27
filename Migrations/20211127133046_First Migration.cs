using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GCUSMS.Migrations
{
    public partial class FirstMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    FatherName = table.Column<string>(nullable: true),
                    DateOfBirth = table.Column<DateTime>(nullable: true),
                    Age = table.Column<int>(nullable: true),
                    RollNumber = table.Column<string>(nullable: true),
                    Section = table.Column<string>(nullable: true),
                    Session = table.Column<string>(nullable: true),
                    Semester = table.Column<string>(nullable: true),
                    DepartmentName = table.Column<string>(nullable: true),
                    Gender = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: true),
                    ProfileImagePath = table.Column<string>(nullable: true),
                    PasswordResetCount = table.Column<int>(nullable: true),
                    LastPasswordReset = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Equipments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipmentName = table.Column<string>(nullable: false),
                    EquipmentType = table.Column<string>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    Condition = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    DateEntered = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    FeedbackID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    FeedbackMessage = table.Column<string>(nullable: true),
                    DateSubmitted = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.FeedbackID);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    ImageId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageName = table.Column<string>(nullable: false),
                    UploadedOn = table.Column<DateTime>(nullable: false),
                    DepartmentName = table.Column<string>(nullable: true),
                    Tournament = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    ImagePath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.ImageId);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    TeamId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamName = table.Column<string>(nullable: true),
                    MaximumPlayers = table.Column<int>(nullable: false),
                    TeamSports = table.Column<string>(nullable: true),
                    TeamLevel = table.Column<string>(nullable: true),
                    DepartmentName = table.Column<string>(nullable: true),
                    LogoImagePath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.TeamId);
                });

            migrationBuilder.CreateTable(
                name: "Tournaments",
                columns: table => new
                {
                    TournamentId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentName = table.Column<string>(nullable: false),
                    TournamentVenue = table.Column<string>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    DueDate = table.Column<DateTime>(nullable: false),
                    TeamsAllowed = table.Column<int>(nullable: false),
                    isActive = table.Column<bool>(nullable: false),
                    isIntraDepartmental = table.Column<bool>(nullable: false),
                    DepartmentName = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Winner = table.Column<string>(nullable: true),
                    RunnerUp = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    PointsForWinning = table.Column<int>(nullable: false),
                    PointsForLosing = table.Column<int>(nullable: false),
                    PointsForDraw = table.Column<int>(nullable: false),
                    SportsCategory = table.Column<string>(nullable: true),
                    GenderAllowed = table.Column<string>(nullable: true),
                    TournamentType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => x.TournamentId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    BlogID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorId = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: false),
                    Content = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    isPublished = table.Column<bool>(nullable: false),
                    isApproved = table.Column<bool>(nullable: false),
                    EditedBy = table.Column<string>(nullable: true),
                    Excerpt = table.Column<string>(nullable: true),
                    Category = table.Column<string>(nullable: true),
                    BlogImagePath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.BlogID);
                    table.ForeignKey(
                        name: "FK_Blogs_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CkEditorImages",
                columns: table => new
                {
                    CkEditorImageId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CkEditorAuthorId = table.Column<string>(nullable: true),
                    CkEditorImagePath = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CkEditorImages", x => x.CkEditorImageId);
                    table.ForeignKey(
                        name: "FK_CkEditorImages_AspNetUsers_CkEditorAuthorId",
                        column: x => x.CkEditorAuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveApplications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    DateRequested = table.Column<DateTime>(nullable: false),
                    DateActioned = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    Approved = table.Column<bool>(nullable: true),
                    LeaveSubject = table.Column<string>(nullable: true),
                    RequestingStudentId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveApplications_AspNetUsers_RequestingStudentId",
                        column: x => x.RequestingStudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    AllocatedBy = table.Column<string>(nullable: true),
                    QuantityAllocated = table.Column<int>(nullable: false),
                    DateAllocated = table.Column<DateTime>(nullable: false),
                    Returned = table.Column<bool>(nullable: true),
                    DateReturned = table.Column<DateTime>(nullable: false),
                    Comments = table.Column<string>(nullable: true),
                    QuantityAccepted = table.Column<int>(nullable: false),
                    RequestingStudentId = table.Column<string>(nullable: true),
                    RequestedEquipmentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentAllocations_Equipments_RequestedEquipmentId",
                        column: x => x.RequestedEquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentAllocations_AspNetUsers_RequestingStudentId",
                        column: x => x.RequestingStudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamPlayer",
                columns: table => new
                {
                    TeamPlayerId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerWeight = table.Column<string>(nullable: true),
                    PlayerHeight = table.Column<string>(nullable: true),
                    PlayerFitnessPoints = table.Column<int>(nullable: false),
                    PlayerRole = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CaptainType = table.Column<string>(nullable: true),
                    PlayerId = table.Column<string>(nullable: true),
                    TeamId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamPlayer", x => x.TeamPlayerId);
                    table.ForeignKey(
                        name: "FK_TeamPlayer_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamPlayer_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    ParticipantId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateApplied = table.Column<DateTime>(nullable: false),
                    DateSelected = table.Column<DateTime>(nullable: false),
                    isSelected = table.Column<bool>(nullable: true),
                    RequestingStudentId = table.Column<string>(nullable: true),
                    AppliedTournamentId = table.Column<int>(nullable: false),
                    ValidEmail = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.ParticipantId);
                    table.ForeignKey(
                        name: "FK_Participants_Tournaments_AppliedTournamentId",
                        column: x => x.AppliedTournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participants_AspNetUsers_RequestingStudentId",
                        column: x => x.RequestingStudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TournamentTeam",
                columns: table => new
                {
                    TournamentTeamId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchesPlayed = table.Column<int>(nullable: false),
                    MatchesWon = table.Column<int>(nullable: false),
                    MatchesLost = table.Column<int>(nullable: false),
                    MatchesDrawed = table.Column<int>(nullable: false),
                    TotalPoints = table.Column<int>(nullable: false),
                    TournamentId = table.Column<int>(nullable: false),
                    TeamId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentTeam", x => x.TournamentTeamId);
                    table.ForeignKey(
                        name: "FK_TournamentTeam_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentTeam_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlogID = table.Column<int>(nullable: false),
                    CommenterID = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    CommentedOn = table.Column<DateTime>(nullable: false),
                    CommentModelId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Blogs_BlogID",
                        column: x => x.BlogID,
                        principalTable: "Blogs",
                        principalColumn: "BlogID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Comments_CommentModelId",
                        column: x => x.CommentModelId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_AspNetUsers_CommenterID",
                        column: x => x.CommenterID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_AuthorId",
                table: "Blogs",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_CkEditorImages_CkEditorAuthorId",
                table: "CkEditorImages",
                column: "CkEditorAuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_BlogID",
                table: "Comments",
                column: "BlogID");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CommentModelId",
                table: "Comments",
                column: "CommentModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CommenterID",
                table: "Comments",
                column: "CommenterID");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentAllocations_RequestedEquipmentId",
                table: "EquipmentAllocations",
                column: "RequestedEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentAllocations_RequestingStudentId",
                table: "EquipmentAllocations",
                column: "RequestingStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_RequestingStudentId",
                table: "LeaveApplications",
                column: "RequestingStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_AppliedTournamentId",
                table: "Participants",
                column: "AppliedTournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_RequestingStudentId",
                table: "Participants",
                column: "RequestingStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamPlayer_PlayerId",
                table: "TeamPlayer",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamPlayer_TeamId",
                table: "TeamPlayer",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentTeam_TeamId",
                table: "TournamentTeam",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentTeam_TournamentId",
                table: "TournamentTeam",
                column: "TournamentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CkEditorImages");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "EquipmentAllocations");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "LeaveApplications");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "TeamPlayer");

            migrationBuilder.DropTable(
                name: "TournamentTeam");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Equipments");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Tournaments");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
