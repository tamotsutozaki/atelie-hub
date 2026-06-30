using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AtelieHub.Core.Entities;
using AtelieHub.Desktop.ViewModels;

namespace AtelieHub.Desktop.Views.Pages;

public partial class PedidosPage : UserControl
{
    private Point _pontoInicioArraste;
    private Pedido? _pedidoArraste;

    public PedidosPage()
    {
        InitializeComponent();
    }

    private void GradePedidos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var origem = e.OriginalSource as DependencyObject;
        while (origem is not null and not DataGridRow)
        {
            origem = VisualTreeHelper.GetParent(origem);
        }

        if (origem is DataGridRow && DataContext is PedidosViewModel vm && vm.EditarCommand.CanExecute(null))
        {
            vm.EditarCommand.Execute(null);
        }
    }

    // ===== Kanban: seleção, duplo-clique e arrastar-e-soltar =====

    private void Card_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement card || card.DataContext is not Pedido pedido)
        {
            return;
        }

        if (DataContext is not PedidosViewModel vm)
        {
            return;
        }

        vm.PedidoSelecionado = pedido;
        _pontoInicioArraste = e.GetPosition(null);
        _pedidoArraste = pedido;

        // Duplo-clique abre a edição (mesma ação do duplo-clique na lista).
        if (e.ClickCount == 2 && vm.EditarCommand.CanExecute(null))
        {
            _pedidoArraste = null;
            vm.EditarCommand.Execute(null);
        }
    }

    private void Card_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _pedidoArraste is null)
        {
            return;
        }

        var pos = e.GetPosition(null);
        if (Math.Abs(pos.X - _pontoInicioArraste.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(pos.Y - _pontoInicioArraste.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        var dados = new DataObject(typeof(Pedido), _pedidoArraste);
        _pedidoArraste = null;
        DragDrop.DoDragDrop((DependencyObject)sender, dados, DragDropEffects.Move);
    }

    private void Coluna_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(Pedido)) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private async void Coluna_Drop(object sender, DragEventArgs e)
    {
        if (sender is not FrameworkElement alvo || alvo.DataContext is not ColunaKanban coluna)
        {
            return;
        }

        if (DataContext is not PedidosViewModel vm || !e.Data.GetDataPresent(typeof(Pedido)))
        {
            return;
        }

        if (e.Data.GetData(typeof(Pedido)) is Pedido pedido)
        {
            await vm.MoverParaStatusAsync(pedido, coluna.Status);
        }
    }
}
