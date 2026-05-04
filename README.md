# Shared Cookie SSO Example

This repository contains a proof-of-concept for implementing Single Sign-On (SSO) across multiple ASP.NET Core Blazor applications using shared authentication cookies and ASP.NET Core Data Protection.

This pattern is useful when you have multiple applications running on subdomains of the same root domain and you want users to log in once and be authenticated across all applications without relying on a heavier identity provider like OpenID Connect or OAuth2.

## Applications in this Solution

The solution consists of four Blazor Web Apps:

1. **AnkkaAuth**: The Identity Provider (IdP). This application handles user authentication and sets the shared authentication cookie. It contains the central login page.
2. **PulttiBoys**: A client application. It relies on the shared cookie for authentication. If an unauthenticated user attempts to access a protected resource, they are redirected to `AnkkaAuth` to log in.
3. **PekoniBoys**: Another client application acting identically to `PulttiBoys` to demonstrate that multiple apps can share the same login session.
4. **AnotherDomainApp**: A client application hosted on a different domain (`anotherdomainapp.eridomain.com`). It serves to demonstrate that the shared cookie won't work across different base domains, as the browser will not send the `.yhteinendomain.com` cookie to `.eridomain.com`.

## How It Works

The shared cookie SSO pattern relies on the following configurations being identical across all participating applications:

1. **Shared Cookie Domain**: All applications are configured to write and read the cookie on a shared top-level domain (`SharedCommon.SsoConstants.SharedDomain`).
   ```csharp
   options.Cookie.Name = SharedCommon.SsoConstants.CookieName;
   options.Cookie.Domain = SharedCommon.SsoConstants.SharedDomain;
   ```
2. **Data Protection**: ASP.NET Core encrypts authentication cookies. For multiple apps to read the same cookie, they must share the same encryption keys and the same application name (`SharedCommon.SsoConstants.ApplicationName`).
   ```csharp
   builder.Services.AddDataProtection()
       .PersistKeysToFileSystem(new DirectoryInfo(SharedCommon.SsoConstants.SharedKeysPath))
       .SetApplicationName(SharedCommon.SsoConstants.ApplicationName);
   ```
3. **Redirection Logic**: The client apps (`PulttiBoys`, `PekoniBoys`) override the default login redirection to point to the authentication app (`AnkkaAuth`), appending a `returnUrl` so the user is sent back after a successful login. This utilizes `SharedCommon.SsoConstants.IdentityProviderLoginUrl`.
   ```csharp
   options.Events.OnRedirectToLogin = context =>
   {
       var originalUri = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
       context.Response.Redirect($"{SharedCommon.SsoConstants.IdentityProviderLoginUrl}?returnUrl={Uri.EscapeDataString(originalUri)}");
       return Task.CompletedTask;
   };
   ```

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Configure your `hosts` file to simulate the shared domain locally. Add the following entries to your OS's hosts file (e.g., `C:\Windows\System32\drivers\etc\hosts` on Windows):

```text
127.0.0.1 auth.yhteinendomain.com
127.0.0.1 pulttiboys.yhteinendomain.com
127.0.0.1 pekoniboys.yhteinendomain.com
127.0.0.1 anotherdomainapp.eridomain.com
```

### Configuration

You need to make sure the Data Protection `PersistKeysToFileSystem` path in `SharedCommon\SsoConstants.cs` points to a valid, shared directory on your machine. Currently, the path is set to `f:\programming\github\shared-cookie-sso-example\SharedKeys`. You should verify this matches the path on your system.

### Centralized Constants

The solution now contains a class library project `SharedCommon` with a class `SsoConstants` that stores all string constants like the URLs, cookie names, and hardcoded credentials (`SsoConstants.TestUsername` and `SsoConstants.TestPassword`). All three main apps reference this project to prevent drifting configurations.

### Running the Apps

1. Run all three applications. You can use Visual Studio's "Multiple Startup Projects" feature or run them from the CLI.
2. Ensure they are bound to different ports and you access them via the custom domains.
3. Try accessing a protected page on `app1.yhteinendomain.com`. You will be redirected to `auth.yhteinendomain.com`.
4. Log in with the hardcoded credentials from `SsoConstants` (Username: `aaa`, Password: `bbb`).
5. You will be redirected back to the client app and logged in.
6. Open the other client app, and you will notice you are already authenticated.

## Note
This is a simple proof-of-concept and does not implement HTTPS, database-backed users, or complex claims routing. 
