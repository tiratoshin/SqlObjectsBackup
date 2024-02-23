using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.IO;
using SqlObjectsBackup.Models;

namespace SqlObjectsBackup;

public static class DatabaseSettingsProvider
{
    public static List<DatabaseSettings> GetDatabaseSettings(string key)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var section = configuration.GetSection(key);
        var settings = new List<DatabaseSettings>();
        foreach (var child in section.GetChildren())
        {
            var setting = new DatabaseSettings
            {
                ConnectionString = child["ConnectionString"],
                Folder = child["Folder"]
            };
            settings.Add(setting);
        }

        return settings;
    }
}