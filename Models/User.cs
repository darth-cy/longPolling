using Microsoft.EntityFrameworkCore;

namespace AdminAPI.Models{
    public class User{
        public long Id { get; set; }
        public string Username { get; set; }
        public string SessionToken { get; set; }
        public string PasswordSalt{ get; set; }
        public string PasswordDigest { get; set; }
        public string Activities { get; set; }
        public string CreatedAt{ get; set; }
        public string UpdatedAt{ get; set; }
    }

    public class UserContext : DbContext{
        public UserContext(DbContextOptions<UserContext> options) : base(options) {}
        public DbSet<User> Users { get; set; }
    }
}