using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace DownloaderDemo.Apis
{
    public class ApiResponse
    {
        public string DevMessage { get; set; } = "";
        public string UsrMessage { get; set; } = "";

        public string ContentDownloaded { get; set; } = "";
        public JToken JsonDownloaded { get; set; } = JObject.Parse("{}");

        public bool IsSuccessStatusCode { get; set; } = false;
        public ApiStatusCode StatusCode { get; set; } = ApiStatusCode.Created;

        public HttpResponseMessage ResponseMessage { get; set; } = null;
        public int HttpStatusCode { get; set; } = 0;
        public string HttpStatusMessage { get; set; } = "";

        public Exception Exception { get; set; } = null;

        public static ApiResponse Failure(string usrMessage, string devMessage, ApiStatusCode statusCode)
        {
            return Failure(usrMessage, devMessage, statusCode, null, null);
        }

        public static ApiResponse Failure(string usrMessage, string devMessage, ApiStatusCode statusCode, Exception exception)
        {
            return Failure(usrMessage, devMessage, statusCode, exception, null);
        }

        public static ApiResponse Failure(string usrMessage, string devMessage, ApiStatusCode statusCode, Exception exception, HttpResponseMessage responseMessage)
        {
            var response = new ApiResponse
            {
                UsrMessage = usrMessage ?? "",
                DevMessage = devMessage ?? "",

                StatusCode = statusCode,
                IsSuccessStatusCode = false,

                Exception = exception,

                ResponseMessage = responseMessage,
                HttpStatusCode = responseMessage != null ? (int)responseMessage.StatusCode : 0,
                HttpStatusMessage = responseMessage != null ? responseMessage.RequestMessage.ToString() : ""
            };
            return response;
        }

        public static ApiResponse Success(string usrMessage, string devMessage, ApiStatusCode statusCode, HttpResponseMessage responseMessage, string contentDownloaded)
        {
            return Success(usrMessage, devMessage, statusCode, responseMessage, contentDownloaded, JToken.Parse("{}"));
        }

        public static ApiResponse Success(string usrMessage, string devMessage, ApiStatusCode statusCode, HttpResponseMessage responseMessage, string contentDownloaded, JToken jsonDownloaded)
        {
            var response = new ApiResponse
            {
                UsrMessage = usrMessage ?? "",
                DevMessage = devMessage ?? "",

                StatusCode = statusCode,
                IsSuccessStatusCode = statusCode == ApiStatusCode.Success,

                ResponseMessage = responseMessage,
                HttpStatusCode = responseMessage != null ? (int)responseMessage.StatusCode : 0,
                HttpStatusMessage = responseMessage != null ? responseMessage.RequestMessage.ToString() : "",

                ContentDownloaded = contentDownloaded ?? "",
                JsonDownloaded = jsonDownloaded ?? JToken.Parse("{}")
            };
            return response;
        }
    }
}
