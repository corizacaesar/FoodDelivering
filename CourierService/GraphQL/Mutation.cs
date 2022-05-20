using CourierService.Models;
using HotChocolate.AspNetCore.Authorization;
using Library.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            var role = context.UserRoles.Where(u => u.UserId == input.Id).FirstOrDefault();
            var courier = context.Users.Where(o => o.Id == input.Id && role.RoleId == 4).FirstOrDefault();
            if (courier != null)
            {
                courier.FullName = input.FullName;
                courier.Email = input.Email;
                courier.Username = input.Username;
                courier.Password = BCrypt.Net.BCrypt.HashPassword(input.Password);
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

            var role = context.UserRoles.Where(u => u.UserId == id).FirstOrDefault();
            var courier = context.Users.Where(o => o.Id == id && role.RoleId == 4).FirstOrDefault();
            if (courier != null)
            {
                context.UserRoles.Remove(role);
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

        [Authorize(Roles = new[] { "COURIER" })]
        public async Task<OrderOutput> UpdateLokasiAsync(
            OrderInputLokasi input,
            [Service] FoodDeliveringContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var Username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == Username).FirstOrDefault();
            var order = context.Orders.Where(o => o.Id == input.OrderId).FirstOrDefault();

            if (order != null)
            {
                if(order.CourierId == user.Id)
                {

                    order.Latitude = input.Latitude;
                    order.Longitude = input.Longitude;


                    context.Orders.Update(order);
                    await context.SaveChangesAsync();
                    return await Task.FromResult(new OrderOutput("Berhasil Menambahkan Lokasi"));
                }
            }

            return await Task.FromResult(new OrderOutput("Gagal Menambahkan Lokasi"));
        }


        [Authorize(Roles = new[] { "COURIER" })]
        public async Task<OrderOutput> CompleteOrderAsync(
            int id,
            [Service] FoodDeliveringContext context,
            ClaimsPrincipal claimsPrincipal)
        {
            var Username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == Username).FirstOrDefault();
            var order = context.Orders.Where(o => o.Id == id).FirstOrDefault();

            if (order != null)
            {
                if(order.CourierId == user.Id)
                {
                    if (order.Status == false)
                    {

                        order.Status = true;


                        context.Orders.Update(order);
                        await context.SaveChangesAsync();
                        return await Task.FromResult(new OrderOutput("Order Telah Selesai"));
                    }
                    else
                    {
                        return await Task.FromResult(new OrderOutput("Order Sudah Selesai"));
                    }
                }
                else
                {
                    return await Task.FromResult(new OrderOutput("Anda Bukan Courier Pada Order Berikut"));
                }
            }
            
            return await Task.FromResult(new OrderOutput("Order Tidak Ada"));
        }
    }
}
