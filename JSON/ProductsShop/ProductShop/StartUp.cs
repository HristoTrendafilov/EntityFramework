using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.Models;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var dbContext = new ProductShopContext();
            //var categoriesJson = File.ReadAllText("../../../Datasets/categories.json");
            //var categoriesProductsJson = File.ReadAllText("../../../Datasets/categories-products.json");
            //var productsJson = File.ReadAllText("../../../Datasets/products.json");
            //var usersJson = File.ReadAllText("../../../Datasets/users.json");

            //ResetDatabase(dbContext);
            //ImportUsers(dbContext, usersJson);
            //ImportProducts(dbContext, productsJson);
            //ImportCategories(dbContext, categoriesJson);
            //ImportCategoryProducts(dbContext, categoriesProductsJson);

            var json = GetUsersWithProducts(dbContext);
            Console.WriteLine(json);


        }

        private static void ResetDatabase(ProductShopContext context)
        {
            context.Database.EnsureDeleted();
            Console.WriteLine("Database successfully deleted");

            context.Database.EnsureCreated();
            Console.WriteLine("Database successfully created");
        }

        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            var users = JsonConvert.DeserializeObject<List<User>>(inputJson);

            context.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Count}";
        }

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var products = JsonConvert.DeserializeObject<Product[]>(inputJson);

            context.Products.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Length}";
        }

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            var categories = JsonConvert.DeserializeObject<Category[]>(inputJson)
                .Where(c => c.Name != null).ToArray();

            context.Categories.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Length}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            var categoryProducts = JsonConvert.DeserializeObject<CategoryProduct[]>(inputJson);

            context.CategoryProducts.AddRange(categoryProducts);
            context.SaveChanges();

            return $"Successfully imported {categoryProducts.Length}";
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .OrderBy(x => x.Price)
                .Select(x => new
                {
                    name = x.Name,
                    price = x.Price,
                    seller = x.Seller.FirstName + " " + x.Seller.LastName
                }).ToArray();

            var json = JsonConvert.SerializeObject(products, Formatting.Indented);

            return json;
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(x => x.ProductsSold.Any(ps => ps.Buyer != null))
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .Select(x => new
                {
                    firstName = x.FirstName,
                    lastName = x.LastName,
                    productsSold = x.ProductsSold
                        .Where(pb => pb.Buyer != null)
                        .Select(pb => new
                        {
                            name = pb.Name,
                            price = pb.Price,
                            buyerFirstName = pb.Buyer.FirstName,
                            buyerLastName = pb.Buyer.LastName
                        }).ToArray()
                }).ToArray();

            var json = JsonConvert.SerializeObject(users, Formatting.Indented);

            return json;
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categories = context.Categories
                .Select(x => new
                {
                    category = x.Name,
                    productsCount = x.CategoryProducts.Count(),
                    averagePrice = x.CategoryProducts.Average(cp => cp.Product.Price).ToString("f2"),
                    totalRevenue = x.CategoryProducts.Sum(cp => cp.Product.Price).ToString("f2")
                })
                .OrderByDescending(x => x.productsCount)
                .ToArray();

            var json = JsonConvert.SerializeObject(categories, Formatting.Indented);

            return json;
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(x => x.ProductsSold.Any(p => p.Buyer != null))
                .Select(x => new
                {
                    lastName = x.LastName,
                    age = x.Age,
                    soldProducts = new
                    {
                        count = x.ProductsSold.Where(p => p.Buyer != null).Count(),
                        products = x.ProductsSold.Where(p => p.Buyer != null)
                            .Select(p => new
                            {
                                name = p.Name,
                                price = p.Price
                            }).ToArray()
                    }
                })
                .OrderByDescending(x => x.soldProducts.count)
                .ToArray();

            var resultObj = new
            {
                usersCount = users.Length,
                users = users
            };

            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            var json = JsonConvert.SerializeObject(resultObj, settings);

            return json;
        }
    }
}