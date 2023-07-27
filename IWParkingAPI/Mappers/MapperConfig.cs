﻿using AutoMapper;
using IWParkingAPI.Models;
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
                cfg.CreateMap<RoleRequest, ApplicationRole>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.NormalizedName, opt => opt.MapFrom(src => src.Name.ToUpper()));
                cfg.CreateMap<RoleRequest, AspNetRole>();
                cfg.CreateMap<UserRequest, ApplicationUser>()
               
        .ForMember(dest =>
            dest.UserName,
            opt => opt.MapFrom(src => src.UserName))
        .ForMember(dest =>
            dest.Name,
            opt => opt.MapFrom(src => src.Name))
        .ForMember(dest =>
            dest.Surname,
            opt => opt.MapFrom(src => src.Surname))
        .ForMember(dest =>
            dest.Email,
            opt => opt.MapFrom(src => src.Email))
        .ForMember(dest =>
            dest.PasswordHash,
            opt => opt.MapFrom(src => src.PasswordHash))
        .ForMember(dest =>
            dest.NormalizedUserName,
            opt => opt.MapFrom(src => src.UserName.ToUpper()))
        .ForMember(dest =>
            dest.NormalizedEmail,
            opt => opt.MapFrom(src => src.Email.ToUpper()))
        .ForMember(dest =>
            dest.PhoneNumber,
            opt => opt.MapFrom(src => src.PhoneNumber));
            });
            var mapper = new Mapper(config);
            return mapper;
        }
    }
}