using AutoMapper;
using Core.Services;
using Core.Services.Abstraction;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Articles.Api.Mapping;
using Articles.Api.Configuration;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Amazon.S3;
using Microsoft.OpenApi.Models;
using Articles.Api.Middleware;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Articles.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("RestrictedCors", corsBuilder =>
                {
                    var configuredOrigins = builder.Configuration
                        .GetSection("Cors:AllowedOrigins")
                        .Get<string[]>() ?? Array.Empty<string>();

                    if (configuredOrigins.Length > 0)
                    {
                        corsBuilder.WithOrigins(configuredOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                        return;
                    }

                    if (builder.Environment.IsDevelopment())
                    {
                        corsBuilder.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:8080")
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                        return;
                    }

                    throw new InvalidOperationException("No CORS origins configured for non-development environment.");
                });
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "Articles API", 
                    Version = "v1",
                    Description = "A .NET 10 Web API for managing articles and newspapers",
                    Contact = new OpenApiContact
                    {
                        Name = "Articles API Support",
                        Email = "support@articlesapi.com"
                    }
                });

                // Configure JWT Authentication for Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

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
                        new string[] {}
                    }
                });

                // Add XML comments if available
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            // Add JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured");
            }

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // Add DbContext - Enable Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));

            // Add AutoMapper
            var mapperLoggerFactory = LoggerFactory.Create(builder => { });
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ArticleMappingProfile>();
                cfg.AddProfile<NewspaperMappingProfile>();
            }, mapperLoggerFactory);
            builder.Services.AddSingleton(mapperConfig);
            builder.Services.AddSingleton<IMapper>(sp => sp.GetRequiredService<MapperConfiguration>().CreateMapper());
            
            // Add Memory Cache
            builder.Services.AddMemoryCache();

            // Validate storage configuration
            ServiceRegistration.ValidateStorageConfiguration(builder.Configuration);
            
            // Add Services with Repository Pattern
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
            builder.Services.AddScoped<INewspaperRepository, NewspaperRepository>();
            builder.Services.AddScoped<INewspaperService, NewspaperService>();
            builder.Services.AddScoped<JwtService>();
            
            // Configure Article Services based on storage provider
            builder.Services.AddArticleServices(builder.Configuration);
            
            // Log storage provider selection
            var storageProvider = ServiceRegistration.GetStorageProviderName(builder.Configuration);
            Log.Information("Application configured to use {StorageProvider} for article storage", storageProvider);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Articles API V1");
                    c.RoutePrefix = "swagger"; // Serve Swagger UI at /swagger
                });
            }

            // Only use HTTPS redirection in production
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Use CORS
            app.UseCors("RestrictedCors");

            app.UseMiddleware<GlobalExceptionHandler>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // Apply migrations and seed data
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                try
                {
                    // Apply migrations to create/update database
                    context.Database.Migrate();
                    
                    // Seed data
                    DataSeeder.SeedDataAsync(context).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during database migration or seeding");
                }
            }

            app.Run();
        }
    }
}
