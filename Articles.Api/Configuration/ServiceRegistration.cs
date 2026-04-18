using Core.Services;
using Core.Services.Abstraction;
using Infrastructure.Persistence.Repositories;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Articles.Api.Configuration
{
    /// <summary>
    /// Service registration configuration for Articles API
    /// Handles dynamic service registration based on storage configuration
    /// </summary>
    public static class ServiceRegistration
    {
        /// <summary>
        /// Adds article services to the dependency injection container
        /// Registers either SqlArticleService or S3ArticleService based on configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddArticleServices(this IServiceCollection services, IConfiguration configuration)
        {
            var useS3 = configuration.GetValue<bool>("StorageSettings:UseS3", false);
            // Note: Logger will be available after service provider is built
            
            if (useS3)
            {
                // Register S3 services
                services.AddAWSService<IAmazonS3>();
                services.AddScoped<IS3FileProvider, S3FileProvider>();
                services.AddScoped<IArticleService, S3ArticleService>();
            }
            else
            {
                // Register SQL Server service
                services.AddScoped<IArticleService, SqlArticleService>();
            }

            return services;
        }

        /// <summary>
        /// Adds common services required by both storage implementations
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            // Repository services (always needed for SQL Server and potentially for other operations)
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IArticleRepository, ArticleRepository>();
            services.AddScoped<INewspaperRepository, NewspaperRepository>();
            
            // Other common services
            services.AddScoped<INewspaperService, NewspaperService>();
            services.AddScoped<JwtService>();
            
            return services;
        }

        /// <summary>
        /// Validates the storage configuration
        /// </summary>
        /// <param name="configuration">The application configuration</param>
        /// <exception cref="InvalidOperationException">Thrown when configuration is invalid</exception>
        public static void ValidateStorageConfiguration(IConfiguration configuration)
        {
            var useS3 = configuration.GetValue<bool>("StorageSettings:UseS3", false);
            
            if (useS3)
            {
                // Validate S3 configuration
                var awsSection = configuration.GetSection("AWS");
                var bucketName = awsSection["BucketName"];
                var region = awsSection["Region"];
                
                if (string.IsNullOrEmpty(bucketName))
                {
                    throw new InvalidOperationException("AWS:BucketName is required when using S3 storage");
                }
                
                if (string.IsNullOrEmpty(region))
                {
                    throw new InvalidOperationException("AWS:Region is required when using S3 storage");
                }
            }
            else
            {
                // Validate SQL Server configuration
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("DefaultConnection is required when using SQL Server storage");
                }
            }
        }

        /// <summary>
        /// Gets the current storage provider name for logging/debugging
        /// </summary>
        /// <param name="configuration">The application configuration</param>
        /// <returns>The storage provider name</returns>
        public static string GetStorageProviderName(IConfiguration configuration)
        {
            var useS3 = configuration.GetValue<bool>("StorageSettings:UseS3", false);
            return useS3 ? "Amazon S3" : "SQL Server";
        }
    }
}