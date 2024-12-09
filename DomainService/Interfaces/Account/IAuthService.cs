using Model.RequestModel.Common;
using Model.ResponseModel.Common;

namespace DomainService.Interfaces.Account
{
    public interface IAuthService
    {
        Task<object> Login(string userName, string? password, UserDeviceRequest userDevice);
        Task<LoginResponse> GetNewTokenByRefreshToken(RefreshTokenRequest model, DeviceInfoRequest deviceInfo, string ipAddress);
        bool RevokeToken(RefreshTokenRequest model, DeviceInfoRequest deviceInfo, string ipAddress);
        bool SendOTPLoginToPhone(SendOTPLoginRequest model, DeviceInfoRequest deviceInfo);
        Task<LoginResponse> LoginByOTP(LoginByOTPRequest model, DeviceInfoRequest deviceInfo, string ipAddress);
        string GetQRLogin(DeviceInfoRequest deviceInfo);
        bool VerifyQRCode(Guid userId, LoginByQrCodeRequest model);
        Task<LoginResponse> WaitVerifyQrCode(LoginByQrCodeRequest model, DeviceInfoRequest deviceInfo, string ipAddress);
        void ClearBlackListSms(ClearBlackListSmsRequest model);
        Task<object> GetInfoGoogle(string accessToken);
    }
}
