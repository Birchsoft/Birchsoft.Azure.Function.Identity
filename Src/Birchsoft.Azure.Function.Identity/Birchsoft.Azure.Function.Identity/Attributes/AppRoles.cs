namespace Birchsoft.Azure.Function.Identity.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class AppRoles : Attribute
    {
        public string[] Roles { get; } = [];
        private AppRoles() { }

        public AppRoles(params string[] roles)
        {
            Roles = roles;
        }
    }
}
