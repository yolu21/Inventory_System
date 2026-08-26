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
        public DbSet<Meal>Meals { get; set; }
        public DbSet<MealIngredient> MealIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MealIngredient>()
                .HasOne<Meal>().WithMany().HasForeignKey(x => x.MealId);
            
            modelBuilder.Entity<MealIngredient>()
                .HasOne<Ingredients>().WithMany().HasForeignKey(x => x.IngredientId);
        }
    }

    
}
