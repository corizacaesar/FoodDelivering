using System;
using System.Collections.Generic;

namespace Library.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public int UserId { get; set; }
        public string? Latitude { get; set; }
        public string? Langitude { get; set; }
        public bool Status { get; set; }
        public DateTime Created { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
