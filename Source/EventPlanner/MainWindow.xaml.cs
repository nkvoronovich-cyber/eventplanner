using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using EventPlanner.ViewModels;

namespace EventPlanner;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        TryLoadLogo();
    }

    private void TryLoadLogo()
    {
        // Try to load logo.png from the application folder. Fail silently if it isn't there.
        try
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
            if (!File.Exists(path)) return;
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(path);
            image.EndInit();
            LogoImage.Source = image;
            LogoImage.Visibility = Visibility.Visible;
        }
        catch
        {
            // Logo is optional; if it can't be loaded we just keep the text-only header.
        }
    }
}
