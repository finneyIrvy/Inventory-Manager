using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_System__Miracle_Shop_.Models
{
    public class MiracleDbContext: IdentityDbContext<NewUserClass>
    {
        public MiracleDbContext(DbContextOptions<MiracleDbContext> options)
           : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
        }
    }
}
