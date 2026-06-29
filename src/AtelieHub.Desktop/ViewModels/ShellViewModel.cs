using AtelieHub.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtelieHub.Desktop.ViewModels;

/// <summary>
/// ViewModel do shell (janela principal): cuida da navegação entre os módulos
/// e dos dados de cabeçalho. Por enquanto só o Painel está ativo;
/// os demais módulos entram nos próximos passos do plano.
/// </summary>
public partial class ShellViewModel : ObservableObject
{
    private readonly IServiceProvider _services;

    public ShellViewModel(IServiceProvider services, IEmpresaService empresaService)
    {
        _services = services;
        _ = CarregarCabecalhoAsync(empresaService);
        NavegarDashboard();
    }

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _tituloSecao = "Painel";

    [ObservableProperty]
    private string _nomeEmpresa = "Ateliê Hub";

    /// <summary>Seção atualmente selecionada — usada para destacar o item no menu lateral.</summary>
    [ObservableProperty]
    private string _secaoAtiva = "Painel";

    [RelayCommand]
    private void NavegarDashboard()
    {
        CurrentView = _services.GetRequiredService<DashboardViewModel>();
        TituloSecao = "Painel";
        SecaoAtiva = "Painel";
    }

    [RelayCommand]
    private void NavegarClientes()
    {
        CurrentView = _services.GetRequiredService<ClientesViewModel>();
        TituloSecao = "Clientes";
        SecaoAtiva = "Clientes";
    }

    [RelayCommand]
    private void NavegarPedidos()
    {
        CurrentView = _services.GetRequiredService<PedidosViewModel>();
        TituloSecao = "Pedidos";
        SecaoAtiva = "Pedidos";
    }

    [RelayCommand]
    private void NavegarFinanceiro()
    {
        CurrentView = _services.GetRequiredService<FinanceiroViewModel>();
        TituloSecao = "Financeiro";
        SecaoAtiva = "Financeiro";
    }

    [RelayCommand]
    private void NavegarEstoque()
    {
        CurrentView = _services.GetRequiredService<EstoqueViewModel>();
        TituloSecao = "Estoque";
        SecaoAtiva = "Estoque";
    }

    [RelayCommand]
    private void NavegarMarketing()
    {
        CurrentView = _services.GetRequiredService<MarketingViewModel>();
        TituloSecao = "Marketing";
        SecaoAtiva = "Marketing";
    }

    private async Task CarregarCabecalhoAsync(IEmpresaService empresaService)
    {
        try
        {
            var empresa = await empresaService.ObterAtualAsync();
            if (empresa is not null)
            {
                NomeEmpresa = empresa.Nome;
            }
        }
        catch (Exception ex)
        {
            // Carga best-effort do cabeçalho: falha não deve derrubar o shell.
            System.Diagnostics.Debug.WriteLine($"[Shell] Falha ao carregar cabeçalho: {ex.Message}");
        }
    }
}
