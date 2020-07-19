using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using CarDealer.DTO;
using CarDealer.Models;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            this.CreateMap<Customer, GetOrderedCustomersDTO>()
                .ForMember(x => x.Name, y =>
                    y.MapFrom(x => x.Name))
                .ForMember(x => x.BirthDate, y =>
                    y.MapFrom(x => x.BirthDate.Date.ToString("dd/MM/yyyy")))
                .ForMember(x => x.IsYoungDriver, y =>
                    y.MapFrom(x => x.IsYoungDriver));

            this.CreateMap<Car, ToyotaCarsDTO>();

            this.CreateMap<Supplier, LocalSupliersDTO>()
                .ForMember(x => x.PartsCount, y =>
                    y.MapFrom(x => x.Parts.Count));
        }
    }
}
