using System.Windows;
using AtelieHub.Desktop.ViewModels;

namespace AtelieHub.Desktop.Views;

public partial class TarefaMarketingEdicaoWindow : Window
{
    public TarefaMarketingEdicaoViewModel ViewModel { get; }

    public TarefaMarketingEdicaoWindow(TarefaMarketingEdicaoViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;

        viewModel.Salvo += () => DialogResult = true;
    }
}
