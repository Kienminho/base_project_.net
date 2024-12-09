using Common.Authorization.Utils;
using Common.Constant;
using Common.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Common.Authorization
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;
        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtUtils jwtUtils)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            var accessToken = context.Request.Query["access_token"];
            // If the request is for our hub...
            var path = context.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/applicationHub")))
            {
                // Read the token out of the query string
                token = accessToken;
                context.Request.Headers["Authorization"] = "Bearer " + accessToken;
            }
            
            if(_appSettings.EnableMicrosoftCheckJwt)
                await HandleValidateTokenMicrosoft(context, jwtUtils, token ?? "");
            else
                HandleValidateTokenCustom(context, jwtUtils, token ?? "");

            await _next(context);
        }

        #region Private Methods

        private void HandleValidateTokenCustom(HttpContext context, IJwtUtils jwtUtils, string token)
        {
            var userId = jwtUtils.ValidateToken(token);
            if (userId != null)
            {
                // attach user to context on successful jwt validation
                //TODO: Assign User
                //context.Items["User"] = userService.GetById(userId ?? Guid.Empty);
                context.Items["User"] = userId;
                context.Items[ContextItemsKey.EnableMicrosoftCheckJwt] = true;
            }
        }


        private async Task HandleValidateTokenMicrosoft(HttpContext context, IJwtUtils jwtUtils, string token)
        {
            var account = await jwtUtils.ValidateTokenMicrosoft(token);
            if (account != null)
            {
                //add information to context: userId, userName, email
                context.Items[ContextItemsKey.ID] = account.Id;
                context.Items[ContextItemsKey.Code] = account.Code;
                context.Items[ContextItemsKey.FullName] = account.FullName;
                context.Items[ContextItemsKey.UserName] = account.UserName;
                context.Items[ContextItemsKey.Email] = account.Email;
                context.Items[ContextItemsKey.EnableMicrosoftCheckJwt] = true;
            }
            else
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }

        #endregion
    }
}
