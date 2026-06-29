using System.Collections.ObjectModel;
using System.Windows;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Services;
using AtelieHub.Desktop.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtelieHub.Desktop.ViewModels;

public partial class MarketingViewModel : ObservableObject
{
    private readonly IMarketingService _marketingService;
    private readonly IServiceProvider _services;

    public MarketingViewModel(IMarketingService marketingService, IServiceProvider services)
    {
        _marketingService = marketingService;
        _services = services;
        _ = CarregarAsync();
    }

    public ObservableCollection<TarefaMarketing> Tarefas { get; } = new();

    [ObservableProperty] private bool _incluirConcluidas;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string _resumo = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditarCommand))]
    [NotifyCanExecuteChangedFor(nameof(AlternarConcluidoCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoverCommand))]
    private TarefaMarketing? _selecionada;

    partial void OnIncluirConcluidasChanged(bool value) => _ = CarregarAsync();

    [RelayCommand]
    private async Task CarregarAsync()
    {
        try
        {
            Carregando = true;
            var idSelecionada = Selecionada?.Id;

            var lista = await _marketingService.ListarAsync(IncluirConcluidas);
            Tarefas.Clear();
            foreach (var t in lista)
            {
                Tarefas.Add(t);
            }
            if (idSelecionada is not null)
            {
                Selecionada = Tarefas.FirstOrDefault(t => t.Id == idSelecionada);
            }
            Resumo = Tarefas.Count == 1 ? "1 lembrete" : $"{Tarefas.Count} lembretes";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível carregar a agenda de marketing.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Novo() => AbrirEdicao(null);

    private bool TemSelecao() => Selecionada is not null;

    [RelayCommand(CanExecute = nameof(TemSelecao))]
    private void Editar() => AbrirEdicao(Selecionada);

    [RelayCommand(CanExecute = nameof(TemSelecao))]
    private async Task AlternarConcluidoAsync()
    {
        if (Selecionada is null)
        {
            return;
        }

        try
        {
            await _marketingService.DefinirConcluidoAsync(Selecionada.Id, !Selecionada.Concluido);
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível atualizar o lembrete.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(TemSelecao))]
    private async Task RemoverAsync()
    {
        if (Selecionada is null)
        {
            return;
        }

        var confirma = MessageBox.Show(
            $"Remover o lembrete \"{Selecionada.Titulo}\"?",
            "Ateliê Hub", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirma != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _marketingService.RemoverAsync(Selecionada.Id);
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível remover o lembrete.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void AbrirEdicao(TarefaMarketing? tarefa)
    {
        var janela = _services.GetRequiredService<TarefaMarketingEdicaoWindow>();
        janela.Owner = Application.Current.MainWindow;
        janela.ViewModel.Inicializar(tarefa);

        if (janela.ShowDialog() == true)
        {
            _ = CarregarAsync();
        }
    }
}
