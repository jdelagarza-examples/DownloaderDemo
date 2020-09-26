using DownloaderDemo.Pages;
using DownloaderDemo.ViewModels;
using Xamarin.Forms;

namespace DownloaderDemo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new ItemsViewerPage(new ItemsViewerViewModel());
        }
    }
}
