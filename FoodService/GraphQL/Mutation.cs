using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate;
using System.Linq;
using System.Threading.Tasks;
using Library.Models;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace FoodService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Food> AddFoodAsync(
            FoodInput input,
            [Service] FoodDeliveringContext context)
        {

            // EF
            var food = new Food
            {
                Name = input.Name,
                Stock = input.Stock,
                Price = input.Price,
                Created = DateTime.Now
            };

            var ret = context.Foods.Add(food);
            await context.SaveChangesAsync();

            return ret.Entity;
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Food> UpdateFoodAsync(
            FoodInput input,
            [Service] FoodDeliveringContext context)
        {
            var food = context.Foods.Where(o => o.Id == input.Id).FirstOrDefault();
            if (food != null)
            {
                food.Name = input.Name;
                food.Stock = input.Stock;
                food.Price = input.Price;

                context.Foods.Update(food);
                await context.SaveChangesAsync();
            }


            return await Task.FromResult(food);
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Food> DeleteFoodAsync(
            int id,
            [Service] FoodDeliveringContext context)
        {
            var product = context.Foods.Where(o => o.Id == id).FirstOrDefault();
            if (product != null)
            {
                context.Foods.Remove(product);
                await context.SaveChangesAsync();
            }


            return await Task.FromResult(product);
        }


        [Authorize(Roles = new[] { "BUYER" })]
        public async Task<OrderOutput> AddOrderByBuyerAsync(
            OrderInput input,
            [Service] FoodDeliveringContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            
            var Username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == Username).FirstOrDefault();
            var food = context.Foods.Where(f => f.Name == input.Product).FirstOrDefault();

            using var transaction = context.Database.BeginTransaction();
            try
            {
                    var order = new Order
                    {
                        UserId = user.Id,
                        Code = DateTime.Now.ToString(),
                        Status = false,
                        Created = DateTime.Now
                    };
                    context.Orders.Add(order);
                    await context.SaveChangesAsync();

                    var orderDetail = new OrderDetail
                    {
                        Quantity = input.Quantity,
                        OrderId = order.Id,
                        FoodId = food.Id
                    };
                    context.OrderDetails.Add(orderDetail);
                    await context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return await Task.FromResult(new OrderOutput(DateTime.Now.ToString(),
                        "Berhasil Memesan Makanan"));
                
            }
            catch
            {
                transaction.Rollback();
            }
            return await Task.FromResult(new OrderOutput(DateTime.Now.ToString(), "Gagal Memesan Makanan"));
        }
    }
}
