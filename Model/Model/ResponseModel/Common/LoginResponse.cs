namespace Model.ResponseModel.Common
{
    public class LoginResponse
    {
        private string? _token;
        private string? _Refreshtoken;
        public Guid UserId { get; set; }
        public string? AccessToken { get { return _token; } }
        public string? RefreshToken { get { return _Refreshtoken; } }

        public void SetToken(string token)
        {
            _token = token;
        }

        public void SetRefreshToken(string RefreshToken)
        {
            _Refreshtoken = RefreshToken;
        }
    }
}
