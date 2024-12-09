using Common.Authorization;
using Common.Authorization.Utils;
using Common.Constant;
using Common.Settings;
using Common.UnitOfWork.UnitOfWorkPattern;
using Common.Utils;
using DomainService.Interfaces.Account;
using Entity.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Model.RequestModel.Common;
using Model.ResponseModel.Common;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;

namespace Infrastructure.Implements.Account
{
    public class AuthService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IJwtUtils _jwtUtils, IOptions<StrJWT> strJwt, IOptions<AppSettings> appSettings) : BaseService(unitOfWork, memoryCache), IAuthService
    {
        private ExchangeService exchangeService;
        private readonly StrJWT _strJwt = strJwt.Value;
        private readonly AppSettings _appSettings = appSettings.Value;
        private const string DOMAIN_NAME = "@vietnamairlines.com";
        public async Task<object> Login(string userName, string? password, UserDeviceRequest userDevice)
        {
            var email = $"{userName}{DOMAIN_NAME}";
            var account = await _unitOfWork.Repository<SysAccount>()
                                           .FirstOrDefaultAsync(s => ((s.Email ?? "").Equals(email) ||
                                                               (s.PhoneNumber ?? "").Equals(userName)) && s.IsDeleted != true)
                                           ?? throw new AppException("Tên tài khoản hoặc mật khẩu bị lỗi!");

            if (RegexUtilities.IsValidEmail(email))
            {
#if !DEBUG
                var isAuth = AuthenticateEmail(userName, password ?? "");
                if (!isAuth) throw new AppException("Tên tài khoản hoặc mật khẩu bị lỗi!");

#endif
                var deviceExist = await _unitOfWork.Repository<SysDevice>()
                                                   .FirstOrDefaultAsync(s => s.UserId == account.Id && s.IsActive == true &&
                                                                        s.UDID == userDevice.DeviceUUID);

                var accesToken = _jwtUtils.GenerateToken(account.Id, account.FullName ?? "", userDevice.DeviceUUID, account.UserName);
                var refreshToken = _jwtUtils.GenerateRefreshToken(account.Id, account.FullName ?? "", account.UserName, userDevice.DeviceUUID,
                    _strJwt.Key, _strJwt.Issuer, _strJwt.Audience, "");

                #region check device

                var deviceInfo = new DeviceInfoRequest
                {
                    UDID = userDevice.DeviceUUID ?? "",
                    DeviceName = userDevice.DeviceName,
                    OSName = userDevice.DeviceOS,
                    OSVersion = userDevice.DevicePlatform
                };

                await CheckDevice(account.Id, refreshToken, deviceInfo);

                #endregion check device

                var loginResponse = new LoginResponse
                {
                    UserId = account.Id,
                };
                loginResponse.SetToken(accesToken);
                loginResponse.SetRefreshToken(refreshToken.Token);

                return Utils.CreateResponseModel(loginResponse);
            }
            else if (RegexUtilities.IsValidPhone(userName))
            {
#if !DEBUG
                var deviceInfo = new DeviceInfoRequest
                {
                    UDID = userDevice.DeviceUUID ?? "",
                    DeviceName = userDevice.DeviceName,
                    OSName = userDevice.DeviceOS,
                    OSVersion = userDevice.DevicePlatform
                };

                var isSendOtp = SendOTPLoginToPhone(new SendOTPLoginRequest { UserPhone = userName }, deviceInfo);

                return Utils.CreateResponseModel(isSendOtp);
#else
                return Utils.CreateResponseModel(true);
#endif
            }

            throw new AppException("Tên tài khoản hoặc mật khẩu bị lỗi!");
        }

        public async Task<LoginResponse> GetNewTokenByRefreshToken(RefreshTokenRequest model, DeviceInfoRequest deviceInfo, string ipAddress)
        {
            var user = await _unitOfWork.Repository<SysAccount>().Where(a => a.IsDeleted != true)
                                  .Join(_unitOfWork.Repository<SysDevice>()
                                                        .Where(d => d.RefreshToken.Equals(model.RefreshToken) &&
                                                               d.UDID.Equals(deviceInfo.UDID)),
                                        a => a.Id, d => d.UserId,
                                       (a, d) => new
                                       {
                                           Account = a,
                                           Device = d
                                       })
                                  .FirstOrDefaultAsync()
                                  ?? throw new KeyNotFoundException(StatusMessage.DataNotFound);

            var device = user.Device;

            // return null if token is no longer active
            if (device.RfTokenExpiryTime <= DateTime.UtcNow || device.RfTokenRevokedTime != null ||
                device.RfTokenRevokedByIp != null || device.RfTokenCreatedByIp != ipAddress)
                throw new AppException(StatusMessage.DataInputInvalid);

            // replace old refresh token with a new one and save
            string? skey = _strJwt.Key;
            string? issuer = _strJwt.Issuer;
            string? audience = _strJwt.Audience;
            var newRefreshToken = _jwtUtils.GenerateRefreshToken(user.Account.Id, user.Account.FullName, user.Account.UserName, deviceInfo.UDID, skey, issuer, audience, ipAddress);
            device.RefreshToken = newRefreshToken.Token;
            device.RfTokenCreateTime = newRefreshToken.CreateTime;
            device.RfTokenExpiryTime = newRefreshToken.Expires;
            device.RfTokenCreatedByIp = newRefreshToken.CreatedByIp;
            device.RfTokenRevokedTime = null;
            device.RfTokenRevokedByIp = null;
            _unitOfWork.Repository<SysDevice>().Update(device);
            _unitOfWork.Repository<SysAccount>().Update(user.Account);
            await _unitOfWork.SaveChangesAsync();

            var jwtToken = _jwtUtils.GenerateToken(user.Account.Id, user.Account.FullName, user.Account.UserName, deviceInfo.UDID);
            var res = new LoginResponse();
            /*res.SetToken(jwtToken);
            res.SetRefreshToken(newRefreshToken.Token);*/

            return res;
        }

        public bool RevokeToken(RefreshTokenRequest model, DeviceInfoRequest deviceInfo, string ipAddress)
        {
            /*var user = _unitOfWork.Repository<ViagsUser>().Include(s => s.Devices).SingleOrDefault(
                s => s.IsDelete != true && s.Devices.Any(t => t.UDID == deviceInfo.UDID && (t.RefreshToken == model.RefreshToken)));

            // return false if no user found with token
            if (user == null) throw new KeyNotFoundException("Token not found");

            var device = user.Devices.Single(x => x.UDID == deviceInfo.UDID && x.RefreshToken == model.RefreshToken);

            // return false if token is not active
            if (device.RfTokenExpiryTime <= DateTime.UtcNow || device.RfTokenRevokedTime != null ||
                device.RfTokenRevokedByIp != null || device.RfTokenCreatedByIp != ipAddress)
                throw new AppException("Token already expires");

            // revoke token and save
            device.RfTokenRevokedTime = DateTime.UtcNow;
            device.RfTokenRevokedByIp = ipAddress;
            _unitOfWork.Repository<Entity.Entities.Device>().Update(device);
            _unitOfWork.Repository<ViagsUser>().Update(user);
            _unitOfWork.SaveChangesAsync();
            _unitOfWork.Dispose();*/

            return true;
        }

        public bool SendOTPLoginToPhone(SendOTPLoginRequest model, DeviceInfoRequest deviceInfo)
        {
            if (!RegexUtilities.IsValidPhone(model.UserPhone)) throw new AppException("Phone is wrong!");
            var otpCode = Utils.GenerateOneTimeOTP();
            var modelOtp = new ModelOtp
            {
                Code = otpCode,
                UDID = deviceInfo.UDID
            };

            Dictionary<string, int> dataBlackList;
            int numSent = 0;

            if (_memoryCache.TryGetValue(CacheKey.BlackListSms, out dataBlackList))
            {
                if (dataBlackList.TryGetValue(model.UserPhone, out numSent) && numSent >= 3)
                    throw new AppException("Too many sms sent, please try again in 24 hours!");
            }
            else
            {
                dataBlackList = new Dictionary<string, int>();
            }

            var res = SmsUtils.SendOTPToPhone(model.UserPhone, otpCode, _appSettings.SmsToken, _appSettings.SmsServiceUrl);
            if (res)
            {
                numSent += 1;
                dataBlackList[model.UserPhone] = numSent;
                _memoryCache.Set(CacheKey.BlackListSms, dataBlackList, CacheTime.BlackList);
            }

            _memoryCache.Set(model.UserPhone, modelOtp, CacheTime.OTP);
            return res;
        }

        public async Task<LoginResponse> LoginByOTP(LoginByOTPRequest model, DeviceInfoRequest deviceInfo, string ipAddress)
        {
            //Check phone to get user
            var user = _unitOfWork.Repository<SysAccount>()
                .FirstOrDefault(s => (s.PhoneNumber ?? "").Equals(model.UserPhone) && s.IsDeleted != true);
            if (user == null) throw new AppException("Phone is not found or not register!");
#if !DEBUG
            //Check otp
            ModelOtp otpSaved;
            if (!_memoryCache.TryGetValue(model.UserPhone, out otpSaved)) throw new AppException("Phone is wrong or OTP was expired, please re-enter the OTP");
            otpSaved.NumCheck += 1;
            if (otpSaved.NumCheck > 3) throw new AppException("OTP has been entered too many times, please re-enter the OTP");
            if (otpSaved.Expire < DateTime.Now) throw new AppException("OTP was expired, please re-enter the OTP");
            if (otpSaved.Code != model.OTP) throw new AppException("OTP is wrong!");
            if (otpSaved.UDID != deviceInfo.UDID) throw new AppException("Device is wrong!");

#else
            var otpdefault = $"89{DateTime.Now.ToString("yyMM")}";
            if (!model.OTP.Equals(otpdefault))
                throw new AppException("OTP is wrong!");
#endif
            string? skey = _strJwt.Key;
            string? issuer = _strJwt.Issuer;
            string? audience = _strJwt.Audience;
            var refreshToken = _jwtUtils.GenerateRefreshToken(user.Id, user.FullName, user.UserName, deviceInfo.UDID, skey,
                issuer, audience, ipAddress);

            #region check device

            await CheckDevice(user.Id, refreshToken, deviceInfo);

            #endregion check device

            var jwtToken = _jwtUtils.GenerateToken(user.Id, user.FullName, deviceInfo.UDID, user.UserName);
            var res = new LoginResponse();
            res.SetToken(jwtToken);
            res.SetRefreshToken(refreshToken.Token);

            return res;
        }

        public string GetQRLogin(DeviceInfoRequest deviceInfo)
        {
            var qr = Guid.NewGuid().ToString();
            var qrCode = Utils.Encode64(qr);
            var modelQr = new ModelOtp
            {
                Code = qrCode,
                UDID = deviceInfo.UDID
            };
            _memoryCache.Set(qr, modelQr, CacheTime.QrCode);
            return qrCode;
        }

        public bool VerifyQRCode(Guid userId, LoginByQrCodeRequest model)
        {
            /*var qr = Utils.Decode64(model.QrCode);
            var user = _unitOfWork.Repository<ViagsUser>().FirstOrDefault(s =>
                s.Id == userId && (s.IsDelete == null || s.IsDelete == false));//s.Phone == userPhone
            if (user == null) throw new AppException("User is not found!");

            //Check otp
            ModelOtp modelQr;
            if (!_memoryCache.TryGetValue(qr, out modelQr)) throw new AppException("Phone is wrong or OTP was expired, please re-enter the OTP");
            modelQr.NumCheck += 1;
            if (modelQr.NumCheck > 3) throw new AppException("OTP has been entered too many times, please re-enter the OTP");
            if (modelQr.Expire < DateTime.Now) throw new AppException("OTP was expired, please re-enter the OTP");
            if (modelQr.Code != model.QrCode) throw new AppException("OTP is wrong!");

            modelQr.userVerifiedId = user.Id;
            modelQr.IsVerify = true;*/
            return true;
        }

        //Nhac front end tang t/g time out call api
        public async Task<LoginResponse> WaitVerifyQrCode(LoginByQrCodeRequest model, DeviceInfoRequest deviceInfo, string ipAddress)
        {
            /*var qr = Utils.Decode64(model.QrCode);
            while (!CheckVerifyQrCode(qr, deviceInfo.UDID))
            {
                await Task.Delay(2000);
            }

            ModelOtp modelQr;
            if (!_memoryCache.TryGetValue(qr, out modelQr)) throw new AppException("QRCode was expired");
            //Check phone to get user
            var user = _unitOfWork.Repository<ViagsUser>()
                .FirstOrDefault(s => s.Id == modelQr.userVerifiedId && (s.IsDelete == null || s.IsDelete == false));//s.Phone == userPhone
            if (user == null) throw new AppException("Phone is not found or not register!");

            //Response
            var device = _unitOfWork.Repository<Device>().FirstOrDefault(s => s.UDID == deviceInfo.UDID);

            string? skey = _strJwt.Key;
            string? issuer = _strJwt.Issuer;
            string? audience = _strJwt.Audience;
            var refreshToken = _jwtUtils.GenerateRefreshToken(user.Id, user.UserName, deviceInfo.UDID, skey,
                issuer, audience, ipAddress);

            if (device == null)
            {
                device = new Entity.Entities.Device
                {
                    UserId = user.Id,
                    UDID = deviceInfo.UDID,
                    OSVersion = deviceInfo.OSVersion,
                    OSName = deviceInfo.OSName,
                    DeviceType = deviceInfo.DeviceType,
                    DeviceName = deviceInfo.DeviceName,
                    DeviceDescription = deviceInfo.DeviceDescription,
                    IsActive = true,
                    RefreshToken = refreshToken.Token,
                    RfTokenCreateTime = refreshToken.CreateTime,
                    RfTokenExpiryTime = refreshToken.Expires,
                    RfTokenCreatedByIp = refreshToken.CreatedByIp,

                    CreatedDate = DateTime.Now,
                    CreatedUser = user.Id,
                };
                _unitOfWork.Repository<Device>().AddAsync(device);
            }
            else
            {
                device.UserId = user.Id;
                device.IsActive = true;
                device.RefreshToken = refreshToken.Token;
                device.RfTokenCreateTime = refreshToken.CreateTime;
                device.RfTokenExpiryTime = refreshToken.Expires;
                device.RfTokenCreatedByIp = refreshToken.CreatedByIp;
                device.RfTokenRevokedTime = null;
                device.RfTokenRevokedByIp = null;
                device.IsDelete = false;

                device.UpdatedDate = DateTime.Now;
                device.UpdatedUser = user.Id;

                _unitOfWork.Repository<Entity.Entities.Device>().Update(device);
            }
            _unitOfWork.Dispose();

            var jwtToken = _jwtUtils.GenerateToken(user.Id, user.UserName, deviceInfo.UDID);*/
            var res = new LoginResponse();
            /*res.SetToken(jwtToken);
            res.SetRefreshToken(refreshToken.Token);*/

            return res;
        }

        public void ClearBlackListSms(ClearBlackListSmsRequest model)
        {
            var keyBlackList = "BlackListSms";
            if (model.UserPhone != null)
            {
                Dictionary<string, int> dataBlackList;
                if (_memoryCache.TryGetValue(keyBlackList, out dataBlackList))
                {
                    dataBlackList.Remove(model.UserPhone);
                }
            }
            else
                _memoryCache.Remove(keyBlackList);
        }

        private bool CheckVerifyQrCode(string keyGetQrCode, string UDID)
        {
            //Check otp
            ModelOtp qrSaved;
            if (!_memoryCache.TryGetValue(keyGetQrCode, out qrSaved)) throw new AppException("QRCode was expired");
            if (qrSaved.Expire < DateTime.Now) throw new AppException("QRCode was expired");
            if (qrSaved.UDID != UDID) throw new AppException("Wrong device!");

            return qrSaved.IsVerify;
        }

        public async Task<object> GetInfoGoogle(string accessToken)
        {
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var info = JsonConvert.DeserializeObject<InfoGoogleResponse>(content);

                var userExist = await _unitOfWork.Repository<SysAccount>()
                                                 .FirstOrDefaultAsync(u => (u.Email ?? "").Equals(info.Email) && u.IsDeleted != true);

                if (userExist == null)
                    throw new AppException("Tài khoản chưa tồn tại trong hệ thống, vui lòng liên hệ admin!");

                var jwtToken = _jwtUtils.GenerateToken(userExist.Id, userExist.FullName, Guid.Empty.ToString(), userExist.UserName);
                string? skey = _strJwt.Key;
                string? issuer = _strJwt.Issuer;
                string? audience = _strJwt.Audience;
                var refreshToken = _jwtUtils.GenerateRefreshToken(userExist.Id, userExist.FullName, userExist.UserName, Guid.Empty.ToString(), skey,
                    issuer, audience, "");

                var retUser = new LoginResponse
                {
                    UserId = userExist.Id
                };

                retUser.SetToken(jwtToken);
                retUser.SetRefreshToken(refreshToken.Token);

                return Utils.CreateResponseModel(retUser);
            }
        }

        #region Private Methods

        private bool AuthenticateEmail(string userName, string password)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            exchangeService = new ExchangeService(ExchangeVersion.Exchange2010_SP1)
            {
                Url = new Uri("https://mail.vietnamairlines.com/EWS/Exchange.asmx"),
                Credentials = new WebCredentials(userName, password)
            };

            // Replace "emailToCheck" with the email address you want to validate
            var emailToCheck = $"{userName}{DOMAIN_NAME}";

            // Perform a search in the Exchange server's directory
            var resolutions = exchangeService.ResolveName(emailToCheck, ResolveNameSearchLocation.DirectoryOnly, true);

            return resolutions.Count > 0;
        }

        private async System.Threading.Tasks.Task CheckDevice(Guid userId, RfTokenResponse refreshToken, DeviceInfoRequest deviceInfo)
        {
            var device = await _unitOfWork.Repository<SysDevice>()
                                               .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive == true &&
                                                                    s.UDID == deviceInfo.UDID);
            if (device == null)
            {
                device = new SysDevice
                {
                    UserId = userId,
                    UDID = deviceInfo.UDID,
                    OSVersion = deviceInfo.OSVersion,
                    OSName = deviceInfo.OSName,
                    DeviceType = deviceInfo.DeviceType,
                    DeviceName = deviceInfo.DeviceName,
                    DeviceDescription = deviceInfo.DeviceDescription,
                    IsActive = true,
                    RefreshToken = refreshToken.Token,
                    RfTokenCreateTime = refreshToken.CreateTime,
                    RfTokenExpiryTime = refreshToken.Expires,
                    RfTokenCreatedByIp = refreshToken.CreatedByIp,
                    CreatedDate = DateTime.Now,
                    CreatedById = userId,
                };
                _unitOfWork.Repository<SysDevice>().Add(device);
            }
            else
            {
                device.UserId = userId;
                device.IsActive = true;
                device.RefreshToken = refreshToken.Token;
                device.RfTokenCreateTime = refreshToken.CreateTime;
                device.RfTokenExpiryTime = refreshToken.Expires;
                device.RfTokenCreatedByIp = refreshToken.CreatedByIp;
                device.RfTokenRevokedTime = null;
                device.RfTokenRevokedByIp = null;
                device.IsDeleted = false;
                device.UpdatedDate = DateTime.Now;
                device.UpdatedById = userId;

                _unitOfWork.Repository<SysDevice>().Update(device);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        #endregion Private Methods
    }
}
