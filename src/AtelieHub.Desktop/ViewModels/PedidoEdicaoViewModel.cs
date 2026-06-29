using System.Collections.ObjectModel;
using System.Windows;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Enums;
using AtelieHub.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtelieHub.Desktop.ViewModels;

public partial class PedidoEdicaoViewModel : ObservableObject
{
    private readonly IPedidoService _pedidoService;
    private readonly IClienteService _clienteService;
    private Guid _id;

    public PedidoEdicaoViewModel(IPedidoService pedidoService, IClienteService clienteService)
    {
        _pedidoService = pedidoService;
        _clienteService = clienteService;
    }

    public event Action? Salvo;

    public ObservableCollection<Cliente> Clientes { get; } = new();

    public Array TiposDisponiveis => Enum.GetValues(typeof(TipoPedido));
    public Array StatusDisponiveis => Enum.GetValues(typeof(StatusPedido));
    public Array PrioridadesDisponiveis => Enum.GetValues(typeof(PrioridadePedido));
    public Array PagamentosDisponiveis => Enum.GetValues(typeof(StatusPagamento));

    [ObservableProperty] private string _tituloJanela = "Novo pedido";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private Cliente? _clienteSelecionado;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private string _titulo = string.Empty;

    [ObservableProperty] private string? _descricao;
    [ObservableProperty] private TipoPedido _tipo = TipoPedido.Venda;
    [ObservableProperty] private StatusPedido _status = StatusPedido.Orcamento;
    [ObservableProperty] private PrioridadePedido _prioridade = PrioridadePedido.Normal;
    [ObservableProperty] private bool _urgente;
    [ObservableProperty] private DateTime _dataPedido = DateTime.Today;
    [ObservableProperty] private DateTime? _prazoEntrega;
    [ObservableProperty] private decimal _valorTotal;
    [ObservableProperty] private StatusPagamento _statusPagamento = StatusPagamento.Pendente;
    [ObservableProperty] private string? _observacoes;
    [ObservableProperty] private string? _transportadora;
    [ObservableProperty] private string? _codigoRastreio;
    [ObservableProperty] private DateTime? _dataPostagem;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private bool _salvando;

    public async Task InicializarAsync(Pedido? pedido)
    {
        await CarregarClientesAsync();

        if (pedido is null)
        {
            _id = Guid.Empty;
            TituloJanela = "Novo pedido";
            DataPedido = DateTime.Today;
            return;
        }

        _id = pedido.Id;
        TituloJanela = $"Pedido #{pedido.Numero}";
        ClienteSelecionado = await GarantirClienteNaListaAsync(pedido.ClienteId);
        Titulo = pedido.Titulo;
        Descricao = pedido.Descricao;
        Tipo = pedido.Tipo;
        Status = pedido.Status;
        Prioridade = pedido.Prioridade;
        Urgente = pedido.Urgente;
        DataPedido = pedido.DataPedido;
        PrazoEntrega = pedido.PrazoEntrega;
        ValorTotal = pedido.ValorTotal;
        StatusPagamento = pedido.StatusPagamento;
        Observacoes = pedido.Observacoes;
        Transportadora = pedido.Transportadora;
        CodigoRastreio = pedido.CodigoRastreio;
        DataPostagem = pedido.DataPostagem;
    }

    private async Task CarregarClientesAsync()
    {
        try
        {
            var lista = await _clienteService.ListarAsync(null, incluirInativos: false);
            Clientes.Clear();
            foreach (var c in lista)
            {
                Clientes.Add(c);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível carregar os clientes.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Retorna o cliente do pedido garantindo que ele exista na coleção do ComboBox.
    /// A lista só traz clientes ativos; se o cliente vinculado foi inativado depois,
    /// ele é buscado e adicionado para não sumir da seleção (e travar o salvar).
    /// </summary>
    private async Task<Cliente?> GarantirClienteNaListaAsync(Guid clienteId)
    {
        var cliente = Clientes.FirstOrDefault(c => c.Id == clienteId);
        if (cliente is not null)
        {
            return cliente;
        }

        try
        {
            cliente = await _clienteService.ObterAsync(clienteId);
            if (cliente is not null)
            {
                Clientes.Add(cliente);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PedidoEdicao] Falha ao recuperar cliente do pedido: {ex.Message}");
        }

        return cliente;
    }

    private bool PodeSalvar() =>
        !string.IsNullOrWhiteSpace(Titulo) && ClienteSelecionado is not null && !Salvando;

    [RelayCommand(CanExecute = nameof(PodeSalvar))]
    private async Task SalvarAsync()
    {
        if (ClienteSelecionado is null)
        {
            return;
        }

        if (ValorTotal < 0)
        {
            MessageBox.Show("O valor total não pode ser negativo.",
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            Salvando = true;

            var pedido = new Pedido
            {
                ClienteId = ClienteSelecionado.Id,
                Titulo = Titulo.Trim(),
                Descricao = Limpar(Descricao),
                Tipo = Tipo,
                Status = Status,
                Prioridade = Prioridade,
                Urgente = Urgente,
                DataPedido = DataPedido,
                PrazoEntrega = PrazoEntrega,
                ValorTotal = ValorTotal,
                StatusPagamento = StatusPagamento,
                Observacoes = Limpar(Observacoes),
                Transportadora = Limpar(Transportadora),
                CodigoRastreio = Limpar(CodigoRastreio),
                DataPostagem = DataPostagem
            };

            if (_id != Guid.Empty)
            {
                pedido.Id = _id;
            }

            await _pedidoService.SalvarAsync(pedido);
            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível salvar o pedido.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Salvando = false;
        }
    }

    private static string? Limpar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
