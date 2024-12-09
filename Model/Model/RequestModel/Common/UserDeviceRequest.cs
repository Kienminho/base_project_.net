namespace Model.RequestModel.Common;
public class UserDeviceRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DeviceUUID { get; set; } = null!;
    public string? AppName { get; set; }
    public string? AppBuild { get; set; }
    public string? AppVersion { get; set; }
    public string? AppLanguage { get; set; }
    public string? SessionToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? PushToken { get; set; }
    public string? DevicePlatform { get; set; }
    public string? DeviceOS { get; set; }
    public string? DeviceModel { get; set; }
    public string? DeviceName { get; set; }
    public string? DeviceLocation { get; set; }
    public bool? IsNotify { get; set; }
    public bool? IsActivate { get; set; }
    public bool? IsMainDevice { get; set; }
    public int AppBuildValue
    {
        get
        {
            try
            {
                return int.Parse(AppBuild);
            }
            catch { return 0; }
        }
    }
}
