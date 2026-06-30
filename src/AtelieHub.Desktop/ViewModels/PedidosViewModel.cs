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

    // Colunas do Kanban (Cancelado fica de fora: não é etapa do fluxo de produção).
    private static readonly (StatusPedido Status, string Titulo)[] _colunasKanban =
    {
        (StatusPedido.Orcamento, "Orçamento"),
        (StatusPedido.AguardandoPagamento, "Aguardando pagamento"),
        (StatusPedido.EmProducao, "Em produção"),
        (StatusPedido.Pronto, "Pronto"),
        (StatusPedido.Enviado, "Enviado"),
        (StatusPedido.Concluido, "Concluído"),
    };

    public PedidosViewModel(IPedidoService pedidoService, IServiceProvider services)
    {
        _pedidoService = pedidoService;
        _services = services;
        _filtroStatusSelecionado = FiltrosStatus[0]; // "Todos os status" visível ao abrir
        _ = CarregarAsync();
    }

    public ObservableCollection<Pedido> Pedidos { get; } = new();

    /// <summary>Pedidos agrupados por status, para o modo Quadro (Kanban).</summary>
    public ObservableCollection<ColunaKanban> Colunas { get; } = new();

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

    /// <summary>false = lista (DataGrid); true = quadro (Kanban).</summary>
    [ObservableProperty] private bool _modoQuadro;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditarCommand))]
    [NotifyCanExecuteChangedFor(nameof(EtiquetaCommand))]
    private Pedido? _pedidoSelecionado;

    partial void OnBuscaChanged(string? value) => _ = CarregarAsync();
    partial void OnFiltroStatusChanged(StatusPedido? value) => _ = CarregarAsync();
    partial void OnFiltroStatusSelecionadoChanged(OpcaoStatus? value) => FiltroStatus = value?.Valor;

    // Filtro de status que estava ativo na Lista, guardado enquanto o Quadro está aberto.
    private OpcaoStatus? _filtroSalvoLista;

    partial void OnModoQuadroChanged(bool value)
    {
        if (value)
        {
            // No Quadro as colunas já SÃO os status: guarda o filtro da Lista e mostra todas.
            _filtroSalvoLista = FiltroStatusSelecionado;
            if (FiltroStatusSelecionado?.Valor is not null)
            {
                FiltroStatusSelecionado = FiltrosStatus[0];
            }
        }
        else if (_filtroSalvoLista is not null)
        {
            // Voltando para a Lista: devolve o filtro que ela tinha antes de ir ao Quadro.
            FiltroStatusSelecionado = _filtroSalvoLista;
            _filtroSalvoLista = null;
        }
    }

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
            RecriarColunas();
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

    /// <summary>Reconstrói as colunas do Kanban a partir de <see cref="Pedidos"/> (fonte única, já filtrada).</summary>
    private void RecriarColunas()
    {
        Colunas.Clear();
        foreach (var (status, titulo) in _colunasKanban)
        {
            var coluna = new ColunaKanban(status, titulo);
            foreach (var pedido in Pedidos.Where(p => p.Status == status))
            {
                coluna.Itens.Add(pedido);
            }
            Colunas.Add(coluna);
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

    /// <summary>Move um pedido para outro status (usado pelo arrastar-e-soltar do Kanban).</summary>
    public async Task MoverParaStatusAsync(Pedido? pedido, StatusPedido novoStatus)
    {
        if (pedido is null || pedido.Status == novoStatus)
        {
            return;
        }

        try
        {
            await _pedidoService.DefinirStatusAsync(pedido.Id, novoStatus);
            PedidoSelecionado = pedido; // CarregarAsync restaura a seleção por Id
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível mover o pedido.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(TemSelecao))]
    private async Task EtiquetaAsync()
    {
        if (PedidoSelecionado is null)
        {
            return;
        }

        var janela = _services.GetRequiredService<EtiquetaEnvioWindow>();
        janela.Owner = Application.Current.MainWindow;
        await janela.ViewModel.InicializarAsync(PedidoSelecionado);
        janela.ShowDialog();
    }

    [RelayCommand]
    private async Task ModoFocoAsync()
    {
        var janela = _services.GetRequiredService<ModoFocoWindow>();
        janela.Owner = Application.Current.MainWindow;
        await janela.ViewModel.InicializarAsync(3);
        janela.ShowDialog();

        // Pedidos podem ter virado "Pronto" durante o Modo Foco — recarrega para refletir.
        await CarregarAsync();
    }
}
