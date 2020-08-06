namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    using Data;
    using System.Text;
    using System.Xml.Serialization;
    using TeisterMask.DataProcessor.ImportDto;
    using System.IO;
    using TeisterMask.Data.Models;
    using System.Globalization;
    using TeisterMask.Data.Models.Enums;
    using Castle.Components.DictionaryAdapter;
    using Newtonsoft.Json;
    using System.Linq;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(ImportProjectsDTO[]), new XmlRootAttribute("Projects"));

            using(StringReader stringReader = new StringReader(xmlString))
            {
                var projectDtos = (ImportProjectsDTO[])serializer.Deserialize(stringReader);

                var validProjects = new List<Project>();

                foreach (var project in projectDtos)
                {
                    if (!IsValid(project))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime projectOpenDate;

                    var isProjectOpenDateValid = DateTime.TryParseExact(project.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out projectOpenDate);


                    if (!isProjectOpenDateValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime? projectDueDate;

                    if (!string.IsNullOrEmpty(project.DueDate))
                    {
                        DateTime projectDueDateValue;
                        var isProjectDueDateValid = DateTime.TryParseExact(project.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out projectDueDateValue);

                        if (!isProjectDueDateValid)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        projectDueDate = projectDueDateValue;
                    }
                    else
                    {
                        projectDueDate = null;
                    }

                    var projectToImport = new Project()
                    {
                        Name = project.Name,
                        OpenDate = projectOpenDate,
                        DueDate = projectDueDate,
                    };

                    foreach (var task in project.Tasks)
                    {
                        if (!IsValid(task))
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        DateTime taskOpenDate;

                        var isTaskOpenDateValid = DateTime.TryParseExact(task.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out taskOpenDate);

                        if (!isTaskOpenDateValid)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        DateTime taskDueDate;

                        var isTaskDueDateValid = DateTime.TryParseExact(task.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out taskDueDate);

                        if (!isTaskDueDateValid)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        if (taskOpenDate < projectOpenDate)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        if (projectDueDate.HasValue)
                        {
                            if (taskDueDate > projectDueDate.Value)
                            {
                                sb.AppendLine(ErrorMessage);
                                continue;
                            }
                        }

                        var taskToImport = new Task()
                        {
                            Name = task.Name,
                            OpenDate = taskOpenDate,
                            DueDate = taskDueDate,
                            ExecutionType = (ExecutionType)task.ExecutionType,
                            LabelType = (LabelType)task.LabelType
                        };

                        projectToImport.Tasks.Add(taskToImport);

                    }
                    validProjects.Add(projectToImport);

                    sb.AppendLine(String.Format(SuccessfullyImportedProject, projectToImport.Name, projectToImport.Tasks.Count));
                }

                context.AddRange(validProjects);
                context.SaveChanges();
            }

            return sb.ToString().TrimEnd();
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var employeeDtos = JsonConvert.DeserializeObject<ImportEmployeeDTO[]>(jsonString);

            var employees = new List<Employee>();

            foreach (var employee in employeeDtos)
            {
                if (!IsValid(employee))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var em = new Employee()
                {
                    Username = employee.Username,
                    Email = employee.Email,
                    Phone = employee.Phone
                };

                foreach (var taskId in employee.Tasks.Distinct())
                {
                    var task = context.Tasks
                        .FirstOrDefault(x => x.Id == taskId);

                    if(task == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    em.EmployeesTasks.Add(new EmployeeTask()
                    {
                        Employee = em,
                        Task = task
                    });
                }
                employees.Add(em);
                sb.AppendLine(string.Format(SuccessfullyImportedEmployee, em.Username, em.EmployeesTasks.Count));
            }
            context.Employees.AddRange(employees);
            context.SaveChanges();
            
            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}