using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.Http.Results;
using eCommerce.Data;
using eCommerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eCommerse.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IJwtAuthenticationManager jwtAuthenticationManager;
        private eCommerceDbContext dbContext = new eCommerceDbContext();

        public RegisterController(IJwtAuthenticationManager jwtAuthenticationManager)
        {
            this.jwtAuthenticationManager = jwtAuthenticationManager;
        }


        // POST: api/register
        [HttpPost]
        public IActionResult Post([FromBody] RegisterUserCred registerUserCred)
        {
            if (dbContext.Users.Any(x => x.Username == registerUserCred.Username))
            {
                return Conflict(new { message = $"An existing user with the same username was already found." });
            }

            var user = new User
            {
                Username = registerUserCred.Username,
                Password = registerUserCred.Password,
                CurrencyCode = registerUserCred.CurrencyCode
            };

            if (user.CurrencyCode == null)
            {
                return Conflict(new { message = $"Invalid/empty currency code in body, please re-check your data." });
            }

            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var token = jwtAuthenticationManager.Authenticate(registerUserCred.Username, registerUserCred.Password);

            return Ok("Your user has been created. Bearer token: " + token);
        }
    }
}
