using HotChocolate.AspNetCore.Authorization;
using Library.Models;
using UserService.Models;
using HotChocolate;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace UserService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "ADMIN" })] // dapat diakses kalau sudah login
        public IQueryable<UserData> GetUsers([Service] FoodDeliveringContext context) =>
               context.Users.Select(p => new UserData()
               {
                   Id = p.Id,
                   FullName = p.FullName,
                   Email = p.Email,
                   Username = p.Username
               });

        [Authorize]
        public IQueryable<Profile?> GetProfile([Service] FoodDeliveringContext context, ClaimsPrincipal claimsPrincipal)
        {
            var Username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(u => u.Username == Username).FirstOrDefault();
            if (user != null)
            {
                var profiles = context.Profiles.Where(p => p.UserId == user.Id);
                return profiles.AsQueryable();
            }
            return new List<Profile>().AsQueryable();
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public IQueryable<Order> GetOrder([Service] FoodDeliveringContext context) =>
            context.Orders.Include(o => o.OrderDetails).ThenInclude(f => f.Food);
    }
}
