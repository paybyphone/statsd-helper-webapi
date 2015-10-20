using System;
using System.Configuration;

namespace StatsDHelper.WebApi
{
    internal class AppSettings : IAppSettings
    {
        public bool GetBoolean(string key)
        {
            bool value;
            bool.TryParse(ConfigurationManager.AppSettings[key], out value);
            return value;
        } 
    }
}