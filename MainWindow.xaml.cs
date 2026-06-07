using System.Windows;
using MasaHotelPro.ViewModels;

namespace MasaHotelPro;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}