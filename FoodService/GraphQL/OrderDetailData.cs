using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodService.GraphQL
{
    public class OrderDetailData
    {
        public int? Id { get; set; }
        public int? OrderId { get; set; }
        public int FoodId { get; set; }
        public int Quantity { get; set; }
    }
}
