namespace Model.ResponseModel.Common
{
    public class RfTokenResponse
    {
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreatedByIp { get; set; } = null!;
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }
    }
}
