using System.Text.Json;
using System.Text.Json.Serialization;
using HomeCenter.Core.Interface;

namespace HomeCenter.Core;

public class OAuthClient : IOAuthClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OAuthClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<OAuthTokens> GetAccessTokenAsync(string clientId, string clientSecret, string tokenEndpoint, string redirectUri, string authorizationCode, string scope)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("code", authorizationCode),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("scope", scope)
            ])
        };

        var oAuthTokens = await GetOAuthTokensAsync(request).ConfigureAwait(false);

        return oAuthTokens;
    }

    public async Task<OAuthTokens> RefreshAccessTokenAsync(string clientId, string clientSecret, string tokenEndpoint, string refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            ])
        };

        var oAuthTokens = await GetOAuthTokensAsync(request).ConfigureAwait(false);

        return oAuthTokens;
    }

    #region Helpers

    private async Task<OAuthTokens> GetOAuthTokensAsync(HttpRequestMessage request)
    {
        var _httpClient = _httpClientFactory.CreateClient();

        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var getAccessTokenResponse = JsonSerializer.Deserialize<GetAccessTokenResponse>(json) ?? throw new InvalidOperationException("Failed to deserialize access token.");

        var oAuthTokens = new OAuthTokens
        {
            AccessToken = getAccessTokenResponse.AccessToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(getAccessTokenResponse.ExpiresIn),
            RefreshToken = getAccessTokenResponse.RefreshToken
        };

        return oAuthTokens;
    }

    #endregion

    #region Types

    public class GetAccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; init; }

        [JsonPropertyName("expires_in")]
        public required int ExpiresIn { get; init; }

        [JsonPropertyName("refresh_token")]
        public required string RefreshToken { get; init; }
    }

    #endregion
}