namespace DownloaderDemo.Apis
{
    public enum ApiStatusCode
    {
        Unknown,
        Created,
        Success,
        InvalidUrl,
        ExpiredTime,
        NoInternetConnection,
        NullContent,
        HttpStatusCodeNotSuccess,
        UnreadContent,
        InvalidRequest,
        ConnectionFailure,
        ReadFromCache,
        UserCanceled
    }
}
