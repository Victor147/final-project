﻿using AutoMapper;
using final_project.Data.Entities;
using final_project.Models;
using final_project.ViewModels;

namespace final_project.MappingProfiles;

public class Profiles : Profile
{
    public Profiles()
    {
        CreateMap<RegisterModel, User>();
        CreateMap<ProductModel, Product>();
        CreateMap<Product, ProductViewModel>();
        CreateMap<Product, UpdateProductModel>();
        CreateMap<UpdateProductModel, ProductModel>()
            .ForMember(
                dest => dest.Image,
                opt => opt.MapFrom(src => src.File)
            );
        CreateMap<Manufacturer, ManufacturerViewModel>();
        CreateMap<User, UserModel>()
            .ForMember(
                dest => dest.Username,
                opt => opt.MapFrom(src => src.UserName)
            );
        CreateMap<Order, OrderAdminModel>()
            .ForMember(dest => dest.Username,
                opt => opt.MapFrom(src => src.User.UserName)
            );
        CreateMap<Category, CategoryViewModel>();
        CreateMap<User, ProfileModel>();
    }
}