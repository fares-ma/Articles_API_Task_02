using AutoMapper;
using Core.Services;
using Core.Services.Abstraction;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Articles.Api.Mapping;
using Serilog;

namespace Articles.Api
{
    #region summary

    /// Main entry point for the Articles API application.
    /// 
    /// Purpose:
    /// - Configures and starts the ASP.NET Core web application
    /// - Sets up dependency injection container
    /// - Configures services, middleware, and database connections
    /// - Seeds initial data for development and testing
    /// 
    /// Dependencies:
    /// - Entity Framework Core for database operations
    /// - AutoMapper for object mapping
    /// - Swagger for API documentation
    /// - Custom services and repositories
    /// 
    /// Alternatives:
    /// - Could use different DI containers (Autofac, Ninject)
    /// - Could implement different database providers (PostgreSQL, MySQL)
    /// - Could use different API documentation tools (Redoc, Postman)

    #endregion
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddControllers();
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add AutoMapper
            builder.Services.AddAutoMapper(typeof(ArticleMappingProfile), typeof(NewspaperMappingProfile));

            // Add Services
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
            builder.Services.AddScoped<IArticleService, ArticleService>();
            builder.Services.AddScoped<INewspaperRepository, NewspaperRepository>();
            builder.Services.AddScoped<INewspaperService, NewspaperService>();

            #region To register AWS S3 service for S3 article fetching, add this line in the DI setup:
            // builder.Services.AddAWSService<IAmazonS3>(); 
            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            // Seed data
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await DataSeeder.SeedDataAsync(context);
            }

            app.Run();
        }
    }
}
