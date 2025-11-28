namespace EXAT.ECM.EService.API.Model.Requests
{
    public class AccessTokenRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class LoginTokenRequest
    {
        public string? LoginToken { get; set; }
    }
}
