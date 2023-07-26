using AutoMapper;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Mappers
{
    public class MapperConfig
    {
        public static Mapper InitializeAutomapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<RoleRequest, AspNetRole>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.Name.ToUpper()));
            });
            var mapper = new Mapper(config);
            return mapper;
        }
    }
}
