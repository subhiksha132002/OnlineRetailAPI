using Microsoft.EntityFrameworkCore;
using OnlineRetailAPI.Models.Entities;

namespace OnlineRetailAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        } 

        public DbSet<Product> Products { get; set; }
    }
}
