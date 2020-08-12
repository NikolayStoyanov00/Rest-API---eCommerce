using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eCommerce.Models
{
    public class User
    {
        public User()
        {
            this.OrderProducts = new HashSet<OrderProduct>();
        }

        [Key]
        public int Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string CurrencyCode { get; set; }

        public virtual ICollection<OrderProduct> OrderProducts { get; set; }
    }
}