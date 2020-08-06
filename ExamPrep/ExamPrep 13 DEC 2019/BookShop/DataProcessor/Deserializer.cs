namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using BookShop.Data.Models;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(ImportBooksDTO[]), new XmlRootAttribute("Books"));

            using (StringReader stringReader = new StringReader(xmlString))
            {
                var booksDto = (ImportBooksDTO[])serializer.Deserialize(stringReader);

                var validBooks = new List<Book>();

                foreach (var book in booksDto)
                {
                    if (!IsValid(book))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime publishedOn;
                    var isValidDate = DateTime.TryParseExact(book.PublishedOn, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out publishedOn);

                    if (!isValidDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var bookToImport = new Book()
                    {
                        Name = book.Name,
                        Genre = (Genre)book.Genre,
                        Pages = book.Pages,
                        Price = book.Price,
                        PublishedOn = publishedOn,
                    };

                    validBooks.Add(bookToImport);

                    sb.AppendLine(string.Format(SuccessfullyImportedBook, book.Name, book.Price));
                }
                context.AddRange(validBooks);
                context.SaveChanges();

                return sb.ToString().TrimEnd();
            }
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            var sb = new StringBuilder();

            var authorsDto = JsonConvert.DeserializeObject<ImportAuthorsDTO[]>(jsonString);

            var validAuthors = new List<Author>();

            foreach (var author in authorsDto)
            {
                if (!IsValid(author))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if(validAuthors.Any(x => x.Email == author.Email))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var authorToImport = new Author()
                {
                    FirstName = author.FirstName,
                    LastName = author.LastName,
                    Phone = author.Phone,
                    Email = author.Email
                };

                foreach (var book in author.Books)
                {
                    if (!book.Id.HasValue)
                    {
                        continue;
                    }

                    var bookToImport = context.Books
                        .FirstOrDefault(x => x.Id == book.Id);

                    if(bookToImport == null)
                    {
                        continue;
                    }

                    authorToImport.AuthorsBooks.Add(new AuthorBook()
                    {
                        Author = authorToImport,
                        Book = bookToImport
                    });
                }
                
                if(authorToImport.AuthorsBooks.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                validAuthors.Add(authorToImport);
                sb.AppendLine(string.Format(SuccessfullyImportedAuthor, authorToImport.FirstName + ' ' + authorToImport.LastName, authorToImport.AuthorsBooks.Count));
            }

            context.Authors.AddRange(validAuthors);
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