namespace Model.RequestModel.Common
{
    public class DeviceInfoRequest
    {
        public string UDID { get; set; } = null!;
        public string? OSVersion { get; set; }
        public string? OSName { get; set; }
        public string? DeviceType { get; set; }
        public string? DeviceName { get; set; }
        public string? DeviceDescription { get; set; }
    }
}
