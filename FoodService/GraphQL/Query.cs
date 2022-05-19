using FoodService.Models;
using HotChocolate.AspNetCore.Authorization;
using Library.Models;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace FoodService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "MANAGER" })]
        public IQueryable<FoodData> GetFood([Service] FoodDeliveringContext context) =>
                  context.Foods.Select(p => new FoodData()
                  {
                      Id = p.Id,
                      Name = p.Name,
                      Stock = p.Stock,
                      Price = p.Price
                  });
    }
}
