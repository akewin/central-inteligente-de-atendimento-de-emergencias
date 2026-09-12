namespace Api.Dtos
{
    public class RefreshTokenResponseDto
    {
        public string Token { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
    }
}
