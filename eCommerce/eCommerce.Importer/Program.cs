using eCommerce.Data;
using eCommerce.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace eCommerce.Importer
{
    class Program
    {
        static void Main(string[] args)
        {
            string dbName = "eCommerceDatabase.db";
            if (File.Exists(dbName))
            {
                File.Delete(dbName);
            }
            using (var dbContext = new eCommerceDbContext())
            {
                dbContext.Database.EnsureCreated();

                var json = File.ReadAllText("../../../products.json");
                var productDtos = JsonConvert.DeserializeObject<ImportProductsDto[]>(json);

                var products = new List<Product>();

                foreach (var productDto in productDtos)
                {
                    var product = new Product
                    {
                        Name = productDto.Name,
                        ImageUrl = productDto.Image,
                        Price = productDto.Price
                    };

                    products.Add(product);
                }

                dbContext.Products.AddRange(products);

                foreach (var product in dbContext.Products)
                {
                    Console.WriteLine($"product name: {product.Name}, product price: {product.Price}");
                }

                var user = new User
                {
                    Username = "test1",
                    Password = "password1",
                    CurrencyCode = "EUR"
                };

                dbContext.Users.Add(user);
                dbContext.SaveChanges();
            }
        }
    }
}
