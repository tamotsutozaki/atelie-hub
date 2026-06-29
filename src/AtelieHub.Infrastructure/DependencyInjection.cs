using AtelieHub.Core.Services;
using AtelieHub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AtelieHub.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra o acesso a dados e os serviços de negócio.
    /// Usa DbContextFactory (padrão recomendado para apps desktop): cada operação cria
    /// um contexto curto, evitando problemas de lifetime/scope que existem em UI stateful.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AtelieHub")
            ?? throw new InvalidOperationException(
                "Connection string 'AtelieHub' não configurada. Verifique o appsettings.Local.json do projeto Desktop.");

        services.AddDbContextFactory<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddSingleton<IEmpresaService, EmpresaService>();

        return services;
    }
}
