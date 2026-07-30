namespace InventorySys.DTOs
{
    public class ImportRequestDto
    {
        public string FileName { get; set; } = "";

        public List<ImportInventoryDto> Data { get; set; } = new();
    }
}
