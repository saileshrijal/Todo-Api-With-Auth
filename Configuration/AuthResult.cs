namespace Configuration
{
    public class AuthResult
    {
        public string? Token { get; set; }
        public bool Success { get; set; }
        public List<String> Errors { get; set; }
    }
}