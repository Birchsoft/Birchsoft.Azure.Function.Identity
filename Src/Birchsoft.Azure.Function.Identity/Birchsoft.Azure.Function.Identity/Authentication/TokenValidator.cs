using Birchsoft.Azure.Function.Identity.Constants;
using Birchsoft.Azure.Function.Identity.Models;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Birchsoft.Azure.Function.Identity.Authentication
{
    internal class TokenValidator
    {
        internal async Task<ClaimsPrincipal> ValidateToken(string token, AzureMEID data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new SecurityTokenException("Token is missing.");
                }

                var configurationManager = GetOpenIdConnectConfiguration(token, data.TenantId);
                var openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = GetIssuer(token, data.TenantId),

                    ValidateAudience = true,
                    ValidAudience = data.Audience,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),

                    IssuerValidator = (issuer, jwToken, parameters) =>
                    {
                        if (jwToken is JwtSecurityToken jwt)
                        {
                            var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                            var tidClaim = jwt.Claims.FirstOrDefault(c => c.Type == "tid")?.Value;

                            if (subClaim != data.ObjectId)
                            {
                                throw new SecurityTokenValidationException($"Invalid subject.");
                            }

                            if (tidClaim != data.TenantId)
                            {
                                throw new SecurityTokenValidationException($"Invalid tenant.");
                            }
                        }
                        else
                        {
                            throw new SecurityTokenValidationException($"Invalid token.");
                        }

                        return issuer;
                    },

                    RequireSignedTokens = true,
                    IssuerSigningKeys = openIdConfig.SigningKeys
                };

                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                if (validatedToken != null && validatedToken is JwtSecurityToken jwt &&
                    jwt.Header.Alg.Equals(SecurityAlgorithms.None, StringComparison.OrdinalIgnoreCase))
                {
                    throw new SecurityTokenInvalidAlgorithmException("Invalid security algorithm.");
                }

                return principal;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private ConfigurationManager<OpenIdConnectConfiguration> GetOpenIdConnectConfiguration(string token, string tenantId)
        {
            var oidcEndpoint = Consts.OIDCEndpointV1_0.Replace("{Tenant-Id}", tenantId);
            var handler = new JwtSecurityTokenHandler();
            var jwtTokenObj = handler.ReadJwtToken(token);
            var claims = jwtTokenObj.Claims;
            var jwtVersion = claims.FirstOrDefault(c => c.Type == "ver")?.Value;

            if (!string.IsNullOrEmpty(jwtVersion))
            {
                if (jwtVersion == "2.0")
                {
                    oidcEndpoint = Consts.OIDCEndpointV2_0.Replace("{Tenant-Id}", tenantId);
                }
            }

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                oidcEndpoint, new OpenIdConnectConfigurationRetriever(), new HttpDocumentRetriever());

            return configurationManager;
        }

        private string GetIssuer(string token, string tenantId)
        {
            var issuer = Consts.IssuerV1_0.Replace("{Tenant-Id}", tenantId);
            var handler = new JwtSecurityTokenHandler();
            var jwtTokenObj = handler.ReadJwtToken(token);
            var claims = jwtTokenObj.Claims;
            var jwtVersion = claims.FirstOrDefault(c => c.Type == "ver")?.Value;

            if (!string.IsNullOrEmpty(jwtVersion))
            {
                if (jwtVersion == "2.0")
                {
                    issuer = Consts.IssuerV2_0.Replace("{Tenant-Id}", tenantId);
                }
            }

            return issuer;
        }
    }
}
