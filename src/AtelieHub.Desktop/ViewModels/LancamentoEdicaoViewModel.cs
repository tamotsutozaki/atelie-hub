using System.Windows;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Enums;
using AtelieHub.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtelieHub.Desktop.ViewModels;

public partial class LancamentoEdicaoViewModel : ObservableObject
{
    private readonly IFinanceiroService _financeiroService;
    private Guid _id;

    public LancamentoEdicaoViewModel(IFinanceiroService financeiroService)
    {
        _financeiroService = financeiroService;
    }

    public event Action? Salvo;

    public Array TiposDisponiveis => Enum.GetValues(typeof(TipoLancamento));

    [ObservableProperty] private string _tituloJanela = "Novo lançamento";
    [ObservableProperty] private TipoLancamento _tipo = TipoLancamento.Entrada;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private string _descricao = string.Empty;

    [ObservableProperty] private string? _categoria;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private decimal _valor;

    [ObservableProperty] private DateTime _data = DateTime.Today;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private bool _salvando;

    public void Inicializar(LancamentoFinanceiro? lancamento)
    {
        if (lancamento is null)
        {
            _id = Guid.Empty;
            TituloJanela = "Novo lançamento";
            Data = DateTime.Today;
            return;
        }

        _id = lancamento.Id;
        TituloJanela = "Editar lançamento";
        Tipo = lancamento.Tipo;
        Descricao = lancamento.Descricao;
        Categoria = lancamento.Categoria;
        Valor = lancamento.Valor;
        Data = lancamento.Data;
    }

    private bool PodeSalvar() => !string.IsNullOrWhiteSpace(Descricao) && Valor > 0 && !Salvando;

    [RelayCommand(CanExecute = nameof(PodeSalvar))]
    private async Task SalvarAsync()
    {
        try
        {
            Salvando = true;

            var lancamento = new LancamentoFinanceiro
            {
                Tipo = Tipo,
                Descricao = Descricao.Trim(),
                Categoria = string.IsNullOrWhiteSpace(Categoria) ? null : Categoria.Trim(),
                Valor = Valor,
                Data = Data
            };

            if (_id != Guid.Empty)
            {
                lancamento.Id = _id;
            }

            await _financeiroService.SalvarAsync(lancamento);
            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível salvar o lançamento.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Salvando = false;
        }
    }
}
