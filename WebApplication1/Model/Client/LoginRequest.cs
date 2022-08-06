namespace WebApplication1.Model.Client
{
    public class LoginRequest
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public DateTime TokenExpireTime { get; set; }
        public string Name { get; set; }
    }
}
