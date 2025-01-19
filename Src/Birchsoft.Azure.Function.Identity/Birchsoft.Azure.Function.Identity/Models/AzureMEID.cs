namespace Birchsoft.Azure.Function.Identity.Models
{
    public class AzureMEID
    {
        public required string TenantId { get; set; }
        public required string Audience { get; set; }
        public required string ObjectId { get; set; }
    }
}
