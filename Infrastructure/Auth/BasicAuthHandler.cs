using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace CmsFetchService.Infrastructure.Auth;

public class BasicAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, 
    ILoggerFactory logger, 
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{

    private static readonly List<(string Username, string Password, string[] Roles)> _users = new()
    {
        new("cms", "cmspass", ["CmsClient"]),
        new("admin", "adminpass", ["Admin", "User"]),
        new("user", "userpass", ["User"])
    };

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out StringValues value))
        {
            return AuthenticateResult.Fail("Missing Authorization Header");
        }

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(value!);
            if (!"Basic".Equals(authHeader.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Fail("Invalid auth scheme");
            }
            if (string.IsNullOrEmpty(authHeader.Parameter))
            {
                return AuthenticateResult.Fail("Missing credentials");
            }

            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
            if (credentials.Length != 2)
            {
                return AuthenticateResult.Fail("Invalid auth format");
            }

            var username = credentials[0];
            var password = credentials[1];
            var userData = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);

            if (string.IsNullOrWhiteSpace(userData.Username))
            {
                return AuthenticateResult.Fail("Invalid credentials");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, userData.Username)
            };
            claims.AddRange(userData.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);

        }
        catch (FormatException)
        {
            return AuthenticateResult.Fail("Invalid encoding");
        }
        catch (Exception ex)
        {
            return AuthenticateResult.Fail($"Authentication error: {ex.Message}");
        }
    }
}