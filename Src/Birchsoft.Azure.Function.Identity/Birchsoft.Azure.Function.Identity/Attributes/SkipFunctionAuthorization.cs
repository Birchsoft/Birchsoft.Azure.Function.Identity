namespace Birchsoft.Azure.Function.Identity.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class SkipFunctionAuthorization : Attribute
    {
        public SkipFunctionAuthorization()
        {
        }
    }
}
