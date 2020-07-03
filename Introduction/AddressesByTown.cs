using SoftUni.Data;
using SoftUni.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            var dbContext = new SoftUniContext();

            Console.WriteLine(GetAddressesByTown(dbContext));
        }
        public static string GetAddressesByTown(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var addresses = context.Addresses
                            .OrderByDescending(a => a.Employees.Count)
                            .ThenBy(a => a.Town.Name)
                            .Take(10)
                            .Select(a => new
                            {
                                Text = a.AddressText,
                                Town = a.Town.Name,
                                EmployeesCount = a.Employees.Count
                            })
                            .ToList();

            foreach (var address in addresses)
            {
                sb.AppendLine($"{address.Text}, {address.Town} - {address.EmployeesCount} employees");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
