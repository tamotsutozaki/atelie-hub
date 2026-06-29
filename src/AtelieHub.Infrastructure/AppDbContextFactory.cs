using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AtelieHub.Infrastructure;

/// <summary>
/// Fábrica usada APENAS em design-time pelas ferramentas do EF (migrations).
/// Localiza a raiz da solução e lê a connection string dos appsettings do projeto Desktop,
/// mantendo uma única fonte de verdade para a configuração do banco.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var desktopDir = LocalizarPastaDesktop();

        var config = new ConfigurationBuilder()
            .SetBasePath(desktopDir)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .Build();

        var connectionString = config.GetConnectionString("AtelieHub")
            ?? throw new InvalidOperationException(
                "Connection string 'AtelieHub' não encontrada nos appsettings do projeto Desktop.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static string LocalizarPastaDesktop()
    {
        // Sobe a árvore de diretórios até achar a raiz que contém src/AtelieHub.Desktop.
        var relativo = Path.Combine("src", "AtelieHub.Desktop");
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, relativo)))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new InvalidOperationException(
                "Não foi possível localizar a pasta do projeto Desktop a partir de " + AppContext.BaseDirectory);
        }

        return Path.Combine(dir.FullName, relativo);
    }
}
