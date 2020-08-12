using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace eCommerce.Models
{
    public class Product
    {
        public Product()
        {
            this.OrderProducts = new HashSet<OrderProduct>();
        }

        [Key]
        public int Id { get; set; }

        [StringLength(100, MinimumLength = 2)]
        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public ICollection<OrderProduct> OrderProducts { get; set; }
    }
}
