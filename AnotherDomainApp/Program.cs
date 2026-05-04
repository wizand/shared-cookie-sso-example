using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using AnotherDomainApp.Components;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure Data Protection to share keys
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(SharedCommon.SsoConstants.SharedKeysPath))
    .SetApplicationName(SharedCommon.SsoConstants.ApplicationName);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = SharedCommon.SsoConstants.CookieName;
        options.Cookie.Domain = SharedCommon.SsoConstants.SharedDomain;
        options.Cookie.Path = "/";
        options.LoginPath = "/login";
        options.Events.OnRedirectToLogin = context =>
        {
            var originalUri = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
            context.Response.Redirect($"{SharedCommon.SsoConstants.IdentityProviderLoginUrl}?returnUrl={Uri.EscapeDataString(originalUri)}");
            return Task.CompletedTask;
        };
    });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
// app.UseHttpsRedirection(); // Disabled for HTTP PoC
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
