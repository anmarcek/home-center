namespace HomeCenter.Core.Interface;

public interface IOAuthClient
{
    Task<OAuthTokens> GetAccessTokenAsync(string clientId, string clientSecret, string tokenEndpoint, string redirectUri, string accessToken, string scope);
    Task<OAuthTokens> RefreshAccessTokenAsync(string clientId, string clientSecret, string tokenEndpoint, string refreshToken);
}
