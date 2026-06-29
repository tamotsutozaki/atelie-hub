using System.Windows;
using AtelieHub.Desktop.ViewModels;

namespace AtelieHub.Desktop.Views;

public partial class ProdutoEstoqueEdicaoWindow : Window
{
    public ProdutoEstoqueEdicaoViewModel ViewModel { get; }

    public ProdutoEstoqueEdicaoWindow(ProdutoEstoqueEdicaoViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;

        viewModel.Salvo += () => DialogResult = true;
    }
}
