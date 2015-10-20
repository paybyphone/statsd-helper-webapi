using System;
using System.Configuration;

namespace StatsDHelper.WebApi
{
    public class AppSettings
    {
        public static bool GetBoolean(string key)
        {
            bool value;
            bool.TryParse(ConfigurationManager.AppSettings[key], out value);
            return value;
        } 
    }
}