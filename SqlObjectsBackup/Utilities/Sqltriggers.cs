using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;


namespace SqlObjectsBackup;

public class SqlTriggers
{
    public static async Task<List<string>> GetMatchingTriggersAsync(SqlConnection connection, string likePattern)
    {
        List<string> names = new List<string>();
        using (SqlCommand cmd = new SqlCommand(@"SELECT [name] FROM sys.triggers 
                                                  WHERE [name] LIKE @pattern 
                                                  AND is_ms_shipped = 0", connection))
        {
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@pattern", "%" + likePattern + "%");

            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    names.Add(reader["name"].ToString());
                }
            }
        }
        return names;
    }
}
