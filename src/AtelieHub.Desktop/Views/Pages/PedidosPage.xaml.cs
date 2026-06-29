using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AtelieHub.Desktop.ViewModels;

namespace AtelieHub.Desktop.Views.Pages;

public partial class PedidosPage : UserControl
{
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
}
