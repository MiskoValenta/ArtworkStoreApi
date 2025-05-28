using ArtworkStoreApi.Repositories;
using ArtworkStoreApi.Services;
using ArtworkStoreApi.Utils;
using Microsoft.EntityFrameworkCore;

namespace ArtworkStoreApi.Data
{
    public static class DataExtensions
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services, string connectionString)
        {
            // SQLite Database context
            services.AddDbContext<DatabaseContext>(options =>
                options.UseSqlite(connectionString));

            // AutoMapper
            services.AddAutoMapper(typeof(DataExtensions).Assembly);

            // Generic repository a services
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped(typeof(IGenericService<,,>), typeof(GenericService<,,>));

            // Specific services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IAppLogger, AppLogger>();

            return services;
        }
    }
}
