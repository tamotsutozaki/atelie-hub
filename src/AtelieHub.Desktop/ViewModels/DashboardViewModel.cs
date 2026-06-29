using AtelieHub.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AtelieHub.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IEmpresaService _empresaService;

    public DashboardViewModel(IEmpresaService empresaService)
    {
        _empresaService = empresaService;
        _ = CarregarAsync();
    }

    [ObservableProperty]
    private string _saudacao = "Bem-vinda!";

    [ObservableProperty]
    private string _nomeEmpresa = "Ateliê Hub";

    // Indicadores do painel — placeholders nesta v1; ganham dados reais nos próximos módulos.
    [ObservableProperty]
    private string _pedidosEmAberto = "—";

    [ObservableProperty]
    private string _aEntregar = "—";

    [ObservableProperty]
    private string _pendenciasFinanceiras = "—";

    [ObservableProperty]
    private string _estoqueBaixo = "—";

    private async Task CarregarAsync()
    {
        try
        {
            var empresa = await _empresaService.ObterAtualAsync();
            if (empresa is null)
            {
                return;
            }

            NomeEmpresa = empresa.Nome;
            var quem = string.IsNullOrWhiteSpace(empresa.NomeResponsavel) ? empresa.Nome : empresa.NomeResponsavel;
            Saudacao = $"Bem-vinda, {quem}!";
        }
        catch (Exception ex)
        {
            // Carga best-effort: se falhar, mantém os placeholders sem derrubar a UI.
            System.Diagnostics.Debug.WriteLine($"[Dashboard] Falha ao carregar dados: {ex.Message}");
        }
    }
}
