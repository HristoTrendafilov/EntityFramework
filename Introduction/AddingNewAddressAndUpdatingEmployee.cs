using SoftUni.Data;
using SoftUni.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            var dbContext = new SoftUniContext();

            Console.WriteLine(AddNewAddressToEmployee(dbContext));
        }
        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var newAddress = new Address()
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };

            context.Addresses.Add(newAddress);

            var employeeToUpdateAdress = context.Employees
                .Where(x => x.LastName == "Nakov")
                .First();

            employeeToUpdateAdress.Address = newAddress;

            context.SaveChanges();

            var adresses = context.Employees
                .OrderBy(x=>x.AddressId)
                .Take(10)
                .Select(x=> new { adressName = x.Address.AddressText})
                .ToList();

            foreach (var address in adresses)
            {
                sb.AppendLine(address.adressName);
            };

            return sb.ToString().TrimEnd();
        }
    }
}
