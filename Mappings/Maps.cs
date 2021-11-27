using AutoMapper;
using GCUSMS.Models;
using GCUSMS.ViewModels;

namespace GCUSMS.Mappings
{
    public class Maps : Profile
    {
        public Maps()
        {
            CreateMap<GalleryModel, GalleryVM>().ReverseMap();
            CreateMap<LeaveApplicationModel, LeaveApplicationVM>().ReverseMap();
            CreateMap<StudentModel, StudentVM>().ReverseMap();
            CreateMap<EquipmentModel, EquipmentVM>().ReverseMap();
            CreateMap<EquipmentAllocationModel, EquipmentAllocationVM>().ReverseMap();
            CreateMap<StudentDetailVM, StudentModel>().ReverseMap();
            CreateMap<TournamentVM, TournamentModel>().ReverseMap();
            CreateMap<ParticipantVM, ParticipantModel>().ReverseMap();
            CreateMap<BlogVM, BlogModel>().ReverseMap();
            CreateMap<EditBlogVM, BlogModel>().ReverseMap();
            CreateMap<CommentVM, CommentModel>().ReverseMap();
            CreateMap<TeamVM, TeamModel>().ReverseMap();
            CreateMap<EditTeamVM, TeamModel>().ReverseMap();
            CreateMap<CreateTeamVM, TeamModel>().ReverseMap();
            CreateMap<TeamPlayerVM, TeamPlayerModel>().ReverseMap();
            CreateMap<EditTeamPlayerVM, TeamPlayerModel>().ReverseMap();
            CreateMap<TournamentTeamVM, TournamentTeamModel>().ReverseMap();
            CreateMap<CkEditorImagesVM, CkEditorImagesModel>().ReverseMap();
            CreateMap<FeedbackVM, FeedbackModel>().ReverseMap();
        }

    }
}
