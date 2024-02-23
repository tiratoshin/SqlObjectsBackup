using Microsoft.Data.SqlClient;
using NLog;
using Microsoft.Extensions.Configuration;
using SqlObjectsBackup.Models;
using SqlObjectsBackup.Utilites;
using Newtonsoft.Json;
using SqlObjectsBackup;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;


class Program
{
    private static Logger logger = LogManager.GetCurrentClassLogger();

    static async Task Main(string[] args)
    {        
        var rootCommand = new RootCommand
        {
            new Option<string>("--local-git-dir", getDefaultValue: () => ConfigurationProviders.GetConfigValue("GitSettings:RepoPath"), description: "The path to the local directory where the Git repository is located."),
            new Option<string[]>("--connection-strings", getDefaultValue: () => ConfigurationProviders.GetConfigValues("DatabaseSettings"), description: "The connection strings to the SQL Server databases."),
            new Option<string>("--branch", getDefaultValue: () => ConfigurationProviders.GetConfigValue("GitSettings:Branch"), description: "The name of the Git branch to use."),
            new Option<string>("--repo", getDefaultValue: () => ConfigurationProviders.GetConfigValue("GitSettings:Repo"), description: "The URL of the Git repository."),
            new Option<string>("--like-pattern", getDefaultValue: () => ConfigurationProviders.GetConfigValue("GitSettings:LikePattern"), description: "The pattern to match SQL objects."),
            new Option<string>("--log-file-path", getDefaultValue: () => ConfigurationProviders.GetConfigValue("GitSettings:LogFilePath"), description: "The path to the log file."),
            new Option<int>("--max-degree-of-parallelism", getDefaultValue: () => int.Parse(ConfigurationProviders.GetConfigValue("ParallelismSettings:MaxDegreeOfParallelism") ?? "1"), description: "The maximum degree of parallelism for operations."),
            new Option<List<DatabaseSettings>>("--database-settings", getDefaultValue: () => DatabaseSettingsProvider.GetDatabaseSettings("DatabaseSettings"), description: "The database settings.")
        };

        rootCommand.Description = "A command-line tool for backing up SQL objects.";

        rootCommand.Handler = CommandHandler.Create<string, List<DatabaseSettings>, string, string, string, string, int>(async (localGitDir, databaseSettings, branch, repo, likePattern, logFilePath, maxDegreeOfParallelism) =>
        {
            try
            {
                // Extract the connection strings from the list of DatabaseSetting objects
                var connectionStrings = databaseSettings.Select(ds => ds.ConnectionString).ToArray();

                // Pass the array of connection strings to your ProcessCommand method
                await CommandProcessor.ProcessCommand(localGitDir, connectionStrings, branch, repo, likePattern, logFilePath, maxDegreeOfParallelism);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        });

        await rootCommand.InvokeAsync(args);
    }
}
