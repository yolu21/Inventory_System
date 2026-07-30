namespace InventorySys.Models
{
    public class ImportLog
    {
        public int id { get; set; }
        public string UserName { get; set; } = "";
        public string FileName { get; set; } = "";
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int TotalCount { get; set; }
        public DateTime ImportTime { get; set; }
        public bool IsSuccess { get; set; }
        public string ? ErrMsg { get; set; }


    }
}
