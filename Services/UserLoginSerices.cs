using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AdminPortal.Data;
using AdminPortal.Models;
using AdminPortal.Models.Entities;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AdminPortal.Services
{
    public class UserLoginServices
    {
        public ApplicationDBContext dBContext;
        public IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserLoginServices(
            ApplicationDBContext dBContext,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor
        )
        {
            this.dBContext = dBContext;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        public bool ValidateUser(LoginDto loginRequest)
        {
            var data = dBContext.Employees.FirstOrDefault(e => e.EmailId == loginRequest.EmailId);
            Console.WriteLine(data);

            if (data == null)
            {
                return false;
            }
            else
            {
                if (loginRequest.EmailId == data.EmailId && loginRequest.Password == data.Password)
                {
                    return true;
                }
                return false;
            }
        }

        public string GenerateJwtToken(
            string Email,
            Role role,
            string name,
            Decimal Salary,
            string PhoneNo,
            Guid id
        )
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, Email),
                new Claim("ID", id.ToString()),
                new Claim("Role", role.ToString()),
                new Claim("Name", name.ToString()),
                new Claim("Salary", Salary.ToString()),
                new Claim("PhoneNo", PhoneNo.ToString()),
                new Claim("Email", Email.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(120),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GetClaimValue(string claimType)
        {
            var identity = _httpContextAccessor.HttpContext?.User.Identity as ClaimsIdentity;
            return identity?.FindFirst(claimType)?.Value ?? string.Empty;
        }
    }
}
