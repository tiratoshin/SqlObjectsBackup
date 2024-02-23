using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Linq;
using System.Text;
using NLog;

public class SqlObjectScripter
{

    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    //static List<string> matchingStoredProcedures = new List<string>();

    public static Dictionary<string, string> GetSqlObjectsUsingSMO(SqlConnection connection, string objectType, string likePattern, List<string> matchingObjects)
    {
        Console.WriteLine($"matchingObjects Count Inside GetSqlObjectsUsingSMO: {matchingObjects.Count}");

        Dictionary<string, string> result = new Dictionary<string, string>();

        ServerConnection serverConnection = new ServerConnection(connection);
        Server server = new Server(serverConnection);
        Database db = server.Databases[connection.Database];

        ScriptingOptions options = new ScriptingOptions();
        options.ScriptDrops = false; // Do not script DROP statements
        options.Indexes = true; // Script out indexes
        options.DriAll = true; // Script out all declarative referential integrity
        options.Triggers = true; // Script out triggers
        options.FullTextIndexes = true; // Script out full-text indexes
        options.IncludeHeaders = false; // Include standard SQL headers
        options.ToFileOnly = false; // Don't just script to a file, we want the string output


        switch (objectType)
        {
            case "StoredProcedure":
                Console.WriteLine("Starting Stored Procedures...");
                foreach (StoredProcedure sp in db.StoredProcedures)
                {
                    if (matchingObjects.Contains(sp.Name))
                    {
                        StringBuilder fullScript = new StringBuilder();
                        foreach (string script in sp.Script(options))
                        {
                            fullScript.AppendLine(script);
                            fullScript.AppendLine("GO");
                        }
                        Console.WriteLine("Working on: " + sp.Name + " |  Script parts: " + sp.Script(options).Cast<string>().Count());
                        logger.Info("Working on: " + sp.Name + " |  Script parts: " + sp.Script(options).Cast<string>().Count());
                        result[sp.Name] = fullScript.ToString();
                    }
                }
                Console.WriteLine("Stored procedures done.");
                break;


            case "Function":
                Console.WriteLine("Starting Functions...");
                foreach (UserDefinedFunction fn in db.UserDefinedFunctions)
                {
                    if (matchingObjects.Contains(fn.Name))
                    {
                        StringBuilder fullScript = new StringBuilder();
                        foreach (string script in fn.Script(options))
                        {
                            fullScript.AppendLine(script);
                            fullScript.AppendLine("GO");
                        }
                        Console.WriteLine("Working on: " + fn.Name + " |  Script parts: " + fn.Script(options).Cast<string>().Count());
                        logger.Info("Working on: " + fn.Name + " |  Script parts: " + fn.Script(options).Cast<string>().Count());
                        result[fn.Name] = fullScript.ToString();
                    }
                }
                Console.WriteLine("Functions done.");
                break;


            case "Trigger":
                Console.WriteLine("Starting Triggers...");

                // Scripting out Database-Level Triggers
                foreach (Trigger dbTrigger in db.Triggers)
                {
                    if (matchingObjects.Contains(dbTrigger.Name))
                    {
                        StringBuilder fullScript = new StringBuilder();
                        foreach (string script in dbTrigger.Script(options))
                        {
                            fullScript.AppendLine(script);
                            fullScript.AppendLine("GO");
                        }
                        Console.WriteLine("Working on: " + dbTrigger.Name + " |  Script parts: " + dbTrigger.Script(options).Cast<string>().Count());
                        logger.Info("Working on: " + dbTrigger.Name + " |  Script parts: " + dbTrigger.Script(options).Cast<string>().Count());
                        result[dbTrigger.Name] = fullScript.ToString();
                    }
                }

                // Scripting out Table-Level Triggers
                foreach (Table tbl in db.Tables)
                {
                    foreach (Trigger tableTrigger in tbl.Triggers)
                    {
                        if (matchingObjects.Contains(tableTrigger.Name))
                        {
                            StringBuilder fullScript = new StringBuilder();
                            foreach (string script in tableTrigger.Script(options))
                            {
                                fullScript.AppendLine(script);
                                fullScript.AppendLine("GO");
                            }
                            Console.WriteLine("Working on: " + tableTrigger.Name + " |  Script parts: " + tableTrigger.Script(options).Cast<string>().Count());
                            logger.Info("Working on: " + tableTrigger.Name + " |  Script parts: " + tableTrigger.Script(options).Cast<string>().Count());
                            result[tableTrigger.Name] = fullScript.ToString();
                        }
                    }
                }

                Console.WriteLine("Triggers done.");
                break;

            case "View":
                Console.WriteLine("Starting Views...");
                foreach (View vw in db.Views)
                {
                    if (matchingObjects.Any(name => string.Equals(name, vw.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        StringBuilder fullScript = new StringBuilder();
                        foreach (string script in vw.Script(options))
                        {
                            fullScript.AppendLine(script);
                            fullScript.AppendLine("GO");
                        }
                        Console.WriteLine("Working on: " + vw.Name + " |  Script parts: " + vw.Script(options).Cast<string>().Count());
                        logger.Info("Working on: " + vw.Name + " |  Script parts: " + vw.Script(options).Cast<string>().Count());
                        result[vw.Name] = fullScript.ToString();
                    }
                }
                Console.WriteLine($"Generated scripts for {result.Count} views.");
                Console.WriteLine("Views done.");
                break;

            case "Table":
                Console.WriteLine("Starting Tables...");
                Console.WriteLine($"Number of matching tables: {matchingObjects.Count}");
                foreach (string name in matchingObjects)
                {
                    Console.WriteLine($"Matching table: {name}");
                }
                foreach (Table tbl in db.Tables)
                {
                    Console.WriteLine($"Checking table: {tbl.Name}");
                    if (matchingObjects.Contains(tbl.Name))
                    {
                        StringBuilder fullScript = new StringBuilder();
                        var scriptParts = tbl.Script(options).Cast<string>().ToList();
                        foreach (string script in scriptParts)
                        {
                            fullScript.AppendLine(script);
                            fullScript.AppendLine("GO");
                        }
                        Console.WriteLine($"Working on: {tbl.Name} |  Script parts: {scriptParts.Count}");
                        result[tbl.Name] = fullScript.ToString();
                    }
                    else
                    {
                        Console.WriteLine($"Table {tbl.Name} is not in the matchingObjects list.");
                    }
                }
                Console.WriteLine("Tables done.");
                break;

        }
        Console.WriteLine("Finished " + objectType + ".");
        logger.Info("Finished " + objectType + ".");
        return result;
    }
}