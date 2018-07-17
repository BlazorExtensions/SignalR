using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Blazor.Extensions.SignalR.Test.Server.Controllers
{
    [Route("generatetoken")]
    public class TokenController : Controller
    {
        [HttpGet]
        public string GenerateToken()
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, this.Request.Query["user"]) };
            var credentials = new SigningCredentials(Startup.SecurityKey, SecurityAlgorithms.HmacSha256); // Too lazy to inject the key as a service
            var token = new JwtSecurityToken("SignalRTestServer", "SignalRTests", claims, expires: DateTime.UtcNow.AddSeconds(30), signingCredentials: credentials);
            return Startup.JwtTokenHandler.WriteToken(token); // Even more lazy here
        }
    }
}
