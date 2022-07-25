using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Models;

namespace Data
{
    public class ApiDbContext : IdentityDbContext
    {
        public virtual DbSet<ItemData>? Items { get; set; }
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {
        }
    }
}