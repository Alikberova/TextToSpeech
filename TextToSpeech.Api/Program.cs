using TextToSpeech.Api.Extensions;
using TextToSpeech.Api.Middleware;
using TextToSpeech.Core.Config;
using TextToSpeech.Core.Entities;
using TextToSpeech.Infra;
using TextToSpeech.Infra.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetConfig();

ConfigureLogging(builder);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebCorsPolicy",
        builder => builder.WithOrigins($"http://localhost:{SharedConstants.ClientPort}",
        $"https://localhost:{SharedConstants.ClientPort}",
        SharedConstants.Domain)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection(ConfigConstants.JwtConfig));

builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection(ConfigConstants.EmailConfig));

builder.Services.Configure<NarakeetConfig>(builder.Configuration.GetSection(ConfigConstants.NarakeetConfig));

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
app.MapHub<AudioHub>(SharedConstants.AudioHubEndpoint);

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

static void ConfigureLogging(WebApplicationBuilder builder)
{
    var logFile = Path.Combine(builder.Configuration[ConfigConstants.AppDataPath]!, "logs", "log_.txt");

    builder.Host.UseSerilog((context, loggerConfig) =>
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;
        var indexFormat = $"{SharedConstants.AppName.ToLower()}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}";

        var elasticConfig = builder.Configuration.GetRequiredSection(nameof(ElasticsearchConfig)).Get<ElasticsearchConfig>()!;

        var elasticSinkOptions = new ElasticsearchSinkOptions(new Uri(elasticConfig.Url))
        {
            AutoRegisterTemplate = true,
            IndexFormat = indexFormat,
            ModifyConnectionSettings = conn =>
            {
                return conn.BasicAuthentication(elasticConfig.Username, elasticConfig.Password);
            }
        };

        loggerConfig.Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(logFile, rollingInterval: RollingInterval.Day)
            .WriteTo.Elasticsearch(elasticSinkOptions)
            .Enrich.WithProperty("Environment", environment)
            .ReadFrom.Configuration(builder.Configuration);
    });
}

public partial class Program { }
