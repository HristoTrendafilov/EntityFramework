using CarDealer.Data;
using CarDealer.DTO;
using CarDealer.Models;
using CarDealer.XMLHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.XPath;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var dbContext = new CarDealerContext();

            var suppliersXML = File.ReadAllText("../../../Datasets/suppliers.xml");
            var partsXML = File.ReadAllText("../../../Datasets/parts.xml");
            var carsXML = File.ReadAllText("../../../Datasets/cars.xml");
            var customersXML = File.ReadAllText("../../../Datasets/customers.xml");
            var salesXML = File.ReadAllText("../../../Datasets/sales.xml");

            //var suppliersCount = ImportSuppliers(dbContext, suppliersXML);
            //var partsCount = ImportParts(dbContext, partsXML);
            //var carsCount = ImportCars(dbContext, carsXML);
            //var customersCount = ImportCustomers(dbContext, customersXML);
            //var salesCount = ImportSales(dbContext, salesXML);

            //Console.WriteLine(suppliersCount);
            //Console.WriteLine(partsCount);
            //Console.WriteLine(carsCount);
            //Console.WriteLine(customersCount);
            //Console.WriteLine(salesCount);

            var result = GetSalesWithAppliedDiscount(dbContext);

            File.WriteAllText("../../../Results/CarsWithDiscount.xml", result);
        }

        public static void ResetDatabase(CarDealerContext context)
        {
            context.Database.EnsureDeleted();
            Console.WriteLine("Database successfully deleted");

            context.Database.EnsureCreated();
            Console.WriteLine("Database successfully created");
        }

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            var suppliersDtos = XMLConverter.Deserializer<ImportSupplierDTO>(inputXml, "Suppliers");

            var result = suppliersDtos
                .Select(x => new Supplier
                {
                    Name = x.Name,
                    IsImporter = x.IsImporter
                })
                .ToArray();

            context.Suppliers.AddRange(result);
            context.SaveChanges();

            return $"Successfully imported {result.Length}";
        }

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            var partsDtos = XMLConverter.Deserializer<ImportPartDTO>(inputXml, "Parts");

            var parts = partsDtos
                .Where(i => context.Suppliers.Any(s => s.Id == i.SupplierId))
                .Select(x => new Part
                {
                    Name = x.Name,
                    Price = x.Price,
                    Quantity = x.Quantity,
                    SupplierId = x.SupplierId
                })
                .ToArray();

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Length}";
        }

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            var carsDtos = XMLConverter.Deserializer<ImportCarDTO>(inputXml, "Cars");

            var cars = new List<Car>();

            foreach (var carDto in carsDtos)
            {
                var uniqueParts = carDto.Parts.Select(x => x.Id).Distinct().ToArray();
                var realParts = uniqueParts.Where(id => context.Parts.Any(i => i.Id == id));

                var car = new Car
                {
                    Make = carDto.Make,
                    Model = carDto.Model,
                    TravelledDistance = carDto.TravelledDistance,
                    PartCars = realParts.Select(id => new PartCar
                    {
                        PartId = id
                    })
                    .ToArray()
                };
                cars.Add(car);
            };

            context.Cars.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}";
        }

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            var customersDtos = XMLConverter.Deserializer<ImportCustomerDTO>(inputXml, "Customers");

            var customers = customersDtos
                .Select(x => new Customer
                {
                    Name = x.Name,
                    IsYoungDriver = x.IsYoungDriver,
                    BirthDate = DateTime.Parse(x.BirthDate)
                })
                .ToArray();

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Length}";
        }

        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            var salessDtos = XMLConverter.Deserializer<ImportSaleDTO>(inputXml, "Sales");

            var sales = salessDtos
                .Where(i => context.Cars.Any(x => x.Id == i.CarId))
                .Select(x => new Sale
                {
                    CarId = x.CarId,
                    CustomerId = x.CustomerId,
                    Discount = x.Discount
                })
                .ToArray();

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Length}";
        }

        //public static string GetCarsWithDistance(CarDealerContext context)
        //{

        //}

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var result = context.Sales
                .Select(x => new ExportSaleDTO
                {
                    Car = new ExportCarDto
                    {
                        Make = x.Car.Make,
                        Model = x.Car.Model,
                        TravelledDistance = x.Car.TravelledDistance
                    },
                    Discount = x.Discount,
                    CustomerName = x.Customer.Name,
                    Price = x.Car.PartCars.Sum(p => p.Part.Price),
                    PriceWithDiscount = x.Car.PartCars.Sum(p => p.Part.Price) - (x.Car.PartCars.Sum(p => p.Part.Price) * x.Discount / 100)
                })
                .ToArray();

            var xmlResult = XMLConverter.Serialize(result, "sales");

            return xmlResult;
        }
    }
}