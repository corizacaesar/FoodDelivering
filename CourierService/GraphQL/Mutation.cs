using CourierService.Models;
using HotChocolate.AspNetCore.Authorization;
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace CourierService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<CourierData> AddCourierAsync(
            CourierInput input,
            [Service] FoodDeliveringContext context)
        {
            var courier = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (courier != null)
            {
                return await Task.FromResult(new CourierData());
            }
            // EF
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                Username = input.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            };

            var transaction = context.Database.BeginTransaction();
            try
            {
                context.Users.Add(newUser);
                context.SaveChanges();

                var userRole = new UserRole
                {
                    UserId = newUser.Id,
                    RoleId = 4 //Courier
                };

                context.UserRoles.Add(userRole);
                context.SaveChanges();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                transaction.Rollback();
            }

            return await Task.FromResult(new CourierData
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                FullName = newUser.FullName
            });
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<CourierData> UpdateCourierAsync(
            CourierInput input,
            [Service] FoodDeliveringContext context)
        {
            var courier = context.Users.Where(o => o.Id == input.Id).FirstOrDefault();
            if (courier != null)
            {
                courier.FullName = input.FullName;
                courier.Email = input.Email;
                courier.Username = input.Username;
                courier.Password = BCrypt.Net.BCrypt.HashPassword(input.Password);
                context.Users.Update(courier);
                await context.SaveChangesAsync();

                context.Users.Update(courier);
                await context.SaveChangesAsync();
            }


            return await Task.FromResult(new CourierData
            {
                Id = courier.Id,
                Username = courier.Username,
                Email = courier.Email,
                FullName = courier.FullName
            });
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<CourierData> DeleteCourierAsync(
            int id,
            [Service] FoodDeliveringContext context)
        {
            var courier = context.Users.Where(o => o.Id == id).Include(user => user.UserRoles).FirstOrDefault();
            if (courier != null)
            {
                context.Users.Remove(courier);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(new CourierData
            {
                Id = courier.Id,
                Username = courier.Username,
                Email = courier.Email,
                FullName = courier.FullName
            });
        }
    }
}
