using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerce.WebAPI.Controllers
{
    public class CurrencyRateDto
    {
        public string Base { get; set; }

        public string Date { get; set; }

        public Dictionary<string, decimal> Rates { get; set; }
    }
}
