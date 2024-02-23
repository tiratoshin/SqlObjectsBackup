﻿using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SqlObjectsBackup;

public class SqlTables
{
    public static async Task<List<string>> GetMatchingTablesAsync(SqlConnection connection, string likePattern)
    {
        List<string> names = new List<string>();
        using (SqlCommand cmd = new SqlCommand(@"SELECT [name] FROM sys.tables 
                                                  WHERE [name] LIKE @pattern", connection))
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
