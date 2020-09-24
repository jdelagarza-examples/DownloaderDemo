using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Collections;

namespace DownloaderDemo.Apis
{
    public static class ApiRequestExtensions
    {
        public static ApiRequest CreateApiRequest(this string _, ApiMethod method = ApiMethod.Get)
        {
            return ApiRequest.Create(_, method);
        }

        public static ApiRequest WithBody(this ApiRequest _, Dictionary<string, object> body)
        {
            if (_ == null)
                return new ApiRequest();
            if (body != null && body.Count > 0)
                _.Body = ConvertStringToStringContent(ConvertDictionaryToString(body));
            return _;
        }

        public static ApiRequest WithBody(this ApiRequest _, List<Dictionary<string, object>> body)
        {
            if (_ == null)
                return new ApiRequest();
            if (body != null && body.Count > 0)
                _.Body = ConvertStringToStringContent(ConvertListToString(body));
            return _;
        }

        public static ApiRequest WithHeaders(this ApiRequest _, Dictionary<string, string> headers)
        {
            if (_ == null)
                return new ApiRequest();
            if (headers != null && headers.Count > 0)
                _.Headers = headers;
            return _;
        }

        public static ApiRequest WithMethod(this ApiRequest _, ApiMethod method)
        {
            if (_ == null)
                return new ApiRequest();
            _.Method = method;
            return _;
        }

        static StringContent ConvertStringToStringContent(string arg, string mediaType = "application/json")
        {
            if (string.IsNullOrEmpty(arg) || string.IsNullOrWhiteSpace(arg))
                return default;
            var result = default(StringContent);
            try
            {
                result = new StringContent(arg, Encoding.UTF8, mediaType);
            }
            catch (Exception)
            {
            }
            return result;
        }

        static string ConvertDictionaryToString(IDictionary arg)
        {
            var result = "";
            try
            {
                result = JsonConvert.SerializeObject(arg, Formatting.Indented);
            }
            catch (Exception)
            {
            }
            return result;
        }

        static string ConvertListToString(IList arg)
        {
            var result = "";
            try
            {
                result = JsonConvert.SerializeObject(arg, Formatting.Indented);
            }
            catch (Exception)
            {
            }
            return result;
        }
    }
}
