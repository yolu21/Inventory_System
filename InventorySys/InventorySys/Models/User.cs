namespace InventorySys.Models
{
    public class User
    {
        public int id {  get; set; }
        public string UseName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "User";
    }
}
