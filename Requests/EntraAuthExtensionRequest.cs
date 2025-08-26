namespace MapelRestAPI.Requests
{
    public class EntraAuthExtensionRequest
    {
        //public AuthContext AuthenticationContext { get; set; }
        //public EntraUserData Data { get; set; }
        public string email { get; set; }
    }

    public class AuthContext
    {
        public string ClientId { get; set; }
        public string CorrelationId { get; set; }
        public string TenantId { get; set; }
    }

    public class EntraUserData
    {
        public string Email { get; set; }
        public string UserPrincipalName { get; set; }
    }
}