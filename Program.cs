using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskManagerAPI.Data;
using TaskManagerAPI.Token;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
Env.Load();

// Validate JWT secret key
var secretKey = Env.GetString("JWT_SECRET_KEY");
if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
{
    throw new InvalidOperationException("JWT_SECRET_KEY must be at least 32 characters long.");
}

builder.Services.AddControllers();
builder.Services.AddDbContext<Context>(options =>
{
    var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "TaskManager.db");
    options.UseSqlite($"Data Source={dbPath}");
});

var tokenSettings = new TokenSettings
{
    SecretKey = secretKey 
};
builder.Services.AddSingleton(tokenSettings); 
builder.Services.AddScoped<TokenHelper>(provider =>
    new TokenHelper(tokenSettings.SecretKey));

// Add JWT Authentication
var key = Encoding.UTF8.GetBytes(tokenSettings.SecretKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; 
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key) 
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();