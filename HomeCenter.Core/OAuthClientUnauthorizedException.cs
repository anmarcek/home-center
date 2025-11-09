namespace HomeCenter.Core;

public class OAuthClientUnauthorizedException(string? message, Exception? innerException)
    : Exception(message, innerException);