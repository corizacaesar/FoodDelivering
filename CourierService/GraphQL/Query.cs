using CourierService.Models;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace CourierService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] { "MANAGER" })] // dapat diakses kalau sudah login
        public IQueryable<CourierData> GetCouriers([Service] FoodDeliveringContext context) =>
               context.Users.Include(r => r.UserRoles).Where(user => user.UserRoles.Any(i => i.RoleId == 4)).Select(p => new CourierData()
               {
                   Id = p.Id,
                   FullName = p.FullName,
                   Email = p.Email,
                   Username = p.Username
               });
    }
}
