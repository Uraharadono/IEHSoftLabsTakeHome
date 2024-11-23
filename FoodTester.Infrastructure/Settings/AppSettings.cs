using System;
using Microsoft.Extensions.Configuration;
using FoodTester.Utility.Exceptions;
using FoodTester.Utility.Extensions;

namespace FoodTester.Infrastructure.Settings
{
    public interface IAppSettings : IClientAppSettings, ILoggerConfiguration
    {
        bool IsDebug { get; }
        string BaseFolder { get; }
        string AdminEmail { get; }
    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration _config;
        
        public AppSettings(IConfiguration config)
        {
            _config = config;
        }

        public bool IsDebug => ReadBoolean("IsDebug");
        public string BaseFolder => ReadString("BaseFolder");
        public string AdminEmail => ReadString("AdminEmail");

        // Client App Settings
        public string ClientBaseUrl => ReadString("ClientBaseUrl");

        // Logger configuration
        public string LogsFolder => ReadString("LogsFolder");

        // Utility functions
        private string ReadString(string key)
        {
            var settings = _config.GetSection("AppSettings");
            return settings[key];
        }

        private int ReadInt(string key)
        {
            return ReadString(key).ParseInt();
        }

        private bool ReadBoolean(string key)
        {
            var val = ReadString(key);
            return val != null && bool.Parse(val);
        }

        private long ReadLong(string key)
        {
            return Convert.ToInt64(ReadString(key));
        }

        private double ReadDouble(string key)
        {
            return Convert.ToDouble(ReadString(key));
            // return ReadString(key).con();
        }

        private TEnum ReadEnum<TEnum>(string key) where TEnum : struct, IConvertible
        {
            if (!typeof(TEnum).IsEnum)
                throw new AppException("Expected an enum.");

            var value = (TEnum)(object)ReadInt(key);
            var isDefined = Enum.IsDefined(typeof(TEnum), value);

            if (!isDefined)
                throw new AppException($"Provided value: {value} is not defined in enum {typeof(TEnum)}");

            return value;
        }

        private long[] ReadLongArray(string key)
        {
            return System.Text.Json.JsonSerializer.Deserialize<long[]>(ReadString(key));
        }
    }
}
