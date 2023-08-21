using AutoMapper;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses;

namespace IWParkingAPI.Mappers
{
    public class MapperConfig
    {
        public static Mapper InitializeAutomapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<RoleRequest, ApplicationRole>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.Name.ToUpper()));

                cfg.CreateMap<UserRequest, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));

                cfg.CreateMap<UserRegisterRequest, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone));

                cfg.CreateMap<VehicleRequest, Vehicle>()
               //.ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
               .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber));
                //   .ForSourceMember(src => src.Type, opt => opt.DoNotValidate())
                //   .ForSourceMember(src => src.PlateNumber, opt => opt.DoNotValidate()); ;

                cfg.CreateMap<Vehicle, VehicleDTO>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
              .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber))
              .ForMember(dest => dest.TimeCreated, opt => opt.MapFrom(src => src.TimeCreated))
              .ForMember(dest => dest.TimeModified, opt => opt.MapFrom(src => src.TimeModified))
              .ForMember(dest => dest.IsPrimary, opt => opt.MapFrom(src => src.IsPrimary))
              .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

                cfg.CreateMap<Vehicle, VehicleWithoutUserDTO>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
              .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber))
              .ForMember(dest => dest.TimeCreated, opt => opt.MapFrom(src => src.TimeCreated))
              .ForMember(dest => dest.TimeModified, opt => opt.MapFrom(src => src.TimeModified))
              .ForMember(dest => dest.IsPrimary, opt => opt.MapFrom(src => src.IsPrimary));


                cfg.CreateMap<UpdateVehicleRequest, Vehicle>()
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
               .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber));

                cfg.CreateMap<UpdateUserRequest, ApplicationUser>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));


                cfg.CreateMap<UpdateUserRequest, UserDTO>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber));

                cfg.CreateMap<ParkingLotReq, ParkingLot>()
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
              .ForMember(dest => dest.Zone, opt => opt.MapFrom(src => src.Zone))
              .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
              .ForMember(dest => dest.WorkingHourFrom, opt => opt.MapFrom(src => src.WorkingHourFrom))
              .ForMember(dest => dest.WorkingHourTo, opt => opt.MapFrom(src => src.WorkingHourTo))
              .ForMember(dest => dest.CapacityCar, opt => opt.MapFrom(src => src.CapacityCar))
              .ForMember(dest => dest.CapacityAdaptedCar, opt => opt.MapFrom(src => src.CapacityAdaptedCar))
              .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

                cfg.CreateMap<ParkingLot, ParkingLotDTO>()
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
             .ForMember(dest => dest.Zone, opt => opt.MapFrom(src => src.Zone))
             .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
             .ForMember(dest => dest.WorkingHourFrom, opt => opt.MapFrom(src => src.WorkingHourFrom))
             .ForMember(dest => dest.WorkingHourTo, opt => opt.MapFrom(src => src.WorkingHourTo))
             .ForMember(dest => dest.CapacityCar, opt => opt.MapFrom(src => src.CapacityCar))
             .ForMember(dest => dest.CapacityAdaptedCar, opt => opt.MapFrom(src => src.CapacityAdaptedCar))
             .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
             .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
             .ForMember(dest => dest.IsDeactivated, opt => opt.MapFrom(src => src.IsDeactivated))
             .ForMember(dest => dest.TimeCreated, opt => opt.MapFrom(src => src.TimeCreated))
             .ForMember(dest => dest.TimeModified, opt => opt.MapFrom(src => src.TimeModified))
             .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

                cfg.CreateMap<ParkingLotRequest, RequestDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.ParkingLotId, opt => opt.MapFrom(src => src.ParkingLotId))
                .ForMember(dest => dest.TimeCreated, opt => opt.MapFrom(src => src.TimeCreated));

                cfg.CreateMap<AspNetUser, UserWithoutRoleDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

                cfg.CreateMap<AspNetUser, UserDTO>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
               .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Roles.FirstOrDefault().Name));

                cfg.CreateMap<ParkingLotRequest, GetAllRequestsDTO>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
               .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
               .ForMember(dest => dest.ParkingLotId, opt => opt.MapFrom(src => src.ParkingLotId))
               .ForMember(dest => dest.TimeCreated, opt => opt.MapFrom(src => src.TimeCreated))
               .ForMember(dest => dest.ParkingLot, opt => opt.MapFrom(src => src.ParkingLot))
               .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

            }
            );
            var mapper = new Mapper(config);
            return mapper;
        }
    }
}