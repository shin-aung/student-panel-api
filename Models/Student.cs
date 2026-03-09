namespace StudentPortal.API.Models;
public class Student
{
public Guid Id { get; set; }
public string StudentId { get; set; } = string.Empty;
public string FirstName { get; set; } = string.Empty;
public string LastName { get; set; } = string.Empty;
public string Email { get; set; } = string.Empty;
public string PasswordHash { get; set; } = string.Empty;
public DateTime CreatedAt { get; set; }
}
public class LoginRequest
{
public string Email { get; set; } = string.Empty;
public string Password { get; set; } = string.Empty;
}
public class RegisterRequest
{
public string FirstName { get; set; } = string.Empty;
public string LastName { get; set; } = string.Empty;
public string Email { get; set; } = string.Empty;
public string StudentId { get; set; } = string.Empty;
public string Password { get; set; } = string.Empty;
}
public class AuthResponse
{
public string Token { get; set; } = string.Empty;
public StudentDto Student { get; set; } = new();
}
public class StudentDto
{
public Guid Id { get; set; }
public string StudentId { get; set; } = string.Empty;
public string FirstName { get; set; } = string.Empty;
public string LastName { get; set; } = string.Empty;
public string Email { get; set; } = string.Empty;
}