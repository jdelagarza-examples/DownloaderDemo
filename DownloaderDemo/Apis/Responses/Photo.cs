using Newtonsoft.Json.Linq;

namespace DownloaderDemo.Apis.Responses
{
    public class Photo
    {
        public int ID { get; set; }
        public int AlbumID { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }

        public static Photo FromJson(JObject json)
        {
            var photo = new Photo
            {
                ID = json.ContainsKey("id") && json["id"].Type == JTokenType.Integer ? (int)json["id"] : 0,
                AlbumID = json.ContainsKey("albumId") && json["albumId"].Type == JTokenType.Integer ? (int)json["albumId"] : 0,
                Title = json.ContainsKey("title") && json["title"].Type == JTokenType.String ? (string)json["title"] : null,
                Url = json.ContainsKey("url") && json["url"].Type == JTokenType.String ? (string)json["url"] : null,
                ThumbnailUrl = json.ContainsKey("thumbnailUrl") && json["thumbnailUrl"].Type == JTokenType.String ? (string)json["thumbnailUrl"] : null,
            };
            photo.Url = "https://i.picsum.photos/id/621/300/300.jpg?hmac=c9tWQwXg2lTIEw86xjyzUJ5PVur6xDmLiDJAaTkBe-M";
            return photo;
        }
    }
}
