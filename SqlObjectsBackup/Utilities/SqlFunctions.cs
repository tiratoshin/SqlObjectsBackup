using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

public class SqlFunctions
{
    public static async Task<List<string>> GetMatchingFunctionsAsync(SqlConnection connection, string likePattern)
    {
        List<string> names = new List<string>();
        // Pass the command text and connection directly to the SqlCommand constructor
        using (SqlCommand cmd = new SqlCommand(@"SELECT [name] FROM sys.objects 
                                                  WHERE [name] LIKE @pattern 
                                                  AND (type_desc = 'SQL_SCALAR_FUNCTION' 
                                                       OR type_desc = 'SQL_TABLE_VALUED_FUNCTION' 
                                                       OR type_desc = 'SQL_INLINE_TABLE_VALUED_FUNCTION')", connection))
        {
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@pattern", "%" + likePattern + "%");

            // Use ExecuteReaderAsync for asynchronous operation
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
