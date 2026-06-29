using System.Windows;
using AtelieHub.Desktop.ViewModels;

namespace AtelieHub.Desktop.Views;

public partial class ClienteEdicaoWindow : Window
{
    public ClienteEdicaoViewModel ViewModel { get; }

    public ClienteEdicaoWindow(ClienteEdicaoViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;

        viewModel.Salvo += () => DialogResult = true; // fecha o diálogo sinalizando sucesso
    }
}
