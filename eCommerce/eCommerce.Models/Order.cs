using eCommerce.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace eCommerce.Models
{
    public class Order
    {
        public Order()
        {
            this.OrderProducts = new HashSet<OrderProduct>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<OrderProduct> OrderProducts { get; set; }

        public decimal TotalPrice { get; set; }

        public Status Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
