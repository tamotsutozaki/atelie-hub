using System.Windows;
using AtelieHub.Desktop.ViewModels;

namespace AtelieHub.Desktop.Views;

public partial class PedidoEdicaoWindow : Window
{
    public PedidoEdicaoViewModel ViewModel { get; }

    public PedidoEdicaoWindow(PedidoEdicaoViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;

        viewModel.Salvo += () => DialogResult = true;
    }
}
