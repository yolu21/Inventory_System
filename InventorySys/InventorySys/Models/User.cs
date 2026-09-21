namespace InventorySys.Models
{
    public class User
    {
        public int id {  get; set; }
        public string UseName { get; set; }
        public string PasswordHash { get; set; }
    }
}
