namespace Model.ResponseModel.Common
{
    public class InfoGoogleResponse
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? GivenName { get; set; }
        public string? FamilyName { get; set; }
        public string? Picture { get; set; }
        public string? Email { get; set; }
        public bool VerifiedEmail { get; set; }
        public string? Locale { get; set; }
    }
}
