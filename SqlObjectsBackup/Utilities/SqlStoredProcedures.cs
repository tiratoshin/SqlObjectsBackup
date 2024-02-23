using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;


namespace SqlObjectsBackup;

public class SqlStoredProcedures
{
    public static async Task<List<string>> GetMatchingStoredProceduresAsync(SqlConnection connection, string likePattern)
    {
        List<string> names = new List<string>();
        using (SqlCommand cmd = new SqlCommand(@"SELECT [name] FROM sys.procedures 
                                              WHERE [name] LIKE @pattern 
                                              AND OBJECTPROPERTY(object_id, 'IsMSShipped') = 0", connection))
        {
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
