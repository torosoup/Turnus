using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace TurnusTests
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Read headers: Test-User-Id, Test-User-Email, Test-User-Roles (comma separated), Test-Workspace
            var headers = Request.Headers;
            if (!headers.ContainsKey("Test-User-Id"))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var userId = headers["Test-User-Id"].ToString();
            var email = headers.ContainsKey("Test-User-Email") ? headers["Test-User-Email"].ToString() : "test@local";
            var roles = headers.ContainsKey("Test-User-Roles") ? headers["Test-User-Roles"].ToString().Split(',') : new string[0];
            var workspace = headers.ContainsKey("Test-Workspace") ? headers["Test-Workspace"].ToString() : null;

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Email, email)
            };

            foreach (var r in roles)
            {
                if (!string.IsNullOrWhiteSpace(r)) claims.Add(new Claim(ClaimTypes.Role, r.Trim()));
            }

            if (!string.IsNullOrEmpty(workspace)) claims.Add(new Claim("workspace", workspace));

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
