using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Raycasting3D.ViewModels;

namespace Raycasting3D.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}