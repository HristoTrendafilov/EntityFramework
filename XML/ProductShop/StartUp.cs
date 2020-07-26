using AutoMapper;
using ProductShop.Data;
using ProductShop.Dtos.Export;
using ProductShop.Dtos.Import;
using ProductShop.Models;
using ProductShop.XMLHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var dbContext = new ProductShopContext();

            //Import Data
            var usersXML = File.ReadAllText("../../../Datasets/users.xml");
            var productsXML = File.ReadAllText("../../../Datasets/products.xml");
            var categoriesXML = File.ReadAllText("../../../Datasets/categories.xml");
            var categoriesProductsXML = File.ReadAllText("../../../Datasets/categories-products.xml");

            var usersCount = ImportUsers(dbContext, usersXML);
            var productsCount = ImportProducts(dbContext, productsXML);
            var categoriesCount = ImportCategories(dbContext, categoriesXML);
            var categoriesProductsCount = ImportCategoryProducts(dbContext, categoriesProductsXML);

            //Check Result For Imported Data
            Console.WriteLine(usersCount);
            Console.WriteLine(productsCount);
            Console.WriteLine(categoriesCount);
            Console.WriteLine(categoriesProductsCount);

        }

        public static void ResetDatabase(ProductShopContext context)
        {
            context.Database.EnsureDeleted();
            Console.WriteLine("Database successfully deleted");

            context.Database.EnsureCreated();
            Console.WriteLine("Database successfully created");
        }

        //Import Data
        public static string ImportUsers(ProductShopContext context, string inputXml)
        {
            const string rootElement = "Users";

            var usersResult = XMLConverter.Deserializer<ImportUserDTO>(inputXml, rootElement);

            var users = usersResult
                .Select(u => new User
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age
                })
                .ToArray();

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Length}";
        }

        public static string ImportProducts(ProductShopContext context, string inputXml)
        {
            const string rootElements = "Products";

            var productsDtos = XMLConverter.Deserializer<ImportProductDTO>(inputXml, rootElements);

            var products = productsDtos
                .Select(p => new Product
                {
                    Name = p.Name,
                    Price = p.Price,
                    BuyerId = p.BuyerId,
                    SellerId = p.SellerId
                })
                .ToArray();

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        public static string ImportCategories(ProductShopContext context, string inputXml)
        {
            const string rootElements = "Categories";

            var categoriesDtos = XMLConverter.Deserializer<ImportCategoryDTO>(inputXml, rootElements);

            var categories = categoriesDtos
                .Where(c => c.Name != null)
                .Select(c => new Category
                {
                    Name = c.Name,
                })
                .ToArray();

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Length}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputXml)
        {
            const string rootElement = "CategoryProducts";

            var categoryProductsDtos = XMLConverter.Deserializer<ImportCategoryProductDTO>(inputXml, rootElement);

            var categoriesProducts = categoryProductsDtos
                .Where(i =>
                         context.Categories.Any(s => s.Id == i.CategoryId) &&
                         context.Products.Any(s => s.Id == i.ProductId))
                .Select(c => new CategoryProduct
                {
                    CategoryId = c.CategoryId,
                    ProductId = c.ProductId,
                })
                .ToArray();

            context.CategoryProducts.AddRange(categoriesProducts);
            context.SaveChanges();

            return $"Successfully imported {categoriesProducts.Length}";
        }

        //Export Data
        public static string GetProductsInRange(ProductShopContext context)
        {
            const string rootElements = "Products";

            var products = context.Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .Select(x => new ExportProductInfoDTO
                {
                    Name = x.Name,
                    Price = x.Price,
                    Buyer = x.Buyer.FirstName + " " + x.Buyer.LastName
                })
                .OrderBy(p => p.Price)
                .Take(10)
                .ToList();

            var result = XMLConverter.Serialize(products, rootElements);

            return result;
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            const string rootElements = "Users";

            var usersWithProducts = context.Users
                .Where(u => u.ProductsSold.Any())
                .Select(x => new ExportUserSoldProductDTO
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    SoldProducts = x.ProductsSold.Select(p => new UserProductDTO
                    {
                        Name = p.Name,
                        Price = p.Price
                    })
                    .ToArray()
                })
                .OrderBy(l => l.LastName)
                .ThenBy(f => f.FirstName)
                .Take(5)
                .ToArray();

            var result = XMLConverter.Serialize(usersWithProducts, rootElements);

            return result;
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            const string rootElement = "Categories";

            var categories = context.Categories
                .Select(x => new ExportCategoryDTO
                {
                    Name = x.Name,
                    Count = x.CategoryProducts.Count,
                    TotalRevenue = x.CategoryProducts.Select(c => c.Product).Sum(p => p.Price),
                    AveragePrice = x.CategoryProducts.Select(c => c.Product).Average(p => p.Price),
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.TotalRevenue)
                .ToArray();

            var result = XMLConverter.Serialize(categories, rootElement);

            return result;
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var usersAndProducts = context.Users
                .ToArray()
                .Where(p => p.ProductsSold.Any())
                .Select(u => new ExportUserDTO
                {
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Age = u.Age,
                    SoldProducts = new ExportProductCountDTO
                    {
                        Count = u.ProductsSold.Count,
                        Products = u.ProductsSold.Select(p => new ExportProductDTO
                        {
                            Name = p.Name,
                            Price = p.Price
                        })
                        .OrderByDescending(p => p.Price)
                        .ToArray()
                    }
                })
                .OrderByDescending(x => x.SoldProducts.Count)
                .Take(10)
                .ToArray();

            var resultDto = new ExportUsersCountDTO
            {
                Count = context.Users.Where(p => p.ProductsSold.Any()).Count(),
                Users = usersAndProducts
            };

            var result = XMLConverter.Serialize(resultDto, "Users");

            return result;
        }
    }
}