using System.Collections.ObjectModel;
using System.Windows;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Services;
using AtelieHub.Desktop.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtelieHub.Desktop.ViewModels;

public partial class EstoqueViewModel : ObservableObject
{
    private readonly IEstoqueService _estoqueService;
    private readonly IServiceProvider _services;

    public EstoqueViewModel(IEstoqueService estoqueService, IServiceProvider services)
    {
        _estoqueService = estoqueService;
        _services = services;
        _ = CarregarAsync();
    }

    public ObservableCollection<ProdutoEstoque> Produtos { get; } = new();

    [ObservableProperty] private string? _busca;
    [ObservableProperty] private bool _somenteBaixo;
    [ObservableProperty] private bool _mostrarInativos;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string _resumo = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditarCommand))]
    [NotifyCanExecuteChangedFor(nameof(AlternarAtivoCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExcluirCommand))]
    private ProdutoEstoque? _selecionado;

    partial void OnBuscaChanged(string? value) => _ = CarregarAsync();
    partial void OnSomenteBaixoChanged(bool value) => _ = CarregarAsync();
    partial void OnMostrarInativosChanged(bool value) => _ = CarregarAsync();

    [RelayCommand]
    private async Task CarregarAsync()
    {
        try
        {
            Carregando = true;
            var idSelecionado = Selecionado?.Id;

            var lista = await _estoqueService.ListarAsync(Busca, SomenteBaixo, MostrarInativos);
            Produtos.Clear();
            foreach (var p in lista)
            {
                Produtos.Add(p);
            }
            if (idSelecionado is not null)
            {
                Selecionado = Produtos.FirstOrDefault(p => p.Id == idSelecionado);
            }
            Resumo = Produtos.Count == 1 ? "1 item" : $"{Produtos.Count} itens";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível carregar o estoque.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Novo() => AbrirEdicao(null);

    private bool TemSelecao() => Selecionado is not null;

    [RelayCommand(CanExecute = nameof(TemSelecao))]
    private void Editar() => AbrirEdicao(Selecionado);

    [RelayCommand(CanExecute = nameof(TemSelecao))]
    private async Task AlternarAtivoAsync()
    {
        if (Selecionado is null)
        {
            return;
        }

        try
        {
            await _estoqueService.DefinirAtivoAsync(Selecionado.Id, !Selecionado.Ativo);
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível alterar o item.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(TemSelecao))]
    private async Task ExcluirAsync()
    {
        if (Selecionado is null)
        {
            return;
        }

        var confirma = MessageBox.Show(
            $"Excluir o item \"{Selecionado.Nome}\" definitivamente?\n\n" +
            "Esta ação não pode ser desfeita.",
            "Ateliê Hub", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirma != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _estoqueService.RemoverAsync(Selecionado.Id);
            Selecionado = null;
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível excluir o item.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void AbrirEdicao(ProdutoEstoque? produto)
    {
        var janela = _services.GetRequiredService<ProdutoEstoqueEdicaoWindow>();
        janela.Owner = Application.Current.MainWindow;
        janela.ViewModel.Inicializar(produto);

        if (janela.ShowDialog() == true)
        {
            _ = CarregarAsync();
        }
    }
}
