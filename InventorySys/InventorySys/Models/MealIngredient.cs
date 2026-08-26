namespace InventorySys.Models
{
    public class MealIngredient
    {
        public int Id { get; set; }
        public int MealId { get; set; } 
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }

    }
}
