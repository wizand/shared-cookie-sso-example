namespace SharedCommon;

public static class SsoConstants
{
    public const string CookieName = ".AspNet.SharedCookie";
    public const string SharedDomain = ".yhteinendomain.com"; //TODO: this does not change the launchSettings.json, so you need to set the domain in the browser dev tools to test the SSO functionality
    public const string IdentityProviderLoginUrl = $"http://auth.{SharedDomain}:5001/login";
    public const string ApplicationName = "SharedCookieApp";
    public const string SharedKeysPath = @"f:\programming\github\shared-cookie-sso-example\SharedKeys";
    
    // Hardcoded credentials for PoC
    public const string TestUsername = "aaa";
    public const string TestPassword = "bbb";
    public const string TestRole = "Admin";
}
