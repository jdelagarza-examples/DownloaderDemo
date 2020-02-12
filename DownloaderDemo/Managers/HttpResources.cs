using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;

namespace DownloaderDemo
{
    public class HttpResources
    {
        IDictionary<HttpRequestMessage, TimeSpan> _downloadTimes;
        IDictionary<HttpRequestMessage, DateTime> _startingTimes;
        IDictionary<HttpRequestMessage, DateTime> _finishingTimes;

        public async Task<HttpResponse> SendAsync(HttpRequest request, Action starting = null)
        {
            if (request == null)
            {
                return HttpResponse.CreateResponse("Petición no válida", "La petición para el servicio web recibida es nula", HttpResponseStatusCode.NullRequest);
            }
            return await SendAsync(request.URL, request.Body, request.Headers, request.Method, request.Timeout, starting);
        }

        async Task<HttpResponse> SendAsync(string url, IDictionary<string, object> body, IDictionary<string, string> headers, HttpRequestMethod method, double timeout, Action starting)
        {
            PrintRequestInformation(url, body, headers, method, timeout);
            if (string.IsNullOrEmpty(url) || string.IsNullOrWhiteSpace(url))
            {
                return HttpResponse.CreateResponse("La dirección de la petición no es válida", "La dirección URL del servicio web no es válida porque es nula o está vacía", HttpResponseStatusCode.URLNotValid);
            }
            if (!HasInternetConnection())
            {
                return HttpResponse.CreateResponse("No se ha detectado una conexión a internet", "El dispositivo no está conectado a una señal de internet", HttpResponseStatusCode.NoInternetConnection);
            }
            HttpClient client = GetClient(timeout);
            try
            {
                HttpRequestMessage requestMessage = new HttpRequestMessage();
                requestMessage.RequestUri = new Uri(url);
                requestMessage.Method = method == HttpRequestMethod.Get ? HttpMethod.Get :
                    method == HttpRequestMethod.Post ? HttpMethod.Post :
                    method == HttpRequestMethod.Put ? HttpMethod.Put : HttpMethod.Delete;
                if (body != null && body.Count > 0)
                {
                    string bodyString = JsonConvert.SerializeObject(body, Formatting.Indented);
                    requestMessage.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
                }
                else
                {
                    if (method == HttpRequestMethod.Post)
                    {
                        string bodyString = JsonConvert.SerializeObject(new Dictionary<string, object> { ["fakeKey"] = $"fakeValue" }, Formatting.Indented);
                        requestMessage.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
                    }
                }
                if (headers != null && headers.Count > 0)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        requestMessage.Headers.Add(header.Key, header.Value);
                    }
                }
                starting?.Invoke();

                _startingTimes.Add(requestMessage, DateTime.Now);
                HttpResponseMessage responseMessage = await client.SendAsync(requestMessage);
                _finishingTimes.Add(requestMessage, DateTime.Now);
                RegisterTimesForRequest(requestMessage);

