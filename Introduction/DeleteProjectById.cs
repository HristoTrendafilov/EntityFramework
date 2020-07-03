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

            Console.WriteLine(DeleteProjectById(dbContext));
        }
        public static string DeleteProjectById(SoftUniContext context)
        {
            StringBuilder sb = new StringBuilder();

            var employeeCol = context.EmployeesProjects.Where(x => x.ProjectId == 2).ToList();
            
            foreach (var item in employeeCol)
            {
                context.Remove(item);
                context.SaveChanges();
            }

            context.Remove(context.Projects.Where(x => x.ProjectId == 2).FirstOrDefault());
            context.SaveChanges();

            var projects = context.Projects.Take(10).ToList();

            foreach (var project in projects)
            {
                sb.AppendLine(project.Name);
            }

            return sb.ToString().Trim();
        }
    }
}
