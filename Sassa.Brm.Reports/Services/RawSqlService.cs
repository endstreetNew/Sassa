using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Sassa.Brm.Reports.Services
{ 
    public class RawSqlService
    {

        string brmcs;
        public RawSqlService(IConfiguration config)
        {
            brmcs = config.GetConnectionString("BrmConnection");
        }

        public async Task<string> GetNextAltbox(string regionCode)
        {
            try
            {
                string result;
                using (OracleConnection connection = new OracleConnection(brmcs))
                {

                    connection.Open();
                    String sql = $"select CAST(SEQ_DC_ALT_BOX_NO_{regionCode}.NEXTVAL AS NVARCHAR2(20)) from DUAL";
                    using (OracleCommand command = new OracleCommand(sql, connection))
                    {
                        result = (await command.ExecuteScalarAsync()).ToString();
                    }

                }
                return result;
            }
            catch //(OracleException e)
            {
                throw;
            }
        }

        public async Task<int> GetNextTdwBatch()
        {
            try
            {
                int result;
                using (OracleConnection connection = new OracleConnection(brmcs))
                {

                    connection.Open();
                    String sql = $"select CAST(SEQ_TDW_BATCH.NEXTVAL AS INTEGER) from DUAL";
                    using (OracleCommand command = new OracleCommand(sql, connection))
                    {
                        result = int.Parse((await command.ExecuteScalarAsync()).ToString());
                    }

                }
                return result;
            }
            catch //(OracleException e)
            {
                throw;
            }
        }

        //public async Task<string> GetSocpenSearchId(long SRDNo)
        //{

        //    try
        //    {
        //        object result;
        //        using (OracleConnection connection = new OracleConnection(brmcs))
        //        {

        //            connection.Open();
        //            String sql = $"select ID_NO from SASSA.SOCPEN_SRD_BEN where SRD_NO = {SRDNo}";
        //            using (OracleCommand command = new OracleCommand(sql, connection))
        //            {
        //                result = await command.ExecuteScalarAsync();
        //            }

        //        }
        //        return result.ToString();
        //    }
        //    catch //(OracleException e)
        //    {

        //        throw new Exception("SRD not found.");
        //    }

        //    //SocpenSrdBen result = await _context.SocpenSrdBens.Where(s => s.SrdNo == srd).AsNoTracking().FirstOrDefaultAsync();

        //    //if (result == null)
        //    //{
        //    //    throw new Exception("SRD not found.");
        //    //}
        //    //if (result.IdNo == null)
        //    //{
        //    //    throw new Exception("SRD has no Id Number associated and can't be processed.");
        //    //}
        //    //return result.IdNo.ToString();
        //}

        //SELECT BRMWAYBIL.NEXTVAL from DUAL
        public string GetNextWayBill()
        {
            try
            {
                string result;
                using (OracleConnection connection = new OracleConnection(brmcs))
                {

                    connection.Open();
                    String sql = $"SELECT BRMWAYBIL.NEXTVAL from DUAL";
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

        public async Task ExecuteNonQuery(string sql)
        {
            using (OracleConnection connection = new OracleConnection(brmcs))
            {
                OracleCommand command = new OracleCommand(sql, connection);
                command.Connection.Open();
                await command.ExecuteNonQueryAsync();
            }
        }

        public string ExecuteScalar(string sql)
        {
            try
            {
                string result;
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