                if (responseMessage.IsSuccessStatusCode)
                {
                    JToken json = JToken.Parse("{}");
                    string contentDownloaded = await responseMessage.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(contentDownloaded) && contentDownloaded.ToLower() != "null")
                    {
                        try
                        {
                            json = JToken.Parse(contentDownloaded);
                        }
                        catch (Exception)
                        {
                            json = JToken.Parse("{}");
                        }

                        if (json.Type != JTokenType.Object && json.Type != JTokenType.Array && !json.HasValues)
                        {
                            json = JObject.Parse("{}");
                            (json as JObject).Add("data", contentDownloaded);
                        }
                        return HttpResponse.CreateResponse("Petición realizada correctamente",
                            "La petición se realizó de manera correcta", responseMessage, requestMessage, contentDownloaded, json);
                    }
                    else
                    {
                        HttpResponse httpResponse = HttpResponse.CreateResponse("No fue posible obtener la información solicitada",
                            "El contenido de la descarga es nulo", HttpResponseStatusCode.NullContent, requestMessage, responseMessage);
                        httpResponse.ContentDownloaded = contentDownloaded;
                        return httpResponse;
                    }
                }
                else
                {
                    return HttpResponse.CreateResponse("Ocurrió un problema al solicitar la información",
                        "La petición no se realizó correctamente", HttpResponseStatusCode.HttpStatusCode, requestMessage, responseMessage);
                }
            }
            catch (TaskCanceledException ex)
            {
                return HttpResponse.CreateResponse("La descarga de la información demoró mas de lo permitido",
                    "La descarga duró mas del tiempo permitido", HttpResponseStatusCode.Timeout, ex);
            }
            catch (OperationCanceledException ex)
            {
                return HttpResponse.CreateResponse("La descarga de la información demoró mas de lo permitido",
                    "La descarga duró mas del tiempo permitido", HttpResponseStatusCode.Timeout, ex);
            }
            catch (WebException ex)
            {
                return HttpResponse.CreateResponse("No fue posible establecer una conexión con la dirección IP de la petición",
                    "No se logró establecer una conexión con la IP del servicio web, puede que haya esté todo bien, pero que la conexión a internet esté fallando", HttpResponseStatusCode.ConnectionFailure, ex);
            }
            catch (HttpRequestException ex)
            {
                return HttpResponse.CreateResponse("No fue posible establecer una conexión con la dirección IP de la petición",
                    "No se logró establecer una conexión con la IP del servicio web, puede que haya esté todo bien, pero que la conexión a internet esté fallando", HttpResponseStatusCode.ConnectionFailure, ex);
            }
            catch (UriFormatException ex)
            {
                return HttpResponse.CreateResponse("La dirección de la petición no tiene un formato válido",
                    "La url no tiene un formato válido y el objeto Uri lo rechazó", HttpResponseStatusCode.URLNotValid, ex);
            }
            catch (Exception ex)
            {
                return HttpResponse.CreateResponse("Ocurrió un error al intentar realizar la descarga de información de la petición",
                    "Ocurrió un error controlado, pero sin información detallada", HttpResponseStatusCode.Unknown, ex);
            }
        }

        bool HasInternetConnection()
        {
            NetworkAccess currentNetwork = Connectivity.NetworkAccess;
            if (currentNetwork == NetworkAccess.Internet)
            {
                return true;
            }
            return false;
        }

        HttpClient GetClient(double timeout)
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(timeout >= 3 ? timeout : 15.0);
            return client;
        }

        void RegisterTimesForRequest(HttpRequestMessage request)
        {
            if (_startingTimes.ContainsKey(request) && _finishingTimes.ContainsKey(request))
            {
                DateTime startingTime = _startingTimes[request];
                DateTime finishingTime = _finishingTimes[request];
                TimeSpan totalSeconds = finishingTime - startingTime;
                _downloadTimes.Add(request, totalSeconds);

                string separator = "";
                for (int i = 0; i < $"Tiempo descarga de {request.RequestUri.OriginalString}".Length; i++)
                {
                    separator += "*";
                }
                Print($"" +
                    $"{separator}\n" +
                    $"Tiempo descarga de {request.RequestUri.OriginalString}\n" +
                    $"{string.Format("{0:0.0}", totalSeconds.TotalSeconds)} segundos\n" +
                    $"{separator}");

                _startingTimes.Clear();
                _finishingTimes.Clear();
                _downloadTimes.Clear();
            }
        }

        void PrintRequestInformation(string url, IDictionary<string, object> body, IDictionary<string, string> headers, HttpRequestMethod method, double timeout)
        {
            string httpURL = !string.IsNullOrEmpty(url) && !string.IsNullOrWhiteSpace(url) ? url : "N/A";
            string httpBody = body != null && body.Count > 0 ? JsonConvert.SerializeObject(body, Formatting.Indented) : "{\n}";
            string httpHeaders = headers != null && headers.Count > 0 ? JsonConvert.SerializeObject(headers, Formatting.Indented) : "{\n}";
            string httpMethod = Enum.GetName(typeof(HttpRequestMethod), method);

            string separator = "";
            if (httpURL.ToLower() == "n/a")
            {
                separator = "********************";
            }
            else
            {
                for (int i = 0; i < httpURL.Length; i++)
                {
                    separator += "*";
                }
                for (int i = 0; i < 5; i++)
                {
                    separator += "*";
                }
            }

            Print("" +
                $"{separator}\n" +
                $"url: {httpURL}\n" +
                $"body:\n{httpBody}\n" +
                $"headers:\n{httpHeaders}\n" +
                $"method: {httpMethod.ToLower()}\n" +
                $"timeout: {timeout} segundos.\n" +
                $"{separator}");
        }

        void Print(string message)
        {
            System.Diagnostics.Debug.WriteLine($"{message}");
        }

        static HttpResources current;
        protected HttpResources() { }
        public static HttpResources Current
        {
            get
            {
                if (current == null)
                {
                    current = new HttpResources();
                    current._downloadTimes = new Dictionary<HttpRequestMessage, TimeSpan>();
                    current._startingTimes = new Dictionary<HttpRequestMessage, DateTime>();
                    current._finishingTimes = new Dictionary<HttpRequestMessage, DateTime>();
                }
                return current;
            }
        }
    }

    public class HttpRequest
    {
        public string URL { get; set; } = "";
        public IDictionary<string, object> Body { get; set; } = new Dictionary<string, object>();
        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public HttpRequestMethod Method { get; set; } = HttpRequestMethod.Get;
        public double Timeout { get; set; } = 15.0;

        public static HttpRequest Create(string url, HttpRequestMethod method)
        {
            return Create(url, null, null, method);
        }

        public static HttpRequest Create(string url, IDictionary<string, object> body, HttpRequestMethod method)
        {
            return Create(url, body, null, method);
        }

        public static HttpRequest Create(string url, IDictionary<string, object> body, IDictionary<string, string> headers, HttpRequestMethod method)
        {
            HttpRequest httpRequest = new HttpRequest();
            httpRequest.URL = url ?? "";
            httpRequest.Body = body ?? new Dictionary<string, object>();
            httpRequest.Headers = headers ?? new Dictionary<string, string>();
            httpRequest.Method = method;
            return httpRequest;
        }
    }

    public enum HttpRequestMethod
    {
        Get,
        Post,
        Put,
        Delete
    }

    public static class HttpRequestExtensions
    {
        public static HttpRequest CreateHttpRequest(this string self, HttpRequestMethod method = HttpRequestMethod.Get)
        {
            if (string.IsNullOrEmpty(self) || string.IsNullOrWhiteSpace(self))
            {
                return HttpRequest.Create("", HttpRequestMethod.Get);
            }
            return HttpRequest.Create(self, method);
        }

        public static HttpRequest WithBody(this HttpRequest request, IDictionary<string, object> body)
        {
            if (request == null)
            {
                return HttpRequest.Create("", HttpRequestMethod.Get);
            }
            return HttpRequest.Create(request.URL, body, request.Headers, request.Method);
        }

        public static HttpRequest WithHeaders(this HttpRequest request, IDictionary<string, string> headers)
        {
            if (request == null)
            {
                return HttpRequest.Create("", HttpRequestMethod.Get);
            }
            return HttpRequest.Create(request.URL, request.Body, headers, request.Method);
        }

        public static HttpRequest WithMethod(this HttpRequest request, HttpRequestMethod method)
        {
            if (request == null)
            {
                return HttpRequest.Create("", HttpRequestMethod.Get);
            }
            return HttpRequest.Create(request.URL, request.Body, request.Headers, method);
        }

        public static HttpRequest WithTimeout(this HttpRequest request, double timeout)
        {
            if (request == null)
            {
                return HttpRequest.Create("", HttpRequestMethod.Get);
            }
            HttpRequest httpRequest = HttpRequest.Create(request.URL, request.Body, request.Headers, request.Method);
            httpRequest.Timeout = timeout;
            return httpRequest;
        }
    }

    public class HttpResponse
    {
        public string Message { get; set; } = "";
        public string Reason { get; set; } = "";
        public bool IsSuccessStatusCode { get; set; } = false;

        public HttpResponseMessage Response { get; set; } = null;
        public HttpRequestMessage Request { get; set; } = null;

        public int HttpStatusCode { get; set; } = 0;
        public string HttpMessage { get; set; } = "";

        public Exception Exception { get; set; } = null;
        public Exception InnerException { get; private set; } = null;
        public HttpResponseStatusCode StatusCode { get; set; } = HttpResponseStatusCode.Initialize;

        public string ContentDownloaded { get; set; } = "";
        public JToken Json { get; set; } = JObject.Parse("{}");

        public static HttpResponse CreateResponse(string message, string reason, HttpResponseMessage response, HttpRequestMessage request, string content, JToken json)
        {
            HttpResponse httpResponse = new HttpResponse();
            httpResponse.Message = message ?? "N/A";
            httpResponse.Reason = reason ?? "N/A";
            httpResponse.IsSuccessStatusCode = true;
            httpResponse.Response = response;
            httpResponse.Request = request;
            httpResponse.HttpStatusCode = response != null ? (int)response.StatusCode : 0;
            httpResponse.HttpMessage = response?.ReasonPhrase ?? "N/A";
            httpResponse.ContentDownloaded = content ?? "N/A";
            httpResponse.Json = json ?? JObject.Parse("{}");
            httpResponse.StatusCode = HttpResponseStatusCode.Success;
            return httpResponse;
        }

        public static HttpResponse CreateResponse(string message, string reason, HttpResponseStatusCode statusCode)
        {
            return CreateResponse(message, reason, statusCode, null);
        }

        public static HttpResponse CreateResponse(string message, string reason, HttpResponseStatusCode statusCode, Exception exception)
        {
            HttpResponse httpResponse = new HttpResponse();
            httpResponse.Message = message ?? "N/A";
            httpResponse.Reason = reason ?? "N/A";
            httpResponse.StatusCode = statusCode;
            httpResponse.Exception = exception;
            httpResponse.InnerException = exception?.InnerException ?? null;
            return httpResponse;
        }

        public static HttpResponse CreateResponse(string message, string reason, HttpResponseStatusCode statusCode, HttpRequestMessage request, HttpResponseMessage response)
        {
            HttpResponse httpResponse = new HttpResponse();
            httpResponse.Message = message;
            httpResponse.Reason = reason;
            httpResponse.StatusCode = statusCode;
            httpResponse.Request = request;
            httpResponse.Response = response;
            httpResponse.HttpStatusCode = (int)response.StatusCode;
            httpResponse.HttpMessage = Enum.GetName(typeof(HttpStatusCode), response.StatusCode);
            return httpResponse;
        }
    }

    public enum HttpResponseStatusCode
    {
        Initialize = 1101,
        Success = 1102,
        URLNotValid = 1103,
        NoInternetConnection = 1104,
        ConnectionFailure = 1105,
        NullRequest = 1106,
        Unknown = 1107,
        Timeout = 1108,
        HttpStatusCode = 1109,
        NullContent = 1110
    }
}
