using Birchsoft.Azure.Function.Identity.Constants;
using Birchsoft.Azure.Function.Identity.Utils;
using Microsoft.Azure.Functions.Worker;
using System.IdentityModel.Tokens.Jwt;

namespace Birchsoft.Azure.Function.Identity.Authentication
{
    internal class IdentitySettings
    {
        internal string JWToken { get; private set; } = string.Empty;
        internal string[] JWTRoles { get; private set; } = [];
        internal bool SkipFunctionAuthorization { get; private set; } = false;
        internal string[] AttributeRoles { get; private set; } = [];

        internal IdentitySettings Initialize(FunctionContext context)
        {
            SkipFunctionAuthorization = Helper.SkipFunctionAuthorization(context);

            if (Helper.SkipRoleAuthorization(context, out string[] roles) is bool skipRoleAuthorization)
            {
                AttributeRoles = roles;
            }

            return this;
        }

        internal IdentitySettings SetJWTokenRoles(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtTokenObj = handler.ReadJwtToken(token);
            var claims = jwtTokenObj.Claims;
            JWTRoles = claims.Where(c => c.Type == JwtClaim.Roles).Select(c => c.Value).ToArray();

            return this;
        }

        internal IdentitySettings SetJWToken(string? token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                JWToken = token;
            }

            return this;
        }
    }
}
