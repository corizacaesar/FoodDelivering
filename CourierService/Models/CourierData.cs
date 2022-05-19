using Library.Models;

namespace CourierService.Models
{
    public class CourierData
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string? Password { get; set; }
        public int RoleId { get; set; } = 0;

        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
