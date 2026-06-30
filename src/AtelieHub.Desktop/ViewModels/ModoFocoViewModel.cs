using System.Windows;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Enums;
using AtelieHub.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtelieHub.Desktop.ViewModels;

/// <summary>
/// "Modo Foco": fila de produção em tela cheia, uma peça por vez. Mostra os pedidos
/// Em produção com prazo vencendo na janela escolhida (mais os sem prazo), ordenados por
/// urgência/prazo, com navegação anterior/próxima e a ação de marcar como pronto.
/// </summary>
public partial class ModoFocoViewModel : ObservableObject
{
    private readonly IPedidoService _pedidoService;
    private readonly List<Pedido> _fila = new();
    private int _indice;

    public ModoFocoViewModel(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    /// <summary>Pede o fechamento da janela (mesma convenção de evento do projeto).</summary>
    public event Action? SaidaSolicitada;

    [ObservableProperty] private Pedido? _pedidoAtual;
    [ObservableProperty] private string _posicao = string.Empty;
    [ObservableProperty] private string _prazoTexto = string.Empty;
    [ObservableProperty] private bool _temItens;
    [ObservableProperty] private bool _vazio = true;

    public async Task InicializarAsync(int dias = 3)
    {
        _fila.Clear();
        _indice = 0;

        try
        {
            var lista = await _pedidoService.ListarEmProducaoVencendoAsync(dias);
            _fila.AddRange(lista);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível carregar a fila de produção.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        AtualizarAtual();
    }

    private void AtualizarAtual()
    {
        TemItens = _fila.Count > 0;
        Vazio = !TemItens;

        if (!TemItens)
        {
            PedidoAtual = null;
            Posicao = string.Empty;
            PrazoTexto = string.Empty;
            NotificarComandos();
            return;
        }

        _indice = Math.Clamp(_indice, 0, _fila.Count - 1);
        PedidoAtual = _fila[_indice];
        Posicao = $"Peça {_indice + 1} de {_fila.Count}";
        PrazoTexto = DescreverPrazo(PedidoAtual);
        NotificarComandos();
    }

    private bool TemAnterior() => TemItens && _indice > 0;
    private bool TemProxima() => TemItens && _indice < _fila.Count - 1;
    private bool PodeMarcarPronto() => TemItens;

    [RelayCommand(CanExecute = nameof(TemAnterior))]
    private void Anterior()
    {
        _indice--;
        AtualizarAtual();
    }

    [RelayCommand(CanExecute = nameof(TemProxima))]
    private void Proxima()
    {
        _indice++;
        AtualizarAtual();
    }

    [RelayCommand(CanExecute = nameof(PodeMarcarPronto))]
    private async Task MarcarProntoAsync()
    {
        // Captura a peça-alvo ANTES do await: a navegação (setas/botões) continua viva durante
        // a gravação e pode mexer em _indice, então a remoção é por referência, não por índice.
        var alvo = PedidoAtual;
        if (alvo is null)
        {
            return;
        }

        try
        {
            await _pedidoService.DefinirStatusAsync(alvo.Id, StatusPedido.Pronto);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível marcar a peça como pronta.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var posicao = _fila.IndexOf(alvo);
        if (posicao >= 0)
        {
            _fila.RemoveAt(posicao);
            if (_indice > posicao)
            {
                _indice--;
            }
        }
        AtualizarAtual();
    }

    [RelayCommand]
    private void Sair() => SaidaSolicitada?.Invoke();

    private void NotificarComandos()
    {
        AnteriorCommand.NotifyCanExecuteChanged();
        ProximaCommand.NotifyCanExecuteChanged();
        MarcarProntoCommand.NotifyCanExecuteChanged();
    }

    private static string DescreverPrazo(Pedido pedido)
    {
        if (pedido.PrazoEntrega is not { } prazo)
        {
            return "Sem prazo definido";
        }

        var dias = (prazo.Date - DateTime.Today).Days;
        var data = prazo.ToString("dd/MM/yyyy");
        var quando = dias switch
        {
            < 0 => $"atrasado há {-dias} dia(s)",
            0 => "vence hoje",
            1 => "vence amanhã",
            _ => $"vence em {dias} dias",
        };
        return $"Prazo: {data} — {quando}";
    }
}
