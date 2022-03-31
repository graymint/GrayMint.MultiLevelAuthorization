using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Persistence;
using MultiLevelAuthorization.Repositories;

namespace MultiLevelAuthorization.ServiceRegistration;
public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>

            options.UseSqlServer(configuration.GetConnectionString("AuthDatabase")));

        services.AddScoped<IAuthManager,AuthManager>();

        return services;
    }
}