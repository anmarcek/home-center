using HomeCenter.Core.Interface;
using HomeCenter.Core.Netatmo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HomeCenter.WebApi.Controllers;

[ApiController]
[Route("netatmo-oauth")]
public class NetatmoOAuthController(
    IOAuthClient oAuthClient,
    IOptions<NetatmoOptions> netatmoOptions
)
    : ControllerBase
{
    private readonly NetatmoOptions _netatmoOptions = netatmoOptions.Value;

    [HttpGet("get-oauth-tokens", Name = "GetOAuthTokens")]
    public async Task<OAuthTokens> GetOAuthTokens(string authorizationCode, string redirectUri)
    {
        var tokens = await oAuthClient.GetAccessTokenAsync(_netatmoOptions.ClientId, _netatmoOptions.ClientSecret,
            _netatmoOptions.TokenEndpoint, redirectUri, authorizationCode, _netatmoOptions.TokenScope);
        return tokens;
    }

    [HttpGet("refresh-oauth-tokens", Name = "RefreshOAuthTokens")]
    public async Task<OAuthTokens> RefreshOAuthTokens(string refreshToken)
    {
        var tokens = await oAuthClient.RefreshAccessTokenAsync(_netatmoOptions.ClientId, _netatmoOptions.ClientSecret,
            _netatmoOptions.TokenEndpoint, refreshToken);
        return tokens;
    }
}