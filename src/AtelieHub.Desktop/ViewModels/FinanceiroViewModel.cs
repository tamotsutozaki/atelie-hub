using System.Collections.ObjectModel;
using System.Windows;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Services;
using AtelieHub.Desktop.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtelieHub.Desktop.ViewModels;

public partial class FinanceiroViewModel : ObservableObject
{
    private readonly IFinanceiroService _financeiroService;
    private readonly IServiceProvider _services;

    public FinanceiroViewModel(IFinanceiroService financeiroService, IServiceProvider services)
    {
        _financeiroService = financeiroService;
        _services = services;
        _ = CarregarAsync();
    }

    public ObservableCollection<LancamentoFinanceiro> Lancamentos { get; } = new();

    [ObservableProperty] private decimal _entradas;
    [ObservableProperty] private decimal _saidas;
    [ObservableProperty] private decimal _saldo;
    [ObservableProperty] private bool _carregando;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditarCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoverCommand))]
    private LancamentoFinanceiro? _selecionado;

    [RelayCommand]
    private async Task CarregarAsync()
    {
        try
        {
            Carregando = true;
            var idSelecionado = Selecionado?.Id;

            var lista = await _financeiroService.ListarAsync();
            Lancamentos.Clear();
            foreach (var l in lista)
            {
                Lancamentos.Add(l);
            }
            if (idSelecionado is not null)
            {
                Selecionado = Lancamentos.FirstOrDefault(l => l.Id == idSelecionado);
            }

            var resumo = await _financeiroService.ResumoAsync();
            Entradas = resumo.Entradas;
            Saidas = resumo.Saidas;
            Saldo = resumo.Saldo;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível carregar o financeiro.\n\nDetalhe: " + ex.Message,
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
    private async Task RemoverAsync()
    {
        if (Selecionado is null)
        {
            return;
        }

        var confirma = MessageBox.Show(
            $"Remover o lançamento \"{Selecionado.Descricao}\"?",
            "Ateliê Hub", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirma != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _financeiroService.RemoverAsync(Selecionado.Id);
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível remover o lançamento.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void AbrirEdicao(LancamentoFinanceiro? lancamento)
    {
        var janela = _services.GetRequiredService<LancamentoEdicaoWindow>();
        janela.Owner = Application.Current.MainWindow;
        janela.ViewModel.Inicializar(lancamento);

        if (janela.ShowDialog() == true)
        {
            _ = CarregarAsync();
        }
    }
}
