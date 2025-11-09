using HomeCenter.Core;
using HomeCenter.Core.Interface;
using HomeCenter.Core.Netatmo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HomeCenter.WebApi.Controllers;

[ApiController]
[Route("netatmo-oauth")]
public class NetatmoOAuthController (
    IOAuthClient oAuthClient,
    IOptions<NetatmoOptions> netatmoOptions
)
    : ControllerBase
{
    private readonly NetatmoOptions _netatmoOptions = netatmoOptions.Value;

    [HttpGet("get-oauth-tokens", Name = "GetOAuthTokens")]
    public async Task<ActionResult<OAuthTokens>> GetOAuthTokens(string authorizationCode, string redirectUri)
    {
        try
        {
            var tokens = await oAuthClient.GetAccessTokenAsync
            (
                _netatmoOptions.ClientId,
                _netatmoOptions.ClientSecret,
                _netatmoOptions.TokenEndpoint,
                redirectUri,
                authorizationCode,
                _netatmoOptions.TokenScope
            );
        
            return tokens;
        }
        catch (OAuthClientUnauthorizedException)
        {
            return BadRequest("Invalid authorization code.");
        }
    }

    [HttpGet("refresh-oauth-tokens", Name = "RefreshOAuthTokens")]
    public async Task<ActionResult<OAuthTokens>> RefreshOAuthTokens(string refreshToken)
    {
        try
        {
            var tokens = await oAuthClient.RefreshAccessTokenAsync
            (
                _netatmoOptions.ClientId,
                _netatmoOptions.ClientSecret,
                _netatmoOptions.TokenEndpoint,
                refreshToken
            );
        
            return tokens;
        }
        catch (OAuthClientUnauthorizedException)
        {
            return BadRequest("Invalid refresh token.");
        }
    }
}