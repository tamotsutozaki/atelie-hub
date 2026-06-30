using System.Collections.ObjectModel;
using AtelieHub.Core.Entities;
using AtelieHub.Core.Enums;

namespace AtelieHub.Desktop.ViewModels;

/// <summary>Uma coluna do quadro Kanban de pedidos: um status de produção e os pedidos nele.</summary>
public class ColunaKanban
{
    public ColunaKanban(StatusPedido status, string titulo)
    {
        Status = status;
        Titulo = titulo;
    }

    public StatusPedido Status { get; }

    public string Titulo { get; }

    public ObservableCollection<Pedido> Itens { get; } = new();
}
