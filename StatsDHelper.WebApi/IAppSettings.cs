namespace StatsDHelper.WebApi
{
    internal interface IAppSettings
    {
        bool GetBoolean(string key);
    }
}