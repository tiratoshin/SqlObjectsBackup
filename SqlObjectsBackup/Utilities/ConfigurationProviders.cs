using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;

namespace SqlObjectsBackup.Utilites;

public static class ConfigurationProviders
{
    private static IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
    }

    public static string GetConfigValue(string key)
    {
        var configuration = GetConfiguration();
        return configuration[key];
    }

    public static string[] GetConfigValues(string key)
    {
        var configuration = GetConfiguration();

        var section = configuration.GetSection(key);
        var objects = section.GetChildren();
        var values = objects.Select(o => o["ConnectionString"]).ToArray();

        return values;
    }
}