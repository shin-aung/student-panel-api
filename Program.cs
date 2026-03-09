using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using StudentPortal.API.Services;
var builder = WebApplication.CreateBuilder(args);
// JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
        policy.WithOrigins(
                "http://localhost:5173",   // Vite dev server
                "http://localhost:3000",   // fallback if using CRA
                "https://yourdomain.com"  // add your production URL later
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());        // needed if you send cookies or auth headers
});
// Register services
builder.Services.AddSingleton<SupabaseService>();
builder.Services.AddScoped<AuthService>();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("ReactApp");         // ← Must be BEFORE UseAuthentication
app.UseAuthentication();         // ← After CORS
app.UseAuthorization();          // ← After Authentication

app.MapControllers();
app.Run();