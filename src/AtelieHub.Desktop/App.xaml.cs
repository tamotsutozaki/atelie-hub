using System.Windows;
using System.Windows.Threading;
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
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Rede de segurança: qualquer exceção não tratada na thread de UI vira mensagem amigável.
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        // Evita o app encerrar no intervalo entre fechar o onboarding e abrir o shell.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        Wpf.Ui.Appearance.ApplicationThemeManager.Apply(Wpf.Ui.Appearance.ApplicationTheme.Light);

        try
        {
            // Construído aqui (e não no construtor) para que falhas de config/conexão
            // caiam no try/catch amigável em vez de derrubar o processo com diálogo bruto.
            _host = CriarHost();
            await _host.StartAsync();

            // Cria o banco (se preciso) e aplica as migrations pendentes.
            var dbFactory = _host.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
            await using (var db = await dbFactory.CreateDbContextAsync())
            {
                await db.Database.MigrateAsync();
            }

            var empresaService = _host.Services.GetRequiredService<IEmpresaService>();
            if (!await empresaService.ExisteEmpresaAsync())
            {
                var onboarding = _host.Services.GetRequiredService<OnboardingWindow>();
                if (onboarding.ShowDialog() != true)
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
            MostrarErroFatal(ex);
            Shutdown();
        }
    }

    private static IHost CriarHost() =>
        Host.CreateDefaultBuilder()
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
                services.AddTransient<ClientesViewModel>();
                services.AddTransient<ClienteEdicaoViewModel>();

                // Janelas
                services.AddSingleton<ShellWindow>();
                services.AddTransient<OnboardingWindow>();
                services.AddTransient<ClienteEdicaoWindow>();
            })
            .Build();

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MostrarErroFatal(e.Exception);
        e.Handled = true;
        Shutdown();
    }

    private static void MostrarErroFatal(Exception ex)
    {
        MessageBox.Show(
            "Não foi possível iniciar o Ateliê Hub.\n\n" +
            "Verifique se o PostgreSQL está rodando e se a senha no appsettings.Local.json está correta.\n\n" +
            "Detalhe técnico: " + ex.Message,
            "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Síncrono e determinístico: garante shutdown + dispose do Host antes do processo encerrar.
        if (_host is not null)
        {
            try
            {
                _host.StopAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
            }
            finally
            {
                _host.Dispose();
            }
        }

        base.OnExit(e);
    }
}
