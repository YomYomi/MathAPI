using CalculatorAPI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// JWT Configuration
var jwtKey = GenerateJwtKey();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
        };
    });

// Authorization configuration
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

// Minimal API configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Calculator API", Version = "v1" });
    c.OperationFilter<HeaderOperationFilter>(); // Register the operation filter

    // Define the security scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Set up JWT bearer authentication globally for all endpoints
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Calculator API V1");
});

// Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Custom middleware to handle authentication errors
app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/api/token"))
    {
        if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
        {
            context.Response.StatusCode = 401; // Unauthorized
            return;
        }
    }
    await next();
});

// Token generation endpoint
app.MapPost("/api/token", async (HttpContext context) =>
{
    var clientId = "sampleClientId"; // Sample client ID
    var clientSecret = "sampleClientSecret"; // Sample client secret

    // Validate client ID and secret (this is just a sample, actual implementation may vary)
    var providedClientId = context.Request.Form["clientId"];
    var providedClientSecret = context.Request.Form["clientSecret"];

    if (providedClientId != clientId || providedClientSecret != clientSecret)
    {
        context.Response.StatusCode = 400; // Bad Request
        await context.Response.WriteAsync("Invalid client credentials.");
        return;
    }

    // Generate JWT token
    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Expires = DateTime.UtcNow.AddMinutes(10),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(jwtKey), SecurityAlgorithms.HmacSha256Signature)
    };
    var token = tokenHandler.CreateToken(tokenDescriptor);
    await context.Response.WriteAsync(tokenHandler.WriteToken(token));
});

// Calculator endpoint
app.MapPost("/api/calculate", async (HttpContext context, CalculatorRequest request) =>
{

    var operationHeader = context.Request.Headers["Operation"];
    if (string.IsNullOrEmpty(operationHeader))
    {
        context.Response.StatusCode = 400; // Bad Request
        await context.Response.WriteAsync("Operation header is missing.");
        return;
    }

    var operation = operationHeader.ToString().ToLower();
    var resultValue = 0.0;

    switch (operation)
    {
        case "add":
            resultValue = request.Number1 + request.Number2;
            break;
        case "subtract":
            resultValue = request.Number1 - request.Number2;
            break;
        case "multiply":
            resultValue = request.Number1 * request.Number2;
            break;
        case "divide":
            if (request.Number2 == 0)
            {
                context.Response.StatusCode = 400; // Bad Request
                await context.Response.WriteAsync("Cannot divide by zero.");
                return;
            }
            resultValue = request.Number1 / request.Number2;
            break;
        default:
            context.Response.StatusCode = 400; // Bad Request
            await context.Response.WriteAsync("Invalid operation.");
            return;
    }

    await context.Response.WriteAsJsonAsync(new { result = resultValue });
});

app.Run();


// Function to generate JWT key
byte[] GenerateJwtKey()
{
    using var hmac = new HMACSHA256();
    return hmac.Key;
}
