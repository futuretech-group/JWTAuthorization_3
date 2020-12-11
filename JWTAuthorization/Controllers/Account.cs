using JWTAuthorization.Application.Common.Interfaces;
using JWTAuthorization.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JWTAuthorization.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Account : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDateTime _dateTime;
        private readonly IConfiguration _configuration;
        public Account(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IDateTime dateTime, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dateTime = dateTime;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<dynamic> Login(string username, string password)
        {
            var appUser = await _userManager.FindByEmailAsync(username);
            if (appUser != null)
            {
                await _signInManager.SignOutAsync();
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(appUser, password, false, false);
                if (result.Succeeded)
                {
                    return new
                    {
                        Id = appUser.Id,
                        Email = appUser.Email,
                        EmailConfirmed = appUser.EmailConfirmed,
                        FullName = appUser.FullName,
                        token = GenerateJWT(appUser)
                    };
                }
            }
            return null;
        }

        private string GenerateJWT(ApplicationUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim("FullName", user.FullName.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Jwt:AccessTokenLifeTimeMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
