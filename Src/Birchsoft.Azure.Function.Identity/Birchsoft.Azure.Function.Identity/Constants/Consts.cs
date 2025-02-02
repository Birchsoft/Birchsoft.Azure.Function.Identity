namespace Birchsoft.Azure.Function.Identity.Constants
{
    internal static class Consts
    {
        internal const string DetailedErrorMessage = "DetailedErrorMessage";
        internal const string IdentitySettings = "IdentitySettings";
        internal const string OIDCEndpointV1_0 = "https://login.microsoftonline.com/{Tenant-Id}/.well-known/openid-configuration";
        internal const string OIDCEndpointV2_0 = "https://login.microsoftonline.com/{Tenant-Id}/v2.0/.well-known/openid-configuration";
        internal const string IssuerV1_0 = "https://sts.windows.net/{Tenant-Id}/";
        internal const string IssuerV2_0 = "https://login.microsoftonline.com/{Tenant-Id}/v2.0";
    }

    internal static class JwtClaim
    {
        internal const string Version = "ver";
        internal const string Roles = "roles";
        internal const string Subject = "sub";
        internal const string TenantID = "tid";
    }
}
