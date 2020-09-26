using DownloaderDemo.ViewModels;
using Xamarin.Forms;

namespace DownloaderDemo.Pages
{
    public partial class ItemsViewerPage : ContentPage
    {
        ItemsViewerViewModel _viewModel;

        public ItemsViewerPage()
        {
            InitializeComponent();
        }

        public ItemsViewerPage(ItemsViewerViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel?.DownloadPhotos();
        }
    }
}
