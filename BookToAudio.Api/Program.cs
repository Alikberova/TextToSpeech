using BookToAudio.Api.Extensions;
using BookToAudio.Api.Middleware;
using BookToAudio.Core;
using BookToAudio.Core.Config;
using BookToAudio.Core.Entities;
using BookToAudio.Infra;
using BookToAudio.Infra.Services.Common;
using BookToAudio.RealTime;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = HostingEnvironment.IsDevelopment();

var apiDir = PathService.GetProjectDirectory(SharedConstants.ServerProjectName);
var remoteEnvFilePath = Path.Combine(Directory.GetParent(apiDir)!.ToString(), ".env");

DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: isDevelopment, envFilePaths: new[] { remoteEnvFilePath }));

builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: !isDevelopment, reloadOnChange: true)
    .AddEnvironmentVariables();

var appDataPath = builder.Configuration[ConfigConstants.AppDataPath]!;

var logFile = Path.Combine(appDataPath, "Logs", "log_.txt");

builder.Host.UseSerilog((context, config) =>
    config.MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(logFile, rollingInterval: RollingInterval.Hour));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebCorsPolicy",
        builder => builder.WithOrigins("http://localhost:4000") //todo config
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(ConfigConstants.JwtConfig));

builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection(ConfigConstants.EmailConfig));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(b => b.UseNpgsql(connectionString));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

AddAuthentication(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("WebCorsPolicy"); // CORS should be before other middleware

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();
app.MapHub<AudioHub>("/audioHub");

app.Run();

static void AddAuthentication(WebApplicationBuilder builder)
{
    var jwtConfig = builder.Configuration.GetRequiredSection(ConfigConstants.JwtConfig).Get<JwtConfig>();

    var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig!.Symmetric.Key));

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = symmetricKey
            };
        });
}

public partial class Program { }
