using AutoMapper;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Requests;

namespace IWParkingAPI.Mappers
{
    public class MapperConfig
    {
        public static Mapper InitializeAutomapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<RoleRequest, AspNetRole>();
            });
            var mapper = new Mapper(config);
            return mapper;
        }
    }
}
