using Microsoft.EntityFrameworkCore;


namespace AdminAPI.Models{
    public class Post{
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Content { get; set; }
    }
}

namespace AdminAPI.Models{
    public class PostContext : DbContext{
        public PostContext(DbContextOptions<PostContext> options) :base(options) {}
        public DbSet<Post> Posts { get; set; }
    }
}