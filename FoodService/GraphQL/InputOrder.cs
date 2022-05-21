using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodService.GraphQL
{
    public class InputOrder
    {
        public int CourierId { get; set; }
        public List<OrderDetailData> Details { get; set; }
    }
}
