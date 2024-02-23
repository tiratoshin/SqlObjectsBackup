
using Microsoft.Data.SqlClient;
using NLog;

namespace SqlObjectsBackup.Utilites;

public static class SqlObjectRetriever
{
    private static Logger logger = LogManager.GetCurrentClassLogger();
    // private static List<string> matchingStoredProcedures = new List<string>();
    // private static List<string> matchingFunctions = new List<string>();
    // private static List<string> matchingTriggers = new List<string>();
    // private static List<string> matchingViews = new List<string>();
    // private static List<string> matchingTables = new List<string>();

    public static async Task<Dictionary<string, string>> GetStoredProceduresAsync(SqlConnection connection, string likePattern)
    {
        Console.WriteLine("Getting stored procedures...");
        logger.Info("Getting stored procedures...");
        var matchingStoredProcedures = await SqlStoredProcedures.GetMatchingStoredProceduresAsync(connection, likePattern);
        return SqlObjectScripter.GetSqlObjectsUsingSMO(connection, "StoredProcedure", likePattern, matchingStoredProcedures);
    }
    public static async Task<Dictionary<string, string>> GetFunctionsAsync(SqlConnection connection, string likePattern)
    {
        Console.WriteLine("Getting functions...");
        logger.Info("Getting functions...");
        var matchingFunctions = await SqlFunctions.GetMatchingFunctionsAsync(connection, likePattern);
        return SqlObjectScripter.GetSqlObjectsUsingSMO(connection, "Function", likePattern, matchingFunctions);
    }

    public static async Task<Dictionary<string, string>> GetTriggersAsync(SqlConnection connection, string likePattern)
    {
        Console.WriteLine("Getting triggers...");
        logger.Info("Getting triggers...");
        var matchingTriggers = await SqlTriggers.GetMatchingTriggersAsync(connection, likePattern);
        return SqlObjectScripter.GetSqlObjectsUsingSMO(connection, "Trigger", likePattern, matchingTriggers);
    }

    public static async Task<Dictionary<string, string>> GetViewsAsync(SqlConnection connection, string likePattern)
    {
        Console.WriteLine("Getting views...");
        logger.Info("Getting views...");
        var matchingViews = await SqlViews.GetMatchingViewsAsync(connection, likePattern);
        return SqlObjectScripter.GetSqlObjectsUsingSMO(connection, "View", likePattern, matchingViews);
    }

    public static async Task<Dictionary<string, string>> GetTablesAsync(SqlConnection connection, string likePattern)
    {
        Console.WriteLine("Getting tables...");
        logger.Info("Getting tables...");
        var matchingTables = await SqlTables.GetMatchingTablesAsync(connection, likePattern);
        return SqlObjectScripter.GetSqlObjectsUsingSMO(connection, "Table", likePattern, matchingTables);
    }
}