using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AtelieHub.Desktop.ViewModels;

namespace AtelieHub.Desktop.Views;

public partial class EtiquetaEnvioWindow : Window
{
    public EtiquetaEnvioViewModel ViewModel { get; }

    public EtiquetaEnvioWindow(EtiquetaEnvioViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;

        viewModel.ImpressaoSolicitada += Imprimir;
    }

    private void Imprimir()
    {
        var dialog = new PrintDialog();
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var pagina = ConstruirPagina(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight);
        dialog.PrintVisual(pagina, ViewModel.TituloJanela);
    }

    /// <summary>
    /// Monta o visual da etiqueta dimensionado à área imprimível da impressora escolhida.
    /// É construído à parte do preview da tela para imprimir em preto sobre branco, com fontes grandes.
    /// </summary>
    private FrameworkElement ConstruirPagina(double largura, double altura)
    {
        const double margem = 56; // ~1,5 cm a 96 DPI

        var conteudo = new StackPanel { Margin = new Thickness(margem) };
        conteudo.Children.Add(CriarBloco(
            "DESTINATÁRIO",
            ViewModel.DestinatarioNome,
            ViewModel.DestinatarioEndereco,
            ViewModel.DestinatarioContato,
            nomeTamanho: 26, corpoTamanho: 16, espesso: true));
        conteudo.Children.Add(CriarBloco(
            "REMETENTE",
            ViewModel.RemetenteNome,
            ViewModel.RemetenteEndereco,
            ViewModel.RemetenteContato,
            nomeTamanho: 16, corpoTamanho: 12, espesso: false));

        var pagina = new Grid
        {
            Width = largura,
            Height = altura,
            Background = Brushes.White,
        };
        pagina.Children.Add(conteudo);

        var tamanho = new Size(largura, altura);
        pagina.Measure(tamanho);
        pagina.Arrange(new Rect(tamanho));
        pagina.UpdateLayout();
        return pagina;
    }

    private static Border CriarBloco(string rotulo, string nome, string endereco, string? contato,
        double nomeTamanho, double corpoTamanho, bool espesso)
    {
        var painel = new StackPanel();
        painel.Children.Add(new TextBlock
        {
            Text = rotulo,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 0, 0, 6),
        });
        painel.Children.Add(new TextBlock
        {
            Text = nome,
            FontSize = nomeTamanho,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.Black,
            TextWrapping = TextWrapping.Wrap,
        });

        if (!string.IsNullOrWhiteSpace(endereco))
        {
            painel.Children.Add(new TextBlock
            {
                Text = endereco,
                FontSize = corpoTamanho,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 8, 0, 0),
            });
        }

        if (!string.IsNullOrWhiteSpace(contato))
        {
            painel.Children.Add(new TextBlock
            {
                Text = contato,
                FontSize = corpoTamanho,
                Foreground = Brushes.Black,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0),
            });
        }

        return new Border
        {
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(espesso ? 2 : 1),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 0, 28),
            Child = painel,
        };
    }
}
