using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using StudentPortal.API.Models;

namespace StudentPortal.API.Services;

public class SupabaseService
{
    private readonly HttpClient _http;
    private readonly string _url;
    private readonly string _key;

    // ── Private DTO for Supabase snake_case mapping ──────────────────────────
    private class SupabaseStudent
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("student_id")]
        public string StudentId { get; set; } = string.Empty;

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    // ── Constructor ──────────────────────────────────────────────────────────
    public SupabaseService(IConfiguration config)
    {
        _url = config["Supabase:Url"]!;
        _key = config["Supabase:AnonKey"]!;
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("apikey", _key);
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _key);
    }

    // ── Methods ──────────────────────────────────────────────────────────────
    public async Task<Student?> GetStudentByEmail(string email)
    {
        var res = await _http.GetAsync(
            $"{_url}/rest/v1/students?email=eq.{Uri.EscapeDataString(email)}&select=*");
        var body = await res.Content.ReadAsStringAsync();

        var list = JsonSerializer.Deserialize<List<SupabaseStudent>>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var s = list?.FirstOrDefault();
        if (s == null) return null;

        return new Student
        {
            Id = s.Id,
            StudentId = s.StudentId,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Email = s.Email,
            PasswordHash = s.PasswordHash,
            CreatedAt = s.CreatedAt
        };
    }

    public async Task<bool> EmailExists(string email)
    {
        var student = await GetStudentByEmail(email);
        return student != null;
    }

    public async Task<bool> StudentIdExists(string studentId)
    {
        var res = await _http.GetAsync(
            $"{_url}/rest/v1/students?student_id=eq.{Uri.EscapeDataString(studentId)}&select=id");
        var body = await res.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<object>>(body);
        return list?.Count > 0;
    }

    public async Task<Student> CreateStudent(Student student)
    {
        var payload = new
        {
            student_id = student.StudentId,
            first_name = student.FirstName,
            last_name = student.LastName,
            email = student.Email,
            password_hash = student.PasswordHash
        };

        var content = new StringContent(JsonSerializer.Serialize(payload),
            Encoding.UTF8, "application/json");
        content.Headers.Add("Prefer", "return=representation");

        var res = await _http.PostAsync($"{_url}/rest/v1/students", content);
        res.EnsureSuccessStatusCode();

        var body = await res.Content.ReadAsStringAsync();
        var list = JsonSerializer.Deserialize<List<SupabaseStudent>>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var s = list!.First();
        return new Student
        {
            Id = s.Id,
            StudentId = s.StudentId,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Email = s.Email,
            PasswordHash = s.PasswordHash,
            CreatedAt = s.CreatedAt
        };
    }
}