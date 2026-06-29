using System.Windows;
using AtelieHub.Core.Services;
using AtelieHub.Desktop.ViewModels;
using AtelieHub.Desktop.Views;
using AtelieHub.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AtelieHub.Desktop;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                config.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddInfrastructure(context.Configuration);

                // ViewModels
                services.AddSingleton<ShellViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<OnboardingViewModel>();

                // Janelas
                services.AddSingleton<ShellWindow>();
                services.AddTransient<OnboardingWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Evita o app encerrar no intervalo entre fechar o onboarding e abrir o shell.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Light);

        try
        {
            await _host.StartAsync();

            // Cria o banco (se preciso) e aplica as migrations pendentes.
            var dbFactory = _host.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
            await using (var db = await dbFactory.CreateDbContextAsync())
            {
                await db.Database.MigrateAsync();
            }

            var empresaService = _host.Services.GetRequiredService<IEmpresaService>();
            var existeEmpresa = await empresaService.ExisteEmpresaAsync();

            if (!existeEmpresa)
            {
                var onboarding = _host.Services.GetRequiredService<OnboardingWindow>();
                var concluiu = onboarding.ShowDialog();
                if (concluiu != true)
                {
                    Shutdown();
                    return;
                }
            }

            var shell = _host.Services.GetRequiredService<ShellWindow>();
            MainWindow = shell;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            shell.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Não foi possível iniciar o Ateliê Hub.\n\n" +
                "Verifique se o PostgreSQL está rodando e se a senha no appsettings.Local.json está correta.\n\n" +
                "Detalhe técnico: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
