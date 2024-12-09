using Common.Authorization.Utils;
using Common.Utils;
using log4net;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace Common.Authorization;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILog log = LogManager.GetLogger(typeof(ErrorHandlerMiddleware));

    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Intercept the response
        var originalBodyStream = context.Response.Body;
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            if (context.Response.StatusCode != (int)HttpStatusCode.OK && context.Response.StatusCode != (int)HttpStatusCode.BadRequest)
            {
                var response = context.Response;
                // Write log
                var httpStatusCode = (HttpStatusCode)response.StatusCode;
                var id = context.User == null ? null : context.User.FindFirst("Id");
                var userId = id != null ? id.Value : null;
                var apiRequested = context.Request.Path.Value ?? "";
                var paramsOrBody = context.Request.Method == "GET" ? context.Request.QueryString.Value : context.Request.Body.ToString();
                Common.Utils.Utils.WriteLogRequest(log, "ERROR", userId, paramsOrBody ?? "", context.Request);
                // Response
                var dataRes = new BaseResponse<object>();
                dataRes.StatusCode = response.StatusCode;
                dataRes.Message = httpStatusCode.ToString();

                response.ContentType = "application/json";

                // Serialize the dataRes object to the response stream
                response.Body.SetLength(0);
                await response.WriteAsync(JsonSerializer.Serialize(dataRes));
            }
        }
        catch (Exception error)
        {
            var response = context.Response;
            var paramsOrBody = context.Request.Method == "GET" ? context.Request.QueryString.Value : context.Request.Body.ToString();
            // Write log
            var id = context.User == null ? null : context.User.FindFirst("Id");
            var userId = id != null ? id.Value : null;
            var apiRequested = context.Request.Path.Value ?? "";
            Common.Utils.Utils.WriteLogRequest(log, "ERROR", userId, paramsOrBody ?? "", context.Request, error.Message);

            var dataRes = new BaseResponse<object>
            {
                Message = error?.Message
            };

            switch (error)
            {
                case AppException e:
                    // Custom application error
                    dataRes.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case KeyNotFoundException e:
                    // Not found error
                    dataRes.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case KeyExistsException e:
                    // Key exists error
                    dataRes.StatusCode = (int)HttpStatusCode.Conflict;
                    break;
                default:
                    // Unhandled error
                    dataRes.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            response.StatusCode = dataRes.StatusCode;
            response.ContentType = "application/json";
            await response.WriteAsync(JsonSerializer.Serialize(dataRes));
        }
        finally
        {
            // Copy the modified response to the original response body stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
    }
}