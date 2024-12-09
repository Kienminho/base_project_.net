using Common.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Model.ResponseModel.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common.Constant;
using Common.UnitOfWork.UnitOfWorkPattern;
using Entity.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Common.Authorization.Utils
{
    public class JwtUtils(IOptions<StrJWT> strJwt,
        IOptions<MicrosoftSettings> microsoftSettings,
        IUnitOfWork _unitOfWork,
        IMemoryCache _memoryCache) : IJwtUtils
    {
        private readonly StrJWT _strJWT = strJwt.Value;
        private readonly MicrosoftSettings _microsoftSettings = microsoftSettings.Value;
        private const string DOMAIN_MAIL = "@vietnamairlines.com";
        private const string TENANT_KEY = "tid"; // tenant id
        private const string UNIQUE_NAME = "unique_name"; // email
        private const string PREFERRER_USERNAME = "preferred_username";
        private const string EXP_KEY = "exp"; // expire time

        public string GenerateToken(Guid userId, string? fullName, string UDID, string userName)
        {
            string? skey = _strJWT.Key;
            string? issuer = _strJWT.Issuer;
            string? audience = _strJWT.Audience;
            // generate token that is valid for 7 days
            var key = Encoding.ASCII.GetBytes(skey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, userName),
                    new Claim("Id", userId.ToString()),
                    new Claim("UDID", UDID),
                    new Claim(JwtRegisteredClaimNames.Sub, userName),
                    new Claim(JwtRegisteredClaimNames.Name, fullName ?? ""),
                    new Claim(JwtRegisteredClaimNames.Email, userName + DOMAIN_MAIL),
                    new Claim(JwtRegisteredClaimNames.Jti,
                        Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(14),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials
                (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public Guid? ValidateToken(string token)
        {
            if (token == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_strJWT.Key);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                Guid userId = Guid.Empty;
                Guid.TryParse(jwtToken.Claims.First(x => x.Type == "Id").Value, out userId);

                if (userId == Guid.Empty) return null;
                // return user id from JWT token if validation successful
                return userId;
            }
            catch (Exception ex)
            {
                // return null if validation fails
                return null;
            }
        }

        public RfTokenResponse GenerateRefreshToken(Guid userId, string? fullName, string userName, string UDID, string skey, string Issuer, string Audience, string ipAddress)
        {
            var key = Encoding.ASCII.GetBytes(skey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, userName),
                    new Claim("Id", userId.ToString()),
                    new Claim("UDID", UDID),
                    new Claim(JwtRegisteredClaimNames.Sub, userName),
                    new Claim(JwtRegisteredClaimNames.Name, fullName ??""),
                    new Claim(JwtRegisteredClaimNames.Email, userName + "@vietnamairlines.com"),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(15),
                Issuer = Issuer,
                Audience = Audience,
                SigningCredentials = new SigningCredentials
                (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            return new RfTokenResponse
            {
                Token = jwtToken,
                Expires = DateTime.UtcNow.AddDays(15),
                CreateTime = DateTime.Now,
                CreatedByIp = ipAddress
            };
        }
        
        public async Task<SysAccount?> ValidateTokenMicrosoft(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            //decode token get: tid, unique_name, exp
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            var tid = jsonToken?.Claims.First(claim => claim.Type == TENANT_KEY).Value;
            var uniqueName = jsonToken?.Claims
                .First(claim => claim.Type == UNIQUE_NAME || claim.Type == PREFERRER_USERNAME).Value;
            var exp = jsonToken?.Claims.First(claim => claim.Type == EXP_KEY).Value;

            if (string.IsNullOrWhiteSpace(tid) || string.IsNullOrWhiteSpace(uniqueName) ||
                string.IsNullOrWhiteSpace(exp))
                return null;

            //check exp. if exp < now return null
            var expDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp));
            if (expDate < DateTimeOffset.Now)
                return null;

            //check tid in microsoft settings
            if (tid != _microsoftSettings.TenantId)
                return null;

            //get user from cache
            var accounts = await GetAccounts();
            return accounts?.FirstOrDefault(a => a.Email == uniqueName);

        }
        
        #region Private Method

        private async Task<List<SysAccount>?> GetAccounts()
        {
            if (_memoryCache.TryGetValue(CacheKey.UsersCache, out List<SysAccount>? cacheEntry))
                return cacheEntry;

            var accounts = await _unitOfWork.Repository<SysAccount>()
                .Where(a => a.IsDeleted != true)
                .ToListAsync();
            _memoryCache.Set(CacheKey.UsersCache, accounts, TimeSpan.FromDays(1));

            return accounts;
        }
        
        #endregion
    }
}
