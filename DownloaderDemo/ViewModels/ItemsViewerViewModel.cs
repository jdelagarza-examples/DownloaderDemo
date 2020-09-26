using System.Collections.Generic;
using DownloaderDemo.Apis.Responses;
using DownloaderDemo.Commons;
using Newtonsoft.Json.Linq;

namespace DownloaderDemo.ViewModels
{
    public class ItemsViewerViewModel : Bindable
    {
        public List<Photo> Photos { get => _photos; set => SetProperty(ref _photos, value); }
        List<Photo> _photos = new List<Photo>();

        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
        bool _isBusy = false;

        public async void DownloadPhotos()
        {
            IsBusy = true;
            var request = Apis.ApiRequest.Create("https://jsonplaceholder.typicode.com/photos", Apis.ApiMethod.Get);
            var response = await Apis.Api.SendAsync(request, true);
            IsBusy = false;
            if (!response.IsSuccessStatusCode)
                return;
            if (response.JsonDownloaded.Type != JTokenType.Array)
                return;
            var photos = new List<Photo>();
            foreach (var item in response.JsonDownloaded as JArray)
            {
                if (item != null && item.Type == JTokenType.Object)
                    photos.Add(Photo.FromJson(item as JObject));
            }
            Photos = new List<Photo>(photos);
        }
    }
}
