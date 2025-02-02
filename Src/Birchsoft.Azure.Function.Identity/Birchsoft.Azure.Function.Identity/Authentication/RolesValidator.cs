using Microsoft.IdentityModel.Tokens;

namespace Birchsoft.Azure.Function.Identity.Authentication
{
    internal class RolesValidator
    {
        internal bool Validate(IdentitySettings dto)
        {
            try
            {
                if (dto.JWTRoles.Length > 0 && dto.AttributeRoles.Length == 0)
                {
                    throw new SecurityTokenValidationException($"Required roles validation.");
                }

                if (dto.JWTRoles.Length == 0 && dto.AttributeRoles.Length > 0)
                {
                    throw new SecurityTokenValidationException($"JWT does not contain any roles to validate.");
                }

                bool hasCommonRoles = dto.AttributeRoles.Any(dto.JWTRoles.Contains);
                return hasCommonRoles;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
