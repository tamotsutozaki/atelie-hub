using System.Windows;
using AtelieHub.Desktop.ViewModels;

namespace AtelieHub.Desktop.Views;

public partial class LancamentoEdicaoWindow : Window
{
    public LancamentoEdicaoViewModel ViewModel { get; }

    public LancamentoEdicaoWindow(LancamentoEdicaoViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;

        viewModel.Salvo += () => DialogResult = true;
    }
}
