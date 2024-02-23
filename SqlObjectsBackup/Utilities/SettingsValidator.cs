using System.Linq;
using SqlObjectsBackup.Models;

namespace SqlObjectsBackup.Utilites;

public static class SettingsValidator
{
    public static bool ValidateSettings(GitSettings gitSettings, DatabaseSettings[] dbSettingsArray)
    {
        return IsGitSettingsValid(gitSettings) && AreDatabaseSettingsValid(dbSettingsArray);
    }

    private static bool IsGitSettingsValid(GitSettings gitSettings)
    {
        return gitSettings != null 
            && !string.IsNullOrEmpty(gitSettings.RepoPath) 
            && !string.IsNullOrEmpty(gitSettings.Branch);
    }

    private static bool AreDatabaseSettingsValid(DatabaseSettings[] dbSettingsArray)
    {
        return dbSettingsArray.All(dbSettings => dbSettings != null && !string.IsNullOrEmpty(dbSettings.ConnectionString));
    }
}