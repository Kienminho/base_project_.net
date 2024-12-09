using log4net;
using Microsoft.AspNetCore.Http;

namespace Common.Authorization;

public class ReadBodyFromRequestMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;
    private readonly ILog log = LogManager.GetLogger(typeof(ReadBodyFromRequestMiddleware));
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            context.Items["RequestBody"] = body;
            context.Request.Body.Position = 0;
            await _next(context);
        }
        catch (Exception ex)
        {
            throw new Exception("Error reading request body", ex);
        }
    }
}
