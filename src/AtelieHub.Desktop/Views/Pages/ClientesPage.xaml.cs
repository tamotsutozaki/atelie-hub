using System.Windows.Controls;
using System.Windows.Input;
using AtelieHub.Desktop.ViewModels;

namespace AtelieHub.Desktop.Views.Pages;

public partial class ClientesPage : UserControl
{
    public ClientesPage()
    {
        InitializeComponent();
    }

    private void GradeClientes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // Duplo clique numa linha abre a edição do cliente selecionado.
        if (DataContext is ClientesViewModel vm && vm.EditarCommand.CanExecute(null))
        {
            vm.EditarCommand.Execute(null);
        }
    }
}
