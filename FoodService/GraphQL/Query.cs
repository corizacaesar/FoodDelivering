using FoodService.Models;
using HotChocolate.AspNetCore.Authorization;
using Library.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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

        [Authorize(Roles = new[] { "BUYER" })]
        public IQueryable<Food> GetFoodsByBuyer([Service] FoodDeliveringContext context) =>
            context.Foods;

        [Authorize(Roles = new[] { "BUYER" })]
        public IQueryable<Order?> GetOrderByBuyer([Service] FoodDeliveringContext context, ClaimsPrincipal claimsPrincipal)
        {
            var Username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(u => u.Username == Username).FirstOrDefault();
            if (user != null)
            {
                var order = context.Orders.Where(p => p.UserId == user.Id).Include(o => o.OrderDetails).ThenInclude(f => f.Food);
                return order.AsQueryable();
            }
            return new List<Order>().AsQueryable();
        }

        [Authorize(Roles = new[] { "BUYER" })]
        public IQueryable<Order?> GetTrackOrderByBuyer([Service] FoodDeliveringContext context, ClaimsPrincipal claimsPrincipal)
        {
            var userToken = claimsPrincipal.Identity;
            var user = context.Users.Where(u => u.Username == userToken.Name).FirstOrDefault();
            if (user != null)
            {
                var order = context.Orders.Where(p => p.UserId == user.Id && p.Status == false).Include(o => o.OrderDetails).ThenInclude(f => f.Food);
                return order.AsQueryable();
            }
            return new List<Order>().AsQueryable();
        }
    }
}
