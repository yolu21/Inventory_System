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
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // MealIngredient → Meal
            modelBuilder.Entity<MealIngredient>()
                .HasOne<Meal>()
                .WithMany()
                .HasForeignKey(x => x.MealId);

            // MealIngredient → Ingredients
            modelBuilder.Entity<MealIngredient>()
                .HasOne<Ingredients>()
                .WithMany()
                .HasForeignKey(x => x.IngredientId);

            // StockRecord → Ingredients
            modelBuilder.Entity<StockRecord>()
                .HasOne<Ingredients>()
                .WithMany()
                .HasForeignKey(x => x.IngredientId);

            // 食材單位成本
            modelBuilder.Entity<Ingredients>()
                .Property(x => x.UnitCost)
                .HasPrecision(18, 2);

            // 食材最低庫存量
            modelBuilder.Entity<Ingredients>()
                .Property(x => x.MinimumStock)
                .HasPrecision(18, 2);

            // 庫存異動數量
            modelBuilder.Entity<StockRecord>()
                .Property(x => x.Quantity)
                .HasPrecision(18, 2);
            // 食材名稱不可重複
            modelBuilder.Entity<Ingredients>()
                .HasIndex(x => x.Name)
                .IsUnique();

            // 庫存紀錄關聯食材，刪除時不自動連鎖刪除
            modelBuilder.Entity<StockRecord>()
                .HasOne(x => x.Ingredients)
                .WithMany()
                .HasForeignKey(x => x.IngredientId)
                .OnDelete(DeleteBehavior.NoAction);
            // 餐點名稱不可重複
            modelBuilder.Entity<Meal>()
                .HasIndex(x => x.Name)
                .IsUnique();
        }
    }

    
}
