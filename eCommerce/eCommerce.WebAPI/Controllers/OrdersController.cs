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
using Microsoft.EntityFrameworkCore.Internal;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using eCommerce.Models.Enums;

namespace eCommerce.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly eCommerceDbContext dbContext;

        public OrdersController(eCommerceDbContext context)
        {
            dbContext = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await dbContext.Orders.ToListAsync();
        }

        // GET: api/orders/{userId}
        [HttpGet("{userId}")]
        public async Task<ActionResult<User>> GetUserOrders(int userId)
        {
            var user = dbContext.Users.Find(userId);

            var ordersForUser = dbContext.Orders.Where(x => x.UserId == userId);

            foreach (var order in ordersForUser)
            {
                var productsForUsersOrders = dbContext.Products.Where(x => x.OrderProducts.Any(x => x.Order.UserId == userId && x.Order == order));

                foreach (var product in productsForUsersOrders)
                {
                    user.OrderProducts.Add(new OrderProduct { Product = product, ProductId = product.Id, Order = order, OrderId = order.Id });
                }

            }

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/orders/{orderId}
        [HttpPut("{orderId}")]
        public async Task<ActionResult<Order>> ChangeOrderStatus(int orderId, [FromBody] string status)
        {
            var order = dbContext.Orders.FirstOrDefault(x => x.Id == orderId);

            order.Status = (Status)Enum.Parse(typeof(Status), status);

            dbContext.SaveChanges();
            return CreatedAtAction("ChangeOrderStatus", order);
        }

        // POST: api/orders
        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] Order order)
        {
            order.CreatedAt = DateTime.Now;

            var user = dbContext.Users.Where(x => x.Id == order.UserId).First();
            foreach (var orderProduct in order.OrderProducts)
            {
                var orderProductFromDb = dbContext.Products.FirstOrDefault(x => x.Name == orderProduct.Product.Name);

                if (orderProductFromDb != null)
                {
                    orderProduct.Product = orderProductFromDb;
                    orderProduct.ProductId = orderProductFromDb.Id;
                    orderProduct.Order = order;
                    orderProduct.OrderId = order.Id;
                }

                user.OrderProducts.Add(orderProduct);
            }

            //Exchange Rates API

            CurrencyRateDto jsonResult = GetExchangeRates();

            if (user.CurrencyCode != "BGN")
            {
                foreach (var rate in jsonResult.Rates)
                {
                    if (rate.Key == user.CurrencyCode)
                    {
                        order.TotalPrice = Math.Round(order.OrderProducts.Sum(x => x.Product.Price * rate.Value), 2);
                    }
                }
            }
            else
            {
                order.TotalPrice = order.OrderProducts.Sum(x => x.Product.Price);
            }

            order.User = user;

            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();
            return CreatedAtAction("CreateOrder", new { id = order.Id }, order);
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
    }
}
