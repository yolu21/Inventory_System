using Microsoft.EntityFrameworkCore;
using InventorySys.Models;
namespace InventorySys.Data
{
    public class InventoryDbContext:DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
        {
        }

        public DbSet<Ingredients> Ingredients { get; set; }
        public DbSet<StockRecord> StockRecords { get; set; }
        public DbSet<ImportLog> ImportLog { get; set; }
    }
}
