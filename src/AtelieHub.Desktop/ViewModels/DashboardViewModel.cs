using System.Windows;
using AtelieHub.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtelieHub.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IEmpresaService _empresaService;
    private readonly IDashboardService _dashboardService;
    private readonly IBackupService _backupService;

    public DashboardViewModel(
        IEmpresaService empresaService,
        IDashboardService dashboardService,
        IBackupService backupService)
    {
        _empresaService = empresaService;
        _dashboardService = dashboardService;
        _backupService = backupService;
        _ = CarregarAsync(silencioso: true);
    }

    [ObservableProperty] private string _saudacao = "Bem-vinda!";
    [ObservableProperty] private string _nomeEmpresa = "Ateliê Hub";

    [ObservableProperty] private int _pedidosEmAberto;
    [ObservableProperty] private int _aEntregar;
    [ObservableProperty] private int _pendenciasFinanceiras;
    [ObservableProperty] private int _estoqueBaixo;
    [ObservableProperty] private int _marketingPendente;
    [ObservableProperty] private decimal _saldoCaixa;

    [ObservableProperty] private bool _ocupado;

    /// <summary>Sinaliza que a última carga do painel falhou (para a UI avisar em vez de mostrar zeros como se fossem reais).</summary>
    [ObservableProperty] private bool _erroAoCarregar;

    [RelayCommand]
    private Task Atualizar() => CarregarAsync(silencioso: false);

    private async Task CarregarAsync(bool silencioso = true)
    {
        try
        {
            ErroAoCarregar = false;
            var empresa = await _empresaService.ObterAtualAsync();
            if (empresa is not null)
            {
                NomeEmpresa = empresa.Nome;
                var quem = string.IsNullOrWhiteSpace(empresa.NomeResponsavel) ? empresa.Nome : empresa.NomeResponsavel;
                Saudacao = $"Bem-vinda, {quem}!";
            }

            var resumo = await _dashboardService.ObterResumoAsync();
            PedidosEmAberto = resumo.PedidosEmAberto;
            AEntregar = resumo.AEntregar;
            PendenciasFinanceiras = resumo.PendenciasFinanceiras;
            EstoqueBaixo = resumo.EstoqueBaixo;
            MarketingPendente = resumo.MarketingPendente;
            SaldoCaixa = resumo.SaldoCaixa;
        }
        catch (Exception ex)
        {
            ErroAoCarregar = true;
            if (silencioso)
            {
                System.Diagnostics.Debug.WriteLine($"[Dashboard] Falha ao carregar: {ex.Message}");
            }
            else
            {
                MessageBox.Show("Não foi possível atualizar o painel.\n\nDetalhe: " + ex.Message,
                    "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task FazerBackupAsync()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            FileName = _backupService.SugerirNomeArquivo(DateTime.Now),
            Filter = "Backup do Ateliê Hub (*.dump)|*.dump|Todos os arquivos (*.*)|*.*",
            Title = "Salvar backup do Ateliê Hub"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            Ocupado = true;
            await _backupService.CriarBackupAsync(dialog.FileName);
            MessageBox.Show($"Backup salvo com sucesso em:\n{dialog.FileName}",
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível fazer o backup.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Ocupado = false;
        }
    }
}
