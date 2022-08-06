using Microsoft.EntityFrameworkCore;
using WebApplication1.Entities;
namespace WebApplication1
{
    public class PgsqlDb : DbContext
    {
        public PgsqlDb(DbContextOptions<PgsqlDb> options) : base(options) {
        }
       /* protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "melih",
                    Password = "123qwe"
                }
            );
        }*/
        public DbSet<User> Users { get; set; }
        public DbSet<Todo> Todos { get; set; }

    }

}