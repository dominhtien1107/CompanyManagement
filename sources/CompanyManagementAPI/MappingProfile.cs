﻿using AutoMapper;
using Entities.Models;
using Shared.DataTransferObjects;

namespace CompanyManagementAPI;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Company, CompanyDto>().ForMember(c => c.FullAddress, opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));
        CreateMap<Employee, EmployeeDto>();
        CreateMap<CompanyForCreationDto, Company>();
        CreateMap<EmployeeForCreationDto, Employee>();
        CreateMap<EmployeeForUpdateDto, Employee>();
        CreateMap<CompanyForUpdateDto, Company>();
        CreateMap<EmployeeForUpdateDto, Employee>().ReverseMap(); // Ánh xạ 2 chiều.
        CreateMap<UserForRegistrationDto, User>();
    } 
}
