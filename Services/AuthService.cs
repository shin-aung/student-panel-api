using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using StudentPortal.API.Models;
namespace StudentPortal.API.Services;
public class AuthService
{
private readonly SupabaseService _supabase;
private readonly IConfiguration _config;
public AuthService(SupabaseService supabase, IConfiguration config)
{
    _supabase = supabase;
    _config = config;
}

public async Task<AuthResponse> Login(LoginRequest req)
{
    var student = await _supabase.GetStudentByEmail(req.Email)
        ?? throw new Exception("Invalid email or password.");

    if (!BCrypt.Net.BCrypt.Verify(req.Password, student.PasswordHash))
        throw new Exception("Invalid email or password.");

    return new AuthResponse
    {
        Token = GenerateToken(student),
        Student = ToDto(student)
    };
}

public async Task<AuthResponse> Register(RegisterRequest req)
{
    if (await _supabase.EmailExists(req.Email))
        throw new Exception("An account with this email already exists.");

    if (await _supabase.StudentIdExists(req.StudentId))
        throw new Exception("This Student ID is already registered.");

    var student = await _supabase.CreateStudent(new Student
    {
        StudentId = req.StudentId,
        FirstName = req.FirstName,
        LastName = req.LastName,
        Email = req.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
    });

    return new AuthResponse
    {
        Token = GenerateToken(student),
        Student = ToDto(student)
    };
}

private string GenerateToken(Student student)
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var expiry = int.Parse(_config["Jwt:ExpiryHours"] ?? "24");

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, student.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, student.Email),
        new Claim("studentId", student.StudentId),
        new Claim("firstName", student.FirstName),
        new Claim("lastName", student.LastName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(expiry),
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

private static StudentDto ToDto(Student s) => new()
{
    Id = s.Id,
    StudentId = s.StudentId,
    FirstName = s.FirstName,
    LastName = s.LastName,
    Email = s.Email
};
}