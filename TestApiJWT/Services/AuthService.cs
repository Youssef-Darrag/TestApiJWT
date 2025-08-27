﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TestApiJWT.DTOs;
using TestApiJWT.Helpers;
using TestApiJWT.Models;

namespace TestApiJWT.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JWT _jwt;

        public AuthService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt)
        {
            _userManager = userManager;
            _jwt = jwt.Value;
        }

        public async Task<AuthDto> RegisterAsync(RegisterDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) is not null)
                return new AuthDto { Message = "Email is already registered!" };

            if (await _userManager.FindByNameAsync(dto.UserName) is not null)
                return new AuthDto { Message = "UserName is already registered!" };

            var user = new ApplicationUser()
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(",\n", result.Errors.Select(e => e.Description));

                return new AuthDto { Message = errors };
            }

            await _userManager.AddToRoleAsync(user, "User");

            var jwtSecurityToken = await CreateJwtToken(user);

            return new AuthDto
            {
                Email = user.Email,
                ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                UserName = user.UserName
            };
        }

        public async Task<AuthDto> GetTokenAsync(TokenRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return new AuthDto { Message = "Email or Password is incorrect!" };

            var jwtSecurityToken = await CreateJwtToken(user);
            var rolesList = await _userManager.GetRolesAsync(user);

            return new AuthDto
            {
                IsAuthenticated = true,
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Email = user.Email!,
                UserName = user.UserName!,
                ExpiresOn = jwtSecurityToken.ValidTo,
                Roles = rolesList.ToList()
            };
        }

        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim("uid", user.Id)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }
    }
}
