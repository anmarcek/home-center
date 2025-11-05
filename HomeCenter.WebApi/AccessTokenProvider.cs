using HomeCenter.Core.Interface;

namespace HomeCenter.WebApi;

public class AccessTokenProvider : IAccessTokenProvider
{
    private string? _accessToken;
    
    public string AccessToken
    {
        get => _accessToken ?? throw new Exception("Access token has not been set.");
        set => _accessToken = value;
    }
}