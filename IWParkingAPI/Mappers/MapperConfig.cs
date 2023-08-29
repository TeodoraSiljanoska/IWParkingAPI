﻿using AutoMapper;
using IWParkingAPI.Models;
using IWParkingAPI.Models.Data;
using IWParkingAPI.Models.Requests;
using IWParkingAPI.Models.Responses.Dto;

namespace IWParkingAPI.Mappers
{
    public class MapperConfig
    {
        public static Mapper InitializeAutomapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                ///AUTH 

                //Register
                cfg.CreateMap<UserRegisterRequest, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Phone));

                //Register
                cfg.CreateMap<ApplicationUser, UserDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));


                ///PARKING LOT

                //GetAll, Get{id}, Create, Update, Delete{id}, RemoveFavourite, MakeFavourite, GetFavourites
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

                //Create
                /*cfg.CreateMap<ParkingLotReq, ParkingLot>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.Zone, opt => opt.MapFrom(src => src.Zone))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.WorkingHourFrom, opt => opt.MapFrom(src => src.WorkingHourFrom))
                .ForMember(dest => dest.WorkingHourTo, opt => opt.MapFrom(src => src.WorkingHourTo))
                .ForMember(dest => dest.CapacityCar, opt => opt.MapFrom(src => src.CapacityCar))
                .ForMember(dest => dest.CapacityAdaptedCar, opt => opt.MapFrom(src => src.CapacityAdaptedCar))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));*/


                ///REQUEST

                //GetAll, Modify
                cfg.CreateMap<ParkingLotRequest, RequestDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.ParkingLotId, opt => opt.MapFrom(src => src.ParkingLotId))
                .ForMember(dest => dest.TimeCreated, opt => opt.MapFrom(src => src.TimeCreated));


                ///ROLE

                //GetAll, Get{id}, Create, Update{id}, Delete{id}
                cfg.CreateMap<ApplicationRole, RoleDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TimeCreated, opt => opt.MapFrom(src => src.TimeCreated))
                .ForMember(dest => dest.TimeModified, opt => opt.MapFrom(src => src.TimeModified));

                //Create
                cfg.CreateMap<RoleRequest, ApplicationRole>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.Name.ToUpper()));


                ///USER

                //GetAll, Get{id}, Update, Deactivate, Deactivate{id}
                cfg.CreateMap<AspNetUser, UserDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Surname, opt => opt.MapFrom(src => src.Surname))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Roles.FirstOrDefault().Name));


                ///VEHICLE

                //GetAll
                cfg.CreateMap<Vehicle, VehicleWithUserDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber))
                .ForMember(dest => dest.TimeCreated, opt => opt.MapFrom(src => src.TimeCreated))
                .ForMember(dest => dest.TimeModified, opt => opt.MapFrom(src => src.TimeModified))
                .ForMember(dest => dest.IsPrimary, opt => opt.MapFrom(src => src.IsPrimary))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

                //Create
                cfg.CreateMap<VehicleRequest, Vehicle>()
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
               .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber));

                //Create, Delete{id}, Update{id}, Get{id}, GetVehiclesByUser
                cfg.CreateMap<Vehicle, VehicleDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber))
                .ForMember(dest => dest.TimeCreated, opt => opt.MapFrom(src => src.TimeCreated))
                .ForMember(dest => dest.TimeModified, opt => opt.MapFrom(src => src.TimeModified))
                .ForMember(dest => dest.IsPrimary, opt => opt.MapFrom(src => src.IsPrimary));


                ///ZONE

                //Create
                cfg.CreateMap<ZoneRequest, Zone>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));





                cfg.CreateMap<ParkingLotReq, TempParkingLot>()
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
             .ForMember(dest => dest.Zone, opt => opt.MapFrom(src => src.Zone))
             .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
             .ForMember(dest => dest.WorkingHourFrom, opt => opt.MapFrom(src => src.WorkingHourFrom))
             .ForMember(dest => dest.WorkingHourTo, opt => opt.MapFrom(src => src.WorkingHourTo))
             .ForMember(dest => dest.CapacityCar, opt => opt.MapFrom(src => src.CapacityCar))
             .ForMember(dest => dest.CapacityAdaptedCar, opt => opt.MapFrom(src => src.CapacityAdaptedCar))
             .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

                cfg.CreateMap<TempParkingLot, ParkingLotDTO>()
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

                cfg.CreateMap<UpdateParkingLotRequest, TempParkingLot>()
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
             .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
             .ForMember(dest => dest.Zone, opt => opt.MapFrom(src => src.Zone))
             .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
             .ForMember(dest => dest.WorkingHourFrom, opt => opt.MapFrom(src => src.WorkingHourFrom))
             .ForMember(dest => dest.WorkingHourTo, opt => opt.MapFrom(src => src.WorkingHourTo))
             .ForMember(dest => dest.CapacityCar, opt => opt.MapFrom(src => src.CapacityCar))
             .ForMember(dest => dest.CapacityAdaptedCar, opt => opt.MapFrom(src => src.CapacityAdaptedCar))
             .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

                cfg.CreateMap<TempParkingLot, TempParkingLotDTO>()
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

                cfg.CreateMap<TempParkingLotDTO, ParkingLot>()
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

                cfg.CreateMap<ParkingLot, TempParkingLotDTO>()
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


                cfg.CreateMap<ParkingLot, TempParkingLot>()
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

                cfg.CreateMap<TempParkingLotDTO, TempParkingLot>()
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

            }
            );
            var mapper = new Mapper(config);
            return mapper;
        }
    }
}