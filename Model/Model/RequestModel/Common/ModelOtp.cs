namespace Model.RequestModel.Common
{
    public class ModelOtp
    {
        public required string Code { get; set; }
        public DateTime Expire { get; set; } = DateTime.Now.AddMinutes(5);
        public required string UDID { get; set; }
        public int NumCheck { get; set; } = 0;
        public bool IsVerify { get; set; } = false;
        public Guid userVerifiedId { get; set; } = Guid.Empty;
    }
}
