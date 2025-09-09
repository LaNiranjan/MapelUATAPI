using MapelRestAPI.DI;
using MapelRestAPI.Entities;
using MapelRestAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// add this for support https
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5004, listenOptions =>
    {
        listenOptions.UseHttps(); // Uses dev cert by default
    });
});

// Load configuration from appsettings.json, environment variables, and user secrets
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>(); // For local dev secret storage

// Bind AzureAd configuration section to a strongly typed class
builder.Services.Configure<AzureAdSettings>(builder.Configuration.GetSection("AzureAd"));

// Enable CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:8080", "https://thankful-island-0bc34d10f.2.azurestaticapps.net/") // AngularJS frontend
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Extract auth settings
var azureAdSection = builder.Configuration.GetSection("AzureAd");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Clean Architecture dependencies
builder.Services.AddInfrastructure();
builder.Services.AddScoped<UserCreationHandler>();
builder.Services.AddScoped<UserInvitaionHandler>();
builder.Services.AddScoped<UserAuthHandler>();

// Configure JWT authentication
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
                $"{azureAdSection["ClientId"]}",
                $"api://{azureAdSection["ClientId"]}"
            },
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();

// Configure middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontendLocalhost");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
