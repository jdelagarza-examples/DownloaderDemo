using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        async void DownloadJsonOne(string url)
        {
            HttpRequest request = url.CreateHttpRequest(HttpRequestMethod.Get);
            HttpResponse response = await HttpResources.Current.SendAsync(request);
            _ = 0;
        }

        async void DownloadJsonTwo(string url)
        {
            HttpRequest request = HttpRequest.Create(url, HttpRequestMethod.Get);
            HttpResponse response = await HttpResources.Current.SendAsync(request);
            _ = 0;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}
