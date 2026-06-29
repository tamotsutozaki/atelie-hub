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

    [RelayCommand]
    private void NavegarDashboard()
    {
        CurrentView = _services.GetRequiredService<DashboardViewModel>();
        TituloSecao = "Painel";
    }

    private async Task CarregarCabecalhoAsync(IEmpresaService empresaService)
    {
        var empresa = await empresaService.ObterAtualAsync();
        if (empresa is not null)
        {
            NomeEmpresa = empresa.Nome;
        }
    }
}
