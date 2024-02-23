using SqlObjectsBackup.Models;

namespace SqlObjectsBackup.Utilites;

public class CommandProcessor
{
    public static async Task ProcessCommand(string repoPath, string[] connectionStrings, string branch, string repo, string likePattern, string logFilePath, int maxDegreeOfParallelism)
    {
        // Update GitSettings initialization with new parameters
        GitSettings gitSettings = new GitSettings
        {
            RepoPath = repoPath,
            Branch = branch,
            Repo = repo,
            LikePattern = likePattern, 
            LogFilePath = logFilePath
        };

        List<DatabaseSettings> databaseSettings = DatabaseSettingsProvider.GetDatabaseSettings("DatabaseSettings");
        SqlObjectsBackup.Models.DatabaseSettings[] dbSettingsArray = databaseSettings
        .Select(dbSetting => new SqlObjectsBackup.Models.DatabaseSettings
        {
            ConnectionString = dbSetting.ConnectionString,
            Folder = dbSetting.Folder
        })
        .ToArray();

        if (!SettingsValidator.ValidateSettings(gitSettings, dbSettingsArray))
        {
            Console.WriteLine("Error: Missing essential arguments.");
            return;
        }

        ParallelismSettings parallelismSettings = new ParallelismSettings
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };

        await BackupProcessExecutor.ExecuteBackupProcess(gitSettings, dbSettingsArray, parallelismSettings);
    }
}