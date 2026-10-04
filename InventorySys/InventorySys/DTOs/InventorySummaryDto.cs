namespace InventorySys.DTOs
{
    public class InventorySummaryDto
    {
        public int TotalIngredients { get; set; }//食材總數

        public int NeedReplenishment { get; set; }//需要補貨的食材數量

        public decimal TotalEstimatedCost { get; set; }//預估總成本
    }
}