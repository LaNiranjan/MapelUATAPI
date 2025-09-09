using MapelRestAPI.DI;
using MapelRestAPI.Entities;
using MapelRestAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>();

// Bind AzureAd
builder.Services.Configure<AzureAdSettings>(builder.Configuration.GetSection("AzureAd"));

// CORS - hardcoded for your Azure Static Web App
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://thankful-island-0a228fb0f.2.azurestaticapps.net")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var azureAdSection = builder.Configuration.GetSection("AzureAd");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Clean Architecture services
builder.Services.AddInfrastructure();
builder.Services.AddScoped<UserCreationHandler>();
builder.Services.AddScoped<UserInvitaionHandler>();
builder.Services.AddScoped<UserAuthHandler>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{azureAdSection["TenantName"]}.ciamlogin.com/{azureAdSection["TenantId"]}/v2.0";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://{azureAdSection["TenantName"]}.ciamlogin.com/{azureAdSection["TenantId"]}/v2.0",
            ValidateAudience = true,
            ValidAudiences = new[]
            {
                azureAdSection["ClientId"]!,
                $"api://{azureAdSection["ClientId"]}"
            },
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
