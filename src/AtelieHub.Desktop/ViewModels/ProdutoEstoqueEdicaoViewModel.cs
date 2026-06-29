using System.Windows;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtelieHub.Desktop.ViewModels;

public partial class ProdutoEstoqueEdicaoViewModel : ObservableObject
{
    private readonly IEstoqueService _estoqueService;
    private Guid _id;

    public ProdutoEstoqueEdicaoViewModel(IEstoqueService estoqueService)
    {
        _estoqueService = estoqueService;
    }

    public event Action? Salvo;

    [ObservableProperty] private string _tituloJanela = "Novo item";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private string _nome = string.Empty;

    [ObservableProperty] private string? _unidade;
    [ObservableProperty] private decimal _quantidade;
    [ObservableProperty] private decimal _estoqueMinimo;
    [ObservableProperty] private decimal? _custo;
    [ObservableProperty] private string? _observacoes;
    [ObservableProperty] private bool _ativo = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private bool _salvando;

    public void Inicializar(ProdutoEstoque? produto)
    {
        if (produto is null)
        {
            _id = Guid.Empty;
            TituloJanela = "Novo item";
            return;
        }

        _id = produto.Id;
        TituloJanela = "Editar item";
        Nome = produto.Nome;
        Unidade = produto.Unidade;
        Quantidade = produto.Quantidade;
        EstoqueMinimo = produto.EstoqueMinimo;
        Custo = produto.Custo;
        Observacoes = produto.Observacoes;
        Ativo = produto.Ativo;
    }

    private bool PodeSalvar() => !string.IsNullOrWhiteSpace(Nome) && !Salvando;

    [RelayCommand(CanExecute = nameof(PodeSalvar))]
    private async Task SalvarAsync()
    {
        if (Quantidade < 0 || EstoqueMinimo < 0 || Custo < 0)
        {
            MessageBox.Show("Quantidade, estoque mínimo e custo não podem ser negativos.",
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            Salvando = true;

            var produto = new ProdutoEstoque
            {
                Nome = Nome.Trim(),
                Unidade = string.IsNullOrWhiteSpace(Unidade) ? null : Unidade.Trim(),
                Quantidade = Quantidade,
                EstoqueMinimo = EstoqueMinimo,
                Custo = Custo,
                Observacoes = string.IsNullOrWhiteSpace(Observacoes) ? null : Observacoes.Trim(),
                Ativo = Ativo
            };

            if (_id != Guid.Empty)
            {
                produto.Id = _id;
            }

            await _estoqueService.SalvarAsync(produto);
            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível salvar o item.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Salvando = false;
        }
    }
}
