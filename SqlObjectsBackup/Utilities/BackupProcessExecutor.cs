using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NLog;
using SqlObjectsBackup.Models;

namespace SqlObjectsBackup.Utilites;

public class BackupProcessExecutor
{
    private static ILogger logger = LogManager.GetCurrentClassLogger();
    static List<string> modifiedFiles = new List<string>();
    static List<string> matchingStoredProcedures = new List<string>();
    static List<string> matchingTables = new List<string>();
    static List<string> matchingViews = new List<string>();
    static List<string> matchingTriggers = new List<string>();
    static List<string> matchingFunctions = new List<string>();

    public static async Task ExecuteBackupProcess(GitSettings gitSettings, DatabaseSettings[] dbSettingsArray, ParallelismSettings parallelismSettings)
    {
        // Initialize Git object
        Git git = new Git(logger);
        try
        {
            // Checkout to the specified branch
            Console.WriteLine("Checking out to branch...");
            git.Checkout(gitSettings.Branch, gitSettings.RepoPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during checkout: {ex.Message}");
            logger.Error(ex, "Error during checkout to branch {Branch}.", gitSettings.Branch);
        }
        try
        {
            // Pull the latest changes from the remote repository
            Console.WriteLine("Pulling latest changes...");
            git.Pull(gitSettings.Branch, gitSettings.RepoPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during pull: {ex.Message}");
            logger.Error(ex, "Error during pull from repository {RepoPath} on branch {Branch}.", gitSettings.RepoPath, gitSettings.Branch);
        }

        foreach (var dbSettings in dbSettingsArray)
        {
            Console.WriteLine($"Processing database: {dbSettings.ConnectionString}");
            string connectionString = dbSettings.ConnectionString;
            string folder = dbSettings.Folder;
            try
            {
                using (SqlConnection connection = new SqlConnection(dbSettings.ConnectionString))
                {
                    try
                    {
                        // Open the connection asynchronously
                        await connection.OpenAsync();
                        Console.WriteLine("Connected to SQL Server");
                        Console.WriteLine("Getting matching SQL objects...");
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine("Error connecting to SQL Server: " + ex.Message);
                        logger.Error(ex, "Error connecting to SQL Server.");
                        return; // Exit the method early if we can't connect to the database
                    }


                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .Build();

                    parallelismSettings = configuration.GetSection("ParallelismSettings").Get<ParallelismSettings>() ?? new ParallelismSettings();


                    // Get and save SQL objects in parallel
                    Console.WriteLine("Getting and saving SQL objects...");
                    int calculatedParallelism = Math.Min(parallelismSettings.MaxDegreeOfParallelism, Environment.ProcessorCount - 1);
                    var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(calculatedParallelism, 1) };
                    var sqlObjectTypes = new Dictionary<string, Func<SqlConnection, string, Task<Dictionary<string, string>>>>
                    {
                        {"Stored_Procedure", SqlObjectRetriever.GetStoredProceduresAsync},
                        {"Function", SqlObjectRetriever.GetFunctionsAsync},
                        {"Trigger", SqlObjectRetriever.GetTriggersAsync},
                        {"View", SqlObjectRetriever.GetViewsAsync},
                        {"Table", SqlObjectRetriever.GetTablesAsync}
                    };
                    var folderPaths = new Dictionary<string, string>
                    {
                        {"Stored_Procedure", Path.Combine(dbSettings.Folder, "Stored Procedures")},
                        {"Function", Path.Combine(dbSettings.Folder, "Function")},
                        {"Trigger", Path.Combine(dbSettings.Folder, "Triggers")},
                        {"View", Path.Combine(dbSettings.Folder, "Views")},
                        {"Table", Path.Combine(dbSettings.Folder, "Tables")}
                    };
                    Console.WriteLine("Starting to loop through object types...");
                    Console.WriteLine($"Number of items in sqlObjectTypes: {sqlObjectTypes.Count}");
                    foreach (var objectType in sqlObjectTypes.Keys)
                    {
                        if (objectType == "Stored_Procedure")
                        {
                        Console.WriteLine($"Working on '{objectType}'");
                        var objectTexts = await sqlObjectTypes[objectType](connection, gitSettings.LikePattern);
                        var folderPath = folderPaths[objectType];

                        Console.WriteLine($"Found {objectTexts.Count} {objectType}(s).");

                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                            Console.WriteLine($"Created folder: {folderPath}");
                        }

                        // Write files in parallel
                        Console.WriteLine($"Number of items in objectTexts: {objectTexts.Count}");
                        try
                        {
                            //foreach (var kvp in objectTexts)
                            Parallel.ForEach(objectTexts, parallelOptions, kvp =>
                            {
                                string objectName = kvp.Key;
                                string objectText = kvp.Value;

                                string scriptFilePath = Path.Combine(folderPath, $"{objectName}.sql");
                                Console.WriteLine($"Checking: {objectName}");

                                // Read existing content if the file exists
                                string existingContent = File.Exists(scriptFilePath) ? File.ReadAllText(scriptFilePath) : string.Empty;
                                Console.WriteLine($"Script file path: {scriptFilePath}");

                                // If content has changed, update the file and add to the list of modified files
                                if (existingContent.TrimEnd() != objectText.TrimEnd())
                                {
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"Modifying: {objectName}");
                                    Console.ResetColor(); // Reset the console color to default
                                    try
                                    {
                                        Console.WriteLine($"Starting to write file: {objectName}");  
                                        logger.Info($"Starting to write file: {objectName}"); 

                                        File.WriteAllText(scriptFilePath, objectText);
                                        Console.WriteLine($"Finished writing file: {objectName}"); 
                                        logger.Info($"Finished writing file: {objectName}"); 
                                        modifiedFiles.Add(scriptFilePath);
                                    }
                                    catch (IOException ex)
                                    {
                                        Console.WriteLine("File I/O Error: " + ex.Message);
                                        logger.Error(ex, "File I/O Error.");
                                    }
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);
                        }
                            
                        }
                    }

                    try
                    {
                        // Push the changes to the remote repository
                        Console.WriteLine("Pushing changes to remote repository...");
                        logger.Info("Pushing changes to remote repository...");
                        git.Push(gitSettings.RepoPath, gitSettings.Branch, modifiedFiles);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during push: {ex.Message}");
                        logger.Error(ex, "Error during push to repository {RepoPath} on branch {Branch}.", gitSettings.RepoPath, gitSettings.Branch);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                logger.Error(ex, "Error during script execution.");
            }
        }
    }
}