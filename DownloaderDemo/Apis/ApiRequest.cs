using System;
using System.Collections.Generic;
using System.Net.Http;

namespace DownloaderDemo.Apis
{
    public class ApiRequest
    {
        public string Url { get; set; } = "";
        public HttpContent Body { get; set; } = null;
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public ApiMethod Method { get; set; } = ApiMethod.Get;

        public static ApiRequest Create(string url, ApiMethod method)
        {
            return Create(url, null, new Dictionary<string, string>(), method);
        }

        static ApiRequest Create(string url, HttpContent body, Dictionary<string, string> headers, ApiMethod method)
        {
            var request = new ApiRequest
            {
                Url = url ?? "",
                Body = body,
                Headers = headers ?? new Dictionary<string, string>(),
                Method = method,
            };
            return request;
        }
    }
}
