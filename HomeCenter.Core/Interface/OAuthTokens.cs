namespace HomeCenter.Core.Interface;

public class OAuthTokens
{
    public required string AccessToken { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required string RefreshToken { get; init; }
}