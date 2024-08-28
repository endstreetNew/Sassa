using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Sassa.BRM.Services
{
    public class RawSqlService
    {

        string? brmcs;
        public RawSqlService(IConfiguration config)
        {
            brmcs = config.GetConnectionString("BrmConnection");
        }

        public async Task ExecuteNonQuery(string sql)
        {
            using (OracleConnection connection = new OracleConnection(brmcs))
            {
                OracleCommand command = new OracleCommand(sql, connection);
                command.Connection.Open();
                await command.ExecuteNonQueryAsync();
            }
        }

        public string? ExecuteScalar(string sql)
        {
            try
            {
                string? result;
                using (OracleConnection connection = new OracleConnection(brmcs))
                {

                    connection.Open();
                    using (OracleCommand command = new OracleCommand(sql, connection))
                    {
                        result = command.ExecuteScalar().ToString();
                    }

                }
                return result;
            }
            catch //(OracleException e)
            {
                throw;
            }
        }

        public DateTime GetBookMark(string sql)
        {
            try
            {
                DateTime result;
                using (OracleConnection connection = new OracleConnection(brmcs))
                {

                    connection.Open();
                    using (OracleCommand command = new OracleCommand(sql, connection))
                    {
                        result = (DateTime)command.ExecuteScalar();
                    }

                }
                return result;
            }
            catch //(OracleException e)
            {
                throw;
            }
        }

        public async Task<DataTable> GetTable(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                using (OracleConnection con = new OracleConnection(brmcs))
                {
                    OracleCommand cmd = con.CreateCommand();
                    cmd.BindByName = true;
                    cmd.CommandTimeout = 0;
                    cmd.FetchSize *= 8;

                    //Destruction List
                    cmd.CommandText = sql;
                    con.Open();
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        await Task.Run(() => adapter.Fill(dt));
                    }
                    //var reader = await con.CreateCommand().ExecuteReaderAsync();
                    //dt.Load(reader);
                    con.Close();
                    return dt;
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
