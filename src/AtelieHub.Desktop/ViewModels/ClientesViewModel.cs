using System.Collections.ObjectModel;
using System.Windows;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Services;
using AtelieHub.Desktop.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtelieHub.Desktop.ViewModels;

public partial class ClientesViewModel : ObservableObject
{
    private readonly IClienteService _clienteService;
    private readonly IServiceProvider _services;

    public ClientesViewModel(IClienteService clienteService, IServiceProvider services)
    {
        _clienteService = clienteService;
        _services = services;
        _ = CarregarAsync();
    }

    public ObservableCollection<Cliente> Clientes { get; } = new();

    [ObservableProperty] private string? _busca;
    [ObservableProperty] private bool _mostrarInativos;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string _resumo = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditarCommand))]
    [NotifyCanExecuteChangedFor(nameof(AlternarAtivoCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExcluirCommand))]
    private Cliente? _clienteSelecionado;

    partial void OnBuscaChanged(string? value) => _ = CarregarAsync();
    partial void OnMostrarInativosChanged(bool value) => _ = CarregarAsync();

    [RelayCommand]
    private async Task CarregarAsync()
    {
        try
        {
            Carregando = true;
            var idSelecionado = ClienteSelecionado?.Id;
            var lista = await _clienteService.ListarAsync(Busca, MostrarInativos);
            Clientes.Clear();
            foreach (var c in lista)
            {
                Clientes.Add(c);
            }
            // Preserva a seleção (e os botões de ação) após recarregar a lista.
            if (idSelecionado is not null)
            {
                ClienteSelecionado = Clientes.FirstOrDefault(c => c.Id == idSelecionado);
            }
            Resumo = Clientes.Count == 1 ? "1 cliente" : $"{Clientes.Count} clientes";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível carregar os clientes.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Novo() => AbrirEdicao(null);

    private bool TemSelecao() => ClienteSelecionado is not null;

    [RelayCommand(CanExecute = nameof(TemSelecao))]
    private void Editar() => AbrirEdicao(ClienteSelecionado);

    [RelayCommand(CanExecute = nameof(TemSelecao))]
    private async Task AlternarAtivoAsync()
    {
        if (ClienteSelecionado is null)
        {
            return;
        }

        var alvo = !ClienteSelecionado.Ativo;
        try
        {
            await _clienteService.DefinirAtivoAsync(ClienteSelecionado.Id, alvo);
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível alterar a situação do cliente.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(TemSelecao))]
    private async Task ExcluirAsync()
    {
        if (ClienteSelecionado is null)
        {
            return;
        }

        var confirma = MessageBox.Show(
            $"Excluir o cliente \"{ClienteSelecionado.Nome}\" definitivamente?\n\n" +
            "Esta ação não pode ser desfeita.",
            "Ateliê Hub", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirma != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _clienteService.RemoverAsync(ClienteSelecionado.Id);
            ClienteSelecionado = null;
            await CarregarAsync();
        }
        catch (InvalidOperationException ex)
        {
            // Cliente com pedidos vinculados: mensagem amigável já vem pronta do serviço.
            MessageBox.Show(ex.Message, "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível excluir o cliente.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>Abre o diálogo de cadastro/edição. Recarrega a lista se algo foi salvo.</summary>
    public void AbrirEdicao(Cliente? cliente)
    {
        var janela = _services.GetRequiredService<ClienteEdicaoWindow>();
        janela.Owner = Application.Current.MainWindow;
        janela.ViewModel.Inicializar(cliente);

        if (janela.ShowDialog() == true)
        {
            _ = CarregarAsync();
        }
    }
}
