using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NiceDentist.Auth.Application.Auth;
using NiceDentist.Auth.Application.Contracts;
using NiceDentist.Auth.Application.EventHandlers;
using NiceDentist.Auth.Application.Events;
using NiceDentist.Auth.Infrastructure;
using NiceDentist.Auth.Infrastructure.Messaging;
using NiceDentist.Auth.Api;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Config
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-change";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "NiceDentist";
var connectionString = builder.Configuration.GetConnectionString("AuthDb") ?? "Server=localhost;Database=NiceDentistAuthDb;Trusted_Connection=True;TrustServerCertificate=True;";

// Configure RabbitMQ
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));

// DI
builder.Services.AddSingleton<IUserRepository>(_ => new SqlUserRepository(connectionString));
builder.Services.AddSingleton<IJwtTokenService>(_ => new JwtTokenService(jwtKey, jwtIssuer));
builder.Services.AddScoped<AuthService>();

// Event handling
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
builder.Services.AddScoped<IEventHandler<CustomerCreatedEvent>, CustomerCreatedEventHandler>();
builder.Services.AddScoped<IEventHandler<DentistCreatedEvent>, DentistCreatedEventHandler>();
builder.Services.AddHostedService<RabbitMqEventConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "NiceDentist.Auth API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Input your JWT token in this format - Bearer {token}"
    });
    options.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            new string[] { }
        }
    });
});

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = key
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    await DataSeeder.SeedAsync(app);
}

app.Run();

/// <summary>
/// Program class made public for integration tests
/// </summary>
public partial class Program { }
