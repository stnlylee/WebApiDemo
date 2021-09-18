using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace WebApiDemo.Api.Security
{
    [ExcludeFromCodeCoverage]
    internal class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _config;
        private readonly Serilog.ILogger _serilogLogger;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> authSchemeOptions,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration config,
            Serilog.ILogger serilogLogger)
            : base(authSchemeOptions, logger, encoder, clock)
        {
            _config = config;
            _serilogLogger = serilogLogger;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing authorization header");
            }

            string username;
            string password;

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
                username = credentials[0];
                password = credentials[1];
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid authorization header");
            }

            if (!IsAuthorized(username, password))
            {
                return AuthenticateResult.Fail("Invalid username or password");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private bool IsAuthorized(string username, string password)
        {
            try
            {
                var section = _config.GetSection("BasicAuthentication");
                var user = section.GetValue(typeof(string), "Username").ToString();
                var pass = section.GetValue(typeof(string), "Passwords").ToString();

                return username.Equals(user, StringComparison.OrdinalIgnoreCase)
                       && password.Equals(pass);
            }
            catch (Exception ex)
            {
                _serilogLogger.Error(ex, "Something was wrong with basic authentication");
                return false;
            }
        }
    }
}
