namespace SqlObjectsBackup;

public class Help
{
    public static void DisplayHelp()
    {
        Console.WriteLine("Usage: YourExecutable.exe <gitRepoPath> <connectionString> <gitBranch> <gitRepo> <likePattern>] <logFilePath>]");
        Console.WriteLine("Example: YourExecutable.exe \"\\\\192.168.3.20\\git\\testclone\\pass\" \"Server=syteline-sql;Database=ZFRE_APP;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;\" \"nightly\" \"https://github.com/IEM-Holdings/PASS.git\" \"_IEM_\" \"\\\\192.168.3.20\\git\\testclone\\log\\\"");
        Console.WriteLine("Options:");
        Console.WriteLine(" <gitRepoPath>        The local path to the cloned Git repository.");
        Console.WriteLine(" <connectionString>  The SQL Server connection string.");
        Console.WriteLine(" <gitBranch>             The Git branch to commit changes to.");
        Console.WriteLine(" <gitRepo>             The URL of the remote Git repository.");
        Console.WriteLine(" <likePattern>     (Optional) A pattern to match SQL object names against. Only objects that match the pattern will be backed up.");
        Console.WriteLine(" <logFilePath>         (Optional) The path to the log file. If not specified, the log will be written to the console.");
        Console.WriteLine("  -h, --help                       Display this help message.");
    }
}

// .\sqlobjectsbackup.exe "\\192.168.3.20\git\testclone\pass" "Server=syteline-sql;Database=ZFRE_APP;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;" "nightly" "https://github.com/IEM-Holdings/PASS.git" "_IEM_" "\\192.168.3.30\git\testclone\log"

