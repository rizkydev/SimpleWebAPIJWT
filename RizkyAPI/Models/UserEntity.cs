namespace RizkyAPI.Models
{
    public class UserEntity
    {
        public long Id { get; set; }
        public string SureName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
    }

    [Serializable]
    public partial class UserLogin
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
