namespace RizkyAPI.Models
{
    public class TokenEntity
    {
        public long UserID { get; set; }
        public string UserName { get; set; }
        public string MainToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime FetchDate { get; set; }
        public DateTime ExpiredDate { get; set; }
    }
    public class RefreshTokenEntity
    {
        public long UserID { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ExpiredDate { get; set; }
        public char IsUsed { get; set; }
    }
}
