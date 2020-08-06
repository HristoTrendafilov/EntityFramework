namespace TeisterMask.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using TeisterMask.Data.Models.Enums;
    using TeisterMask.DataProcessor.ExportDto;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportProjectWithTheirTasks(TeisterMaskContext context)
        {
            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(ExportProjectsDTO[]), new XmlRootAttribute("Projects"));
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using (StringWriter stringWriter = new StringWriter(sb))
            {
                var projects = context.Projects
                    .Where(x => x.Tasks.Count > 0)
                    .Select(x => new ExportProjectsDTO()
                    {
                        ProjectName = x.Name,
                        TasksCount = x.Tasks.Count,
                        HasEndDate = x.DueDate.HasValue ? "Yes" : "No",
                        Tasks = x.Tasks
                            .Select(t => new ExportTasksToProjectDTO()
                            {
                                Name = t.Name,
                                Label = t.LabelType.ToString()
                            })
                            .OrderBy(t => t.Name)
                            .ToArray()
                    })
                    .OrderByDescending(x => x.TasksCount)
                    .ThenBy(x => x.ProjectName)
                    .ToArray();

                serializer.Serialize(stringWriter, projects, namespaces);
            }
            return sb.ToString().TrimEnd();
        }

        public static string ExportMostBusiestEmployees(TeisterMaskContext context, DateTime date)
        {
            var employees = context.Employees
                .Where(x => x.EmployeesTasks.Any(et => et.Task.OpenDate >= date))
                .Select(x => new
                {
                    Username = x.Username,
                    Tasks = x.EmployeesTasks
                    .Where(et => et.Task.OpenDate >= date)
                    .OrderByDescending(et => et.Task.DueDate)
                    .ThenBy(et => et.Task.Name)
                    .Select(et => new
                    {
                        TaskName = et.Task.Name,
                        OpenDate = et.Task.OpenDate.ToString("d", CultureInfo.InvariantCulture),
                        DueDate = et.Task.DueDate.ToString("d", CultureInfo.InvariantCulture),
                        LabelType = et.Task.LabelType.ToString(),
                        ExecutionType = et.Task.ExecutionType.ToString()
                    })
                    .ToArray()
                })
                .OrderByDescending(x => x.Tasks.Length)
                .ThenBy(x => x.Username)
                .Take(10)
                .ToArray();

            var json = JsonConvert.SerializeObject(employees, Formatting.Indented);

            return json;
        }
    }
}