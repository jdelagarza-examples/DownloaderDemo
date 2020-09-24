using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Essentials;

namespace DownloaderDemo.Apis
{
    public class Api
    {
        public static Dictionary<ApiRequest, DateTime> InitialTimes { get; private set; } = new Dictionary<ApiRequest, DateTime>();
        public static Dictionary<ApiRequest, DateTime> FinalTimes { get; private set; } = new Dictionary<ApiRequest, DateTime>();
        public static Dictionary<ApiRequest, TimeSpan> DurationTimes { get; private set; } = new Dictionary<ApiRequest, TimeSpan>();

        public static Task<ApiResponse> SendAsync(ApiRequest request, bool isForced)
        {
            return SendAsync(request, isForced, CancellationToken.None);
        }

        public static Task<ApiResponse> SendAsync(ApiRequest request, bool isForced, CancellationToken cancellationToken)
        {
            return SendAsync(request.Url, request.Body, request.Headers, ConvertApiMethodToHttpMethod(request.Method), cancellationToken);
        }

        static async Task<ApiResponse> SendAsync(string url, HttpContent body, Dictionary<string, string> headers, HttpMethod method, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
                return ApiResponse.Failure(Messages.ForUsers.InvalidUrl, Messages.ForDevs.InvalidUrl, ApiStatusCode.InvalidUrl);
            if (!HasInternetConnection())
                return ApiResponse.Failure(Messages.ForUsers.HasNotInternetConnection, Messages.ForDevs.HasNotInternetConnection, ApiStatusCode.NoInternetConnection);
            try
            {
                var requestMessage = new HttpRequestMessage(method, url)
                {
                    Content = body
                };
                if (headers != null)
                    foreach (KeyValuePair<string, string> header in headers)
                        requestMessage.Headers.Add(header.Key, header.Value);
                var client = GetHttpClientConfigured();
                var responseMessage = await client.SendAsync(requestMessage, cancellationToken);
                if (!responseMessage.IsSuccessStatusCode)
                    return ApiResponse.Failure(Messages.ForUsers.ResponseFailureStatusCode, Messages.ForDevs.ResponseFailureStatusCode, ApiStatusCode.HttpStatusCodeNotSuccess, null, responseMessage);
                var contentDownloaded = await responseMessage.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(contentDownloaded) || contentDownloaded.ToLower() == "null")
                    return ApiResponse.Failure(Messages.ForUsers.NullContent, Messages.ForDevs.NullContent, ApiStatusCode.NullContent, null, responseMessage);
                var jsonDownloaded = default(JToken);
                try
                {
                    jsonDownloaded = JToken.Parse(contentDownloaded);
                }
                catch (Exception)
                {
                    jsonDownloaded = JToken.Parse("{}");
                }
                if (jsonDownloaded.Type != JTokenType.Object && jsonDownloaded.Type != JTokenType.Array && !jsonDownloaded.HasValues)
                {
                    jsonDownloaded = JObject.Parse("{}");
                    (jsonDownloaded as JObject).Add("data", contentDownloaded);
                }
                return ApiResponse.Success(Messages.ForUsers.Success, Messages.ForDevs.Success, ApiStatusCode.Success, responseMessage, contentDownloaded, jsonDownloaded);
            }
            catch (TaskCanceledException ex)
            {
                return ApiResponse.Failure(Messages.ForUsers.TaskCanceled, Messages.ForDevs.TaskCanceled, ApiStatusCode.ExpiredTime, ex);
            }
            catch (OperationCanceledException ex)
            {
                return ApiResponse.Failure(Messages.ForUsers.OperationCanceledException, Messages.ForDevs.OperationCanceledException, ApiStatusCode.ExpiredTime, ex);
            }
            catch (WebException ex)
            {
                return ApiResponse.Failure(Messages.ForUsers.WebException, Messages.ForDevs.WebException, ApiStatusCode.ConnectionFailure, ex);
            }
            catch (HttpRequestException ex)
            {
                return ApiResponse.Failure(Messages.ForUsers.HttpRequestException, Messages.ForDevs.HttpRequestException, ApiStatusCode.ConnectionFailure, ex);
            }
            catch (UriFormatException ex)
            {
                return ApiResponse.Failure(Messages.ForUsers.UriFormatException, Messages.ForDevs.UriFormatException, ApiStatusCode.InvalidUrl, ex);
            }
            catch (JsonReaderException ex)
            {
                return ApiResponse.Failure(Messages.ForUsers.JsonReaderException, Messages.ForDevs.JsonReaderException, ApiStatusCode.UnreadContent, ex);
            }
            catch (Exception ex)
            {
                return ApiResponse.Failure(Messages.ForUsers.Exception, Messages.ForDevs.Exception, ApiStatusCode.Unknown, ex);
            }
        }

        static bool HasInternetConnection()
        {
            var currentNetwork = Connectivity.NetworkAccess;
            return currentNetwork == NetworkAccess.Internet;
        }

        static HttpClient GetHttpClientConfigured()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10.0)
            };
            return client;
        }

        static HttpMethod ConvertApiMethodToHttpMethod(ApiMethod method)
        {
            return method == ApiMethod.Delete ? HttpMethod.Delete :
                   method == ApiMethod.Get ? HttpMethod.Get :
                   method == ApiMethod.Post ? HttpMethod.Post :
                   method == ApiMethod.Put ? HttpMethod.Put : HttpMethod.Get;
        }

        struct Messages
        {
            public struct ForUsers
            {
                public const string InvalidUrl = "La dirección de la petición no es válida";
                public const string ResponseFailureStatusCode = "Ocurrió un problema al cargar la información";
                public const string NullContent = "Sin contenido";
                public const string Success = "Carga de datos exitosa";
                public const string HasNotInternetConnection = "El dispositivo no tiene una conexión a internet";

                public const string TaskCanceled = "";
                public const string OperationCanceledException = "";
                public const string WebException = "";
                public const string HttpRequestException = "";
                public const string UriFormatException = "";
                public const string JsonReaderException = "";
                public const string Exception = "";
            }

            public struct ForDevs
            {
                public const string InvalidUrl = "La dirección url del servicio web no es válida";
                public const string ResponseFailureStatusCode = "La propiedad HttpStatusCode de la llamada al servicio web no es correcta";
                public const string NullContent = "El contenido cargado del servicio web está vacío";
                public const string Success = "Carga de datos exitosa";
                public const string HasNotInternetConnection = "El dispositivo no tiene una conexión a internet";

                public const string TaskCanceled = "";
                public const string OperationCanceledException = "";
                public const string WebException = "";
                public const string HttpRequestException = "";
                public const string UriFormatException = "";
                public const string JsonReaderException = "";
                public const string Exception = "";
            }
        }
    }
}
