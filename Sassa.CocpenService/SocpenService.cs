using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sassa.Services
{
    public class SocpenService
    {
        string connectionString;
        public SocpenService(string connectionstring) 
        {
            connectionString = connectionstring;
        }
        public void SyncSocpen()
        {
            

            using (OracleConnection conn = new OracleConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    Console.WriteLine("Connected to Oracle DB");
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = conn;  
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "UPDATE_DC_SOCPEN";
                        cmd.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}

