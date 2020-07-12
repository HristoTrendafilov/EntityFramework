namespace BookShop
{
    using BookShop.Models;
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();

            Console.WriteLine(RemoveBooks(db));
        }
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var sb = new StringBuilder();

            var bookTitles = context.Books
                .AsEnumerable()
                .Where(x => x.AgeRestriction.ToString().ToLower() == command.ToLower())
                .OrderBy(x => x.Title)
                .Select(x => new
                {
                    x.Title
                }).ToList();
                

            foreach (var bookTitle in bookTitles)
            {
                sb.AppendLine(bookTitle.Title);
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            var goldenEditionBooks = context.Books
                .AsEnumerable()
                .Where(x => x.EditionType.ToString() == "Gold" && x.Copies < 5000)
                .OrderBy(x => x.BookId)
                .Select(b => b.Title)
                .ToList();

            return string.Join(Environment.NewLine, goldenEditionBooks);
        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var sb = new StringBuilder();
            
            var books = context.Books
                .Where(x => x.Price > 40)
                .Select(x => new
                {
                    x.Title,
                    x.Price
                })
                .OrderByDescending(x => x.Price)
                .ToList();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - ${book.Price:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books
                .Where(x => x.ReleaseDate.Value.Year != year)
                .OrderBy(x => x.BookId)
                .Select(b => b.Title)
                .ToList();

            return string.Join(Environment.NewLine, books);
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var bookTitles = new List<string>();

            var categories = input.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower()).ToArray();

            for (int i = 0; i < categories.Length; i++)
            {
                var currentBooks = context.Books
                    .Where(x => x.BookCategories
                    .Any(bc => bc.Category.Name.ToLower() == categories[i]))
                    .Select(x => x.Title)
                    .ToList();

                bookTitles.AddRange(currentBooks);
            }

            bookTitles = bookTitles.OrderBy(x => x).ToList();

            return string.Join(Environment.NewLine, bookTitles);
        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            var sb = new StringBuilder();

            var dateToCheck = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var books = context.Books
                .Where(x => x.ReleaseDate < dateToCheck)
                .OrderByDescending(x => x.ReleaseDate)
                .Select(x => new
                {
                    x.Title,
                    editionType = x.EditionType.ToString(),
                    x.Price
                }).ToList();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} - {book.editionType} - ${book.Price:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context.Authors
                .Where(x => x.FirstName.EndsWith(input.ToLower()))
                .OrderBy(x => x)
                .Select(x => x.FirstName + " " + x.LastName)
                .ToList();

            return string.Join(Environment.NewLine, authors);
        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var books = context.Books
                .Where(x => x.Title.ToLower().Contains(input.ToLower()))
                .Select(x => x.Title)
                .OrderBy(x => x)
                .ToList();

            return string.Join(Environment.NewLine, books);
        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var sb = new StringBuilder();
            
            var books = context.Books
                .Where(x => x.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .OrderBy(x => x.BookId)
                .Select(x => new
                {
                    x.Title,
                    authorFullName = x.Author.FirstName + " " + x.Author.LastName
                }).ToList();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.Title} ({book.authorFullName})");
            }

            return sb.ToString().TrimEnd();
        }

        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var books = context.Books
                .Where(x => x.Title.Length > lengthCheck)
                .Count();

            return books;
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var sb = new StringBuilder();

            var authors = context.Authors
                .Select(x => new
                {
                    authorFullName = x.FirstName + " " + x.LastName,
                    copiesCount = x.Books.Sum(b => b.Copies)
                })
                .OrderByDescending(x => x.copiesCount)
                .ToList();

            foreach (var author in authors)
            {
                sb.AppendLine($"{author.authorFullName} - {author.copiesCount}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var sb = new StringBuilder();

            var books = context.Categories
                .Select(x => new
                {
                    bookCategory = x.Name,
                    profit = x.CategoryBooks.Sum(cb => cb.Book.Price * cb.Book.Copies)
                })
                .OrderByDescending(x => x.profit)
                .ThenBy(x => x.bookCategory)
                .ToList();

            foreach (var book in books)
            {
                sb.AppendLine($"{book.bookCategory} ${book.profit:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            var sb = new StringBuilder();

            var categories = context.Categories
                .Select(x => new
                {
                    categoryName = x.Name,
                    mostRecentBooks = x.CategoryBooks
                            .OrderByDescending(cb => cb.Book.ReleaseDate)
                            .Take(3)
                            .Select(cb => new
                            {
                                bookTitle = cb.Book.Title,
                                bookYear = cb.Book.ReleaseDate.Value.Year
                            })
                            .ToList()
                })
                .ToList()
                .OrderBy(x => x.categoryName);

            foreach (var category in categories)
            {
                sb.AppendLine($"--{category.categoryName}");

                foreach (var book in category.mostRecentBooks)
                {
                    sb.AppendLine($"{book.bookTitle} ({book.bookYear})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static void IncreasePrices(BookShopContext context)
        {
            var booksToIncreasePrice = context.Books
                .Where(x => x.ReleaseDate.Value.Year < 2010)
                .ToList();

            foreach (var book in booksToIncreasePrice)
            {
                book.Price += 5;
            }
        }

        public static int RemoveBooks(BookShopContext context)
        {
            var booksToDelete = context.Books
                .Where(x => x.Copies < 4200)
                .ToList();

            context.RemoveRange(booksToDelete);
            context.SaveChanges();

            return booksToDelete.Count;
        }
    }
}
