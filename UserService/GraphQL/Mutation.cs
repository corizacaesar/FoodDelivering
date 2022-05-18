using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Library.Models;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models;

namespace UserService.GraphQL
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<UserData> AddUserAsync(
               RegisterUser input,
               [Service] FoodDeliveringContext context)
        {
            var user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            };

            context.Users.Add(newUser);
            await context.SaveChangesAsync();
            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                FullName = newUser.FullName
            });
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<UserData> UpdateUserAsync(
               UserData input,
               [Service] FoodDeliveringContext context
               )
        {
            var user = context.Users.Where(s => s.Id == input.Id).FirstOrDefault();
            if (user != null)
            {
                user.FullName = input.FullName;
                user.Email = input.Email;
                user.Username = input.Username;
                user.Password = BCrypt.Net.BCrypt.HashPassword(input.Password);
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(new UserData
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName
            });
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<UserData> DeleteUserAsync(
            int id,
            [Service] FoodDeliveringContext context
            )
        {
            var user = context.Users.Where(s => s.Id == id).FirstOrDefault();
            if (user != null)
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(new UserData
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName
            });
        }

        public async Task<UserData> RegisterUserAsync(
               RegisterUser input,
               [Service] FoodDeliveringContext context)
        {
            var user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            };

            // EF
            var transaction = context.Database.BeginTransaction();
            try
            {
                context.Users.Add(newUser);
                context.SaveChanges();

                var userRole = new UserRole
                {
                    UserId = newUser.Id,
                    RoleId = 2 //BUYER
                };

                context.UserRoles.Add(userRole);
                context.SaveChanges();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                transaction.Rollback();
            }

            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                FullName = newUser.FullName
            });
        }


        public async Task<UserToken> LoginAsync(
            LoginUser input,
            [Service] IOptions<TokenSettings> tokenSettings, // setting token
            [Service] FoodDeliveringContext context) // EF
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);
            if (valid)
            {
                // generate jwt token
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                // jwt payload
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));

                var userRoles = context.UserRoles.Where(o => o.UserId == user.Id).ToList();
                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.Where(o => o.Id == userRole.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(3);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,
                    claims: claims, // jwt payload
                    signingCredentials: credentials // signature
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
                //return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }

        [Authorize]
        public async Task<ResponseChangePassword> ChangePasswordAsync(
            ChangePassword input,
            ClaimsPrincipal claimsPrincipal,// setting token
            [Service] FoodDeliveringContext context) // EF
        {
            var Username = claimsPrincipal.Identity.Name;

            var user = context.Users.Where(o => o.Username == Username).FirstOrDefault();
            bool valid = BCrypt.Net.BCrypt.Verify(input.CurrentPassword, user.Password);

            if (valid)
            {
                if (input.NewPassword != input.ConfirmPassword)
                {
                    return await Task.FromResult(new ResponseChangePassword(Message: "New Password and Confirmation Password Are Not The Same", Created: DateTime.Now.ToString()));
                }
                user.Password = BCrypt.Net.BCrypt.HashPassword(input.ConfirmPassword);
                context.Users.Update(user);
                context.SaveChangesAsync();
                return await Task.FromResult(new ResponseChangePassword(Message: "Password Has Been Updated", Created: DateTime.Now.ToString()));
            }
            return await Task.FromResult(new ResponseChangePassword(Message: "Failed To Update Password", Created: DateTime.Now.ToString()));
        }
    }
}
