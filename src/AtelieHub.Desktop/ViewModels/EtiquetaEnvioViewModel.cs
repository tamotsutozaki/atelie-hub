using System.Windows;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtelieHub.Desktop.ViewModels;

/// <summary>
/// Monta a etiqueta de envio de um pedido: destinatário = cliente (endereço estruturado),
/// remetente = empresa (o ateliê). A impressão em si é feita pela janela (via
/// <see cref="ImpressaoSolicitada"/>), onde vive a API de impressão do WPF.
/// </summary>
public partial class EtiquetaEnvioViewModel : ObservableObject
{
    private readonly IEmpresaService _empresaService;
    private readonly IClienteService _clienteService;

    public EtiquetaEnvioViewModel(IEmpresaService empresaService, IClienteService clienteService)
    {
        _empresaService = empresaService;
        _clienteService = clienteService;
    }

    /// <summary>Disparado quando o usuário pede impressão; a janela abre o PrintDialog.</summary>
    public event Action? ImpressaoSolicitada;

    [ObservableProperty] private string _tituloJanela = "Etiqueta de envio";

    // ===== Destinatário (cliente) =====
    [ObservableProperty] private string _destinatarioNome = string.Empty;
    [ObservableProperty] private string _destinatarioEndereco = string.Empty;
    [ObservableProperty] private string? _destinatarioContato;

    // ===== Remetente (empresa) =====
    [ObservableProperty] private string _remetenteNome = string.Empty;
    [ObservableProperty] private string _remetenteEndereco = string.Empty;
    [ObservableProperty] private string? _remetenteContato;

    /// <summary>Endereço do destinatário ausente — etiqueta sai sem endereço de entrega.</summary>
    [ObservableProperty] private bool _destinatarioIncompleto;

    /// <summary>Endereço do remetente ausente (o onboarding não coleta endereço da empresa).</summary>
    [ObservableProperty] private bool _remetenteIncompleto;

    public async Task InicializarAsync(Pedido pedido)
    {
        TituloJanela = $"Etiqueta — Pedido #{pedido.Numero}";

        try
        {
            var cliente = pedido.Cliente ?? await _clienteService.ObterAsync(pedido.ClienteId);
            var empresa = await _empresaService.ObterAtualAsync();

            PreencherDestinatario(cliente);
            PreencherRemetente(empresa);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível montar a etiqueta.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PreencherDestinatario(Cliente? cliente)
    {
        if (cliente is null)
        {
            DestinatarioNome = "(cliente não encontrado)";
            DestinatarioEndereco = string.Empty;
            DestinatarioContato = null;
            DestinatarioIncompleto = true;
            return;
        }

        DestinatarioNome = cliente.Nome;

        var linhaRua = Juntar(" - ", Juntar(", ", cliente.Logradouro, cliente.Numero), cliente.Complemento);
        DestinatarioEndereco = JuntarLinhas(
            linhaRua,
            cliente.Bairro,
            Juntar(" / ", cliente.Cidade, cliente.Estado),
            FormatarCep(cliente.Cep));
        DestinatarioContato = string.IsNullOrWhiteSpace(cliente.Telefone) ? null : "Tel.: " + cliente.Telefone.Trim();
        DestinatarioIncompleto = string.IsNullOrWhiteSpace(DestinatarioEndereco);
    }

    private void PreencherRemetente(Empresa? empresa)
    {
        if (empresa is null)
        {
            RemetenteNome = "(empresa não cadastrada)";
            RemetenteEndereco = string.Empty;
            RemetenteContato = null;
            RemetenteIncompleto = true;
            return;
        }

        RemetenteNome = empresa.Nome;
        RemetenteEndereco = JuntarLinhas(
            empresa.Endereco,
            Juntar(" / ", empresa.Cidade, empresa.Estado),
            FormatarCep(empresa.Cep));

        var contato = Juntar("    ",
            string.IsNullOrWhiteSpace(empresa.Telefone) ? null : "Tel.: " + empresa.Telefone.Trim(),
            string.IsNullOrWhiteSpace(empresa.Email) ? null : empresa.Email.Trim());
        RemetenteContato = string.IsNullOrWhiteSpace(contato) ? null : contato;
        RemetenteIncompleto = string.IsNullOrWhiteSpace(RemetenteEndereco);
    }

    [RelayCommand]
    private void Imprimir() => ImpressaoSolicitada?.Invoke();

    private static string? FormatarCep(string? cep) =>
        string.IsNullOrWhiteSpace(cep) ? null : "CEP " + cep.Trim();

    private static string Juntar(string separador, params string?[] partes) =>
        string.Join(separador, partes.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!.Trim()));

    private static string JuntarLinhas(params string?[] linhas) =>
        string.Join(Environment.NewLine, linhas.Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l!.Trim()));
}
