using System.Windows;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtelieHub.Desktop.ViewModels;

public partial class ClienteEdicaoViewModel : ObservableObject
{
    private readonly IClienteService _clienteService;
    private Guid _id;

    public ClienteEdicaoViewModel(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    /// <summary>Disparado quando o cliente é salvo com sucesso (a janela fecha o diálogo).</summary>
    public event Action? Salvo;

    [ObservableProperty] private string _titulo = "Novo cliente";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private string _nome = string.Empty;

    [ObservableProperty] private string? _telefone;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _instagram;
    [ObservableProperty] private string? _origem;
    [ObservableProperty] private string? _observacoes;

    [ObservableProperty] private string? _cep;
    [ObservableProperty] private string? _logradouro;
    [ObservableProperty] private string? _numero;
    [ObservableProperty] private string? _complemento;
    [ObservableProperty] private string? _bairro;
    [ObservableProperty] private string? _cidade;
    [ObservableProperty] private string? _estado;

    [ObservableProperty] private bool _ativo = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private bool _salvando;

    /// <summary>Prepara o formulário: null = novo cliente; instância = edição.</summary>
    public void Inicializar(Cliente? cliente)
    {
        if (cliente is null)
        {
            _id = Guid.Empty;
            Titulo = "Novo cliente";
            return;
        }

        _id = cliente.Id;
        Titulo = "Editar cliente";
        Nome = cliente.Nome;
        Telefone = cliente.Telefone;
        Email = cliente.Email;
        Instagram = cliente.Instagram;
        Origem = cliente.Origem;
        Observacoes = cliente.Observacoes;
        Cep = cliente.Cep;
        Logradouro = cliente.Logradouro;
        Numero = cliente.Numero;
        Complemento = cliente.Complemento;
        Bairro = cliente.Bairro;
        Cidade = cliente.Cidade;
        Estado = cliente.Estado;
        Ativo = cliente.Ativo;
    }

    private bool PodeSalvar() => !string.IsNullOrWhiteSpace(Nome) && !Salvando;

    [RelayCommand(CanExecute = nameof(PodeSalvar))]
    private async Task SalvarAsync()
    {
        try
        {
            Salvando = true;

            var cliente = new Cliente
            {
                Nome = Nome.Trim(),
                Telefone = Limpar(Telefone),
                Email = Limpar(Email),
                Instagram = Limpar(Instagram),
                Origem = Limpar(Origem),
                Observacoes = Limpar(Observacoes),
                Cep = Limpar(Cep),
                Logradouro = Limpar(Logradouro),
                Numero = Limpar(Numero),
                Complemento = Limpar(Complemento),
                Bairro = Limpar(Bairro),
                Cidade = Limpar(Cidade),
                Estado = Limpar(Estado),
                Ativo = Ativo
            };

            if (_id != Guid.Empty)
            {
                cliente.Id = _id;
            }

            await _clienteService.SalvarAsync(cliente);
            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível salvar o cliente.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Salvando = false;
        }
    }

    private static string? Limpar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
