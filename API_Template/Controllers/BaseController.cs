using Common.Authorization;
using Common.Constant;
using Common.Enum;
using Common.Utils;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Model.RequestModel.Common;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Controllers;

public class BaseController : ControllerBase
{
    protected readonly ILog Log = LogManager.GetLogger(typeof(BaseController));
    protected string userId;
    protected string username;
    protected Guid currentUserId;
    protected string currentCrewId;
    protected int currentDeviceId;
    public string currentUserEmail;
    protected string ipAddress;

    public BaseController(IHttpContextAccessor httpContextAccessor)
    {
        var isMicrosoftCheckJwt = bool.Parse(httpContextAccessor.HttpContext?.Items[ContextItemsKey.EnableMicrosoftCheckJwt]?.ToString() ?? "false");
        if (isMicrosoftCheckJwt)
            GetDataFromTokenMicrosoft(httpContextAccessor);
        else
            GetDataFromTokenCustom(httpContextAccessor);
    }

    protected OkObjectResult Ok([ActionResultObjectValue] object? value, int totalRecord = 0)
    {
        return Ok(value, new object(), totalRecord);
    }

    protected OkObjectResult Ok([ActionResultObjectValue] object? value, object body, int totalRecord = 0)
    {
        Utils.WriteLogRequest(Log, "INFO", userId, JsonConvert.SerializeObject(body), Request);
        return base.Ok(Utils.CreateResponseModel(value, totalRecord));
    }

    protected OkObjectResult Error(Exception e)
    {
        Utils.WriteLogRequest(Log, "ERROR", userId, JsonConvert.SerializeObject(e.Message), Request);
        return base.Ok(Utils.CreateErrorModel<object>(message: e.Message, exception: e));
    }

    protected UserDeviceRequest GetRequestDeviceInfo(HttpRequest request)
    {
        try
        {
            string? deviceUUID = Utils.GetValueByKeyName(request, APIResourceRequest.X_DEVICE_UDID);
            string? appName = Utils.GetValueByKeyName(request, APIResourceRequest.X_APP_NAME);
            string? appVersion = Utils.GetValueByKeyName(request, APIResourceRequest.X_APP_VERSION);
            string? appBuild = Utils.GetValueByKeyName(request, APIResourceRequest.X_APP_BUILD);
            string? deviceOS = Utils.GetValueByKeyName(request, APIResourceRequest.X_DEVICE_OS);
            string? devicePlatform = Utils.GetValueByKeyName(request, APIResourceRequest.X_DEVICE_PLATFORM);
            string? deviceModel = Utils.GetValueByKeyName(request, APIResourceRequest.X_DEVICE_MODEL);
            string? deviceName = Utils.GetValueByKeyName(request, APIResourceRequest.X_DEVICE_NAME);
            string? pushToken = Utils.GetValueByKeyName(request, APIResourceRequest.X_PUSH_TOKEN); //signalR
            string? location = Utils.GetValueByKeyName(request, APIResourceRequest.X_DEVICE_LOCATION);
            string language = Utils.GetValueByKeyName(request, APIResourceRequest.X_APP_LANGUAGE);

            if (string.IsNullOrWhiteSpace(deviceUUID))
                throw new AppException(string.Format(CommonMessage.Message_Required, $"{APIResourceRequest.X_DEVICE_UDID}"));

            return new UserDeviceRequest
            {
                DeviceUUID = deviceUUID,
                AppName = appName,
                AppBuild = appBuild,
                AppVersion = appVersion,
                DeviceOS = deviceOS,
                DevicePlatform = devicePlatform,
                DeviceModel = deviceModel,
                DeviceName = deviceName,
                PushToken = pushToken,
                IsNotify = !string.IsNullOrWhiteSpace(pushToken),
                DeviceLocation = location,
            };
        }
        catch (MyException ex)
        {
            throw new MyException(SystemReturnCode.DataInputIsRequire, ex.Message);
        }
    }

    #region Private Methods 

    private void GetDataFromTokenCustom(IHttpContextAccessor httpContextAccessor)
    {
        userId = httpContextAccessor.HttpContext?.User.FindFirstValue("Id") ?? "";
        username = httpContextAccessor.HttpContext?.User.FindFirstValue("UserName") ?? "";
        currentCrewId = httpContextAccessor.HttpContext?.User.FindFirstValue("crewId") ?? "";
        currentUserId = Guid.TryParse(userId, out Guid valUserId) ? valUserId : Guid.Empty;
        var sDeviceId = httpContextAccessor.HttpContext?.User.FindFirstValue("deviceId") ?? "";
        currentDeviceId = int.TryParse(sDeviceId, out int valDeviceId) ? valDeviceId : 0;
        currentUserEmail = httpContextAccessor.HttpContext?.User.FindFirstValue("Email") ?? "";
        ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "";
    }

    private void GetDataFromTokenMicrosoft(IHttpContextAccessor httpContextAccessor)
    {
        userId = httpContextAccessor.HttpContext?.Items[ContextItemsKey.ID]?.ToString() ?? "";
        username = httpContextAccessor.HttpContext?.Items[ContextItemsKey.UserName]?.ToString() ?? "";
        currentUserId = Guid.TryParse(userId, out Guid valUserId) ? valUserId : Guid.Empty;
        currentCrewId = httpContextAccessor.HttpContext?.Items[ContextItemsKey.Code]?.ToString() ?? "";
        currentUserEmail = httpContextAccessor.HttpContext?.Items[ContextItemsKey.Email]?.ToString() ?? "";
        ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "";
    }
    
    #endregion
}