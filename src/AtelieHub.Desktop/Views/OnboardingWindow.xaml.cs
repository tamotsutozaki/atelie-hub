using System.Windows;
using AtelieHub.Desktop.ViewModels;

namespace AtelieHub.Desktop.Views;

public partial class OnboardingWindow : Window
{
    public OnboardingWindow(OnboardingViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.Concluido += () =>
        {
            DialogResult = true; // fecha o diálogo sinalizando sucesso
        };
    }
}
