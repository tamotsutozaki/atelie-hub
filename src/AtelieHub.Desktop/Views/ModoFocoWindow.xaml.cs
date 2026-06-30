using System.Windows;
using AtelieHub.Desktop.ViewModels;

namespace AtelieHub.Desktop.Views;

public partial class ModoFocoWindow : Window
{
    public ModoFocoViewModel ViewModel { get; }

    public ModoFocoWindow(ModoFocoViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;

        viewModel.SaidaSolicitada += () => Close();
    }
}
