using System.Windows;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Enums;
using AtelieHub.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AtelieHub.Desktop.ViewModels;

public partial class TarefaMarketingEdicaoViewModel : ObservableObject
{
    private readonly IMarketingService _marketingService;
    private Guid _id;

    public TarefaMarketingEdicaoViewModel(IMarketingService marketingService)
    {
        _marketingService = marketingService;
    }

    public event Action? Salvo;

    public Array RedesDisponiveis => Enum.GetValues(typeof(RedeSocial));
    public Array TiposDisponiveis => Enum.GetValues(typeof(TipoPublicacao));

    [ObservableProperty] private string _tituloJanela = "Novo lembrete";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private string _titulo = string.Empty;

    [ObservableProperty] private string? _descricao;
    [ObservableProperty] private RedeSocial _rede = RedeSocial.Instagram;
    [ObservableProperty] private TipoPublicacao _tipo = TipoPublicacao.Post;
    [ObservableProperty] private DateTime? _dataPrevista;
    [ObservableProperty] private bool _concluido;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SalvarCommand))]
    private bool _salvando;

    public void Inicializar(TarefaMarketing? tarefa)
    {
        if (tarefa is null)
        {
            _id = Guid.Empty;
            TituloJanela = "Novo lembrete";
            return;
        }

        _id = tarefa.Id;
        TituloJanela = "Editar lembrete";
        Titulo = tarefa.Titulo;
        Descricao = tarefa.Descricao;
        Rede = tarefa.Rede;
        Tipo = tarefa.Tipo;
        DataPrevista = tarefa.DataPrevista;
        Concluido = tarefa.Concluido;
    }

    private bool PodeSalvar() => !string.IsNullOrWhiteSpace(Titulo) && !Salvando;

    [RelayCommand(CanExecute = nameof(PodeSalvar))]
    private async Task SalvarAsync()
    {
        try
        {
            Salvando = true;

            var tarefa = new TarefaMarketing
            {
                Titulo = Titulo.Trim(),
                Descricao = string.IsNullOrWhiteSpace(Descricao) ? null : Descricao.Trim(),
                Rede = Rede,
                Tipo = Tipo,
                DataPrevista = DataPrevista,
                Concluido = Concluido
            };

            if (_id != Guid.Empty)
            {
                tarefa.Id = _id;
            }

            await _marketingService.SalvarAsync(tarefa);
            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Não foi possível salvar o lembrete.\n\nDetalhe: " + ex.Message,
                "Ateliê Hub", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Salvando = false;
        }
    }
}
