using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Lamour.Application.Features.Auth.Dtos;
using Lamour.Application.Features.Employees.Repositories;
using Lamour.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Lamour.Application.Features.Auth.UseCases;

public class LoginUseCase : ILoginUseCase
{
    private readonly IEmployeeRepository       _repo;
    private readonly IConfiguration            _config;
    private readonly ILogger<LoginUseCase>     _logger;

    public LoginUseCase(
        IEmployeeRepository   repo,
        IConfiguration        config,
        ILogger<LoginUseCase> logger)
    {
        _repo   = repo;
        _config = config;
        _logger = logger;
    }

    public async Task<LoginResponseDto> ExecuteAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var employee = await _repo.GetByPhoneAsync(request.Phone, ct);

        if (employee is null)
            throw new DomainException("Số điện thoại hoặc mật khẩu không đúng.");

        if (!employee.IsActive)
            throw new DomainException("Tài khoản đã bị vô hiệu hóa.");

        if (HashPassword(request.Password) != employee.PasswordHash)
            throw new DomainException("Số điện thoại hoặc mật khẩu không đúng.");

        var jwtKey = _config["Jwt:Key"] ?? "supersecretkey_changeme_32chars!!";
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,  employee.Id.ToString()),
            new Claim("phone",                       employee.Phone),
            new Claim("name",                        employee.Name),
            new Claim(ClaimTypes.Role,               employee.Role.ToString()),
        };

        var token = new JwtSecurityToken(
            claims:   claims,
            expires:  DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation("Employee {Id} ({Phone}) logged in.", employee.Id, employee.Phone);

        return new LoginResponseDto
        {
            UserId      = employee.Id,
            Phone       = employee.Phone,
            Name        = employee.Name,
            Role        = employee.Role.ToString(),
            AccessToken = tokenString,
        };
    }

    private static string HashPassword(string password)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
}
