using System.Windows;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtelieHub.Desktop.ViewModels;

public partial class OnboardingViewModel : ObservableObject
{
    private readonly IEmpresaService _empresaService;

    public OnboardingViewModel(IEmpresaService empresaService)
    {
        _empresaService = empresaService;
    }

    /// <summary>Disparado quando o cadastro é concluído com sucesso (a janela fecha o diálogo).</summary>
    public event Action? Concluido;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private string _nome = string.Empty;

    [ObservableProperty]
    private string? _nomeResponsavel;

    [ObservableProperty]
    private string? _documento;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private string? _telefone;

    [ObservableProperty]
    private string? _instagram;

    [ObservableProperty]
    private string? _cidade;

    [ObservableProperty]
    private string? _estado;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private bool _salvando;

    private bool PodeSalvar() => !string.IsNullOrWhiteSpace(Nome) && !Salvando;

    [RelayCommand(CanExecute = nameof(PodeSalvar))]
    private async Task SalvarAsync()
    {
        try
        {
            Salvando = true;

            var empresa = new Empresa
            {
                Nome = Nome.Trim(),
                NomeResponsavel = string.IsNullOrWhiteSpace(NomeResponsavel) ? null : NomeResponsavel.Trim(),
                Documento = string.IsNullOrWhiteSpace(Documento) ? null : Documento.Trim(),
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                Telefone = string.IsNullOrWhiteSpace(Telefone) ? null : Telefone.Trim(),
                Instagram = string.IsNullOrWhiteSpace(Instagram) ? null : Instagram.Trim(),
                Cidade = string.IsNullOrWhiteSpace(Cidade) ? null : Cidade.Trim(),
                Estado = string.IsNullOrWhiteSpace(Estado) ? null : Estado.Trim()
            };

            await _empresaService.SalvarAsync(empresa);
            Concluido?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Não foi possível salvar o cadastro.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Salvando = false;
        }
    }
}
