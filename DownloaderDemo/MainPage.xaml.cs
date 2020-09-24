using System.ComponentModel;
using System.Diagnostics;
using DownloaderDemo.Apis;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace DownloaderDemo
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async void DownloadWithAwaitAndAsync(string url)
        {
            loadingLabel.Text = "Cargando información";
            loadingIndicator.IsRunning = true;
            var response = await Api.SendAsync(ApiRequest.Create(url, ApiMethod.Get), true);
            loadingIndicator.IsRunning = false;
            loadingLabel.FontAttributes = FontAttributes.Bold;
            loadingLabel.TextColor = Color.White;
            if (!response.IsSuccessStatusCode)
            {
                loadingLabel.Text = response.DevMessage;
                return;
            }
            var json = response.JsonDownloaded;
            if (json.Type != JTokenType.Array)
            {
                loadingLabel.Text = "La estructura de la información cargada no es correcta";
                return;
            }
            loadingLabel.Text = $"{(json as JArray).Count} fotos descargadas";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //https://jsonplaceholder.typicode.com/photos
            DownloadWithAwaitAndAsync("http://192.168.100.9/testing/photos.php");
        }
    }
}
