using System.Collections.ObjectModel;
using System.Windows;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Enums;
using AtelieHub.Core.Services;
using AtelieHub.Desktop.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AtelieHub.Desktop.ViewModels;

public partial class PedidosViewModel : ObservableObject
{
    private readonly IPedidoService _pedidoService;
    private readonly IServiceProvider _services;

    public PedidosViewModel(IPedidoService pedidoService, IServiceProvider services)
    {
        _pedidoService = pedidoService;
        _services = services;
        _filtroStatusSelecionado = FiltrosStatus[0]; // "Todos os status" visível ao abrir
        _ = CarregarAsync();
    }

    public ObservableCollection<Pedido> Pedidos { get; } = new();

    public record OpcaoStatus(string Texto, StatusPedido? Valor);

    public IReadOnlyList<OpcaoStatus> FiltrosStatus { get; } = new List<OpcaoStatus>
    {
        new("Todos os status", null),
        new("Orçamento", StatusPedido.Orcamento),
        new("Aguardando pagamento", StatusPedido.AguardandoPagamento),
        new("Em produção", StatusPedido.EmProducao),
        new("Pronto", StatusPedido.Pronto),
        new("Enviado", StatusPedido.Enviado),
        new("Concluído", StatusPedido.Concluido),
        new("Cancelado", StatusPedido.Cancelado),
    };

    [ObservableProperty] private string? _busca;
    [ObservableProperty] private StatusPedido? _filtroStatus;
    [ObservableProperty] private OpcaoStatus? _filtroStatusSelecionado;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string _resumo = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditarCommand))]
    private Pedido? _pedidoSelecionado;

    partial void OnBuscaChanged(string? value) => _ = CarregarAsync();
    partial void OnFiltroStatusChanged(StatusPedido? value) => _ = CarregarAsync();
    partial void OnFiltroStatusSelecionadoChanged(OpcaoStatus? value) => FiltroStatus = value?.Valor;

    [RelayCommand]
    private async Task CarregarAsync()
    {
        try
        {
            Carregando = true;
            var idSelecionado = PedidoSelecionado?.Id;
            var lista = await _pedidoService.ListarAsync(Busca, FiltroStatus);
            Pedidos.Clear();
            foreach (var p in lista)
            {
                Pedidos.Add(p);
            }
            if (idSelecionado is not null)
            {
                PedidoSelecionado = Pedidos.FirstOrDefault(p => p.Id == idSelecionado);
            }
            Resumo = Pedidos.Count == 1 ? "1 pedido" : $"{Pedidos.Count} pedidos";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível carregar os pedidos.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private Task Novo() => AbrirEdicaoAsync(null);

    private bool TemSelecao() => PedidoSelecionado is not null;

    [RelayCommand(CanExecute = nameof(TemSelecao))]
    private Task Editar() => AbrirEdicaoAsync(PedidoSelecionado);

    public async Task AbrirEdicaoAsync(Pedido? pedido)
    {
        var janela = _services.GetRequiredService<PedidoEdicaoWindow>();
        janela.Owner = Application.Current.MainWindow;
        await janela.ViewModel.InicializarAsync(pedido);

        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }
}
