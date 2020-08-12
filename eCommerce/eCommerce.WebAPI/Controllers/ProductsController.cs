using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eCommerce.Data;
using eCommerce.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using eCommerce.WebAPI.Controllers;

namespace eCommerse.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly eCommerceDbContext dbContext;

        public ProductsController(eCommerceDbContext context)
        {
            dbContext = context;
        }

        // GET: api/products
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await dbContext.Products.ToListAsync();
        }

        // GET: api/products/{id}
        [HttpGet("{id}/{userId}")]
        public async Task<ActionResult<Product>> GetProductAndUserBasedPrice(int id, int userId)
        {
            var product = dbContext.Products.Find(id);
            var user = dbContext.Users.Find(userId);

            if (product == null)
            {
                return NotFound();
            }

            CurrencyRateDto jsonResult = GetExchangeRates();

            if (user.CurrencyCode != "BGN")
            {
                foreach (var rate in jsonResult.Rates)
                {
                    if (rate.Key == user.CurrencyCode)
                    {
                        product.Price = Math.Round(product.Price * rate.Value, 2);
                    }
                }
            }

            return product;
        }

        private static CurrencyRateDto GetExchangeRates()
        {
            string exchangeRatesURL = String.Format("https://api.exchangeratesapi.io/latest?base=BGN");
            WebRequest getRequest = WebRequest.Create(exchangeRatesURL);
            getRequest.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)getRequest.GetResponse();

            string result = string.Empty;

            using (Stream stream = response.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                result = sr.ReadToEnd();
                sr.Close();
            }

            var jsonResult = JsonConvert.DeserializeObject<CurrencyRateDto>(result);
            return jsonResult;
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = dbContext.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // POST: api/products/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (dbContext.Products.Any(x => x.Name == product.Name))
            {
                return Conflict(new { message = $"A product with the same name already exists." });
            }
            
            if (string.IsNullOrEmpty(product.Name))
            {
                return Conflict(new { message = $"Name cannot be null or empty." });
            }

            if (product.Price <= 0)
            {
                return Conflict(new { message = $"Price cannot be less or equal to zero." });
            }

            dbContext.Products.Add(product);
            dbContext.SaveChanges();

            return Ok($"Your product {product.Name} has been successfully added.");
        }

        // DELETE: api/products/delete/id
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await dbContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();

            return product;
        }
    }
}
