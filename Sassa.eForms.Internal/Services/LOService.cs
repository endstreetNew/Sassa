using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using Sassa.eDocs.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sassa.eForms.Services
{
    public class LOService : ILOService
    {
        private string LOConnectionstring;
        public LOService(IConfiguration _config)
        {
            LOConnectionstring = _config.GetConnectionString("LOConnectionString");
        }

        //public Task<IEnumerable<Document>> GetDocuments(string reference)
        //{

        //}
        public IEnumerable<Document> GetRequiredDocuments(string reference)
        {

            List<Document> docs = new List<Document>();
            try
            {

                using (OracleConnection connection = new OracleConnection(LOConnectionstring))
                {

                    connection.Open();
                    //String sql = $@"SELECT l.id_granttype_ref, l.refno, l.idno, l.applicationtype, l.documenttype, l.docinstance as DocCount, c.maxsd, CASE WHEN regexp_like(c.maxsd ,'^\d{2}$') THEN TO_NUMBER(c.maxsd, '99') + l.docinstance ELSE l.docinstance END SDNo, c.fid FROM LO_ADMIN.cust_app_doctypes l inner join CONTENTSERVER.cust_maxsd c on (l.idno = c.iname) WHERE l.refno ='{reference.Trim()}'";
                    String sql = $@"SELECT 
                                    l.id_granttype_ref, 
                                    l.refno, 
                                    l.idno, 
                                    l.applicationtype, 
                                    l.documenttype, 
                                    l.regionabbr as RegionCode,
                                    l.IDENTITY_NO,
                                    l.docinstance as DocCount
                                    FROM LO_ADMIN.CUST_APP_DOCTYPES1 l 
                                    WHERE l.refno = '{reference.Trim()}'";

                    DateTime endTime = DateTime.Now.AddSeconds(90);
                    while (DateTime.Now < endTime && docs.Count == 0)
                    {
                        using (OracleCommand command = new OracleCommand(sql, connection))
                        {
                            using (OracleDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Document result = new Document();
                                    result.CSNode = reader.GetString(0);
                                    result.Reference = reference;
                                    result.IdNo = reader.GetString(2);
                                    result.ApplicationTypeId = GetGrantType(reader.GetString(3));
                                    result.LoDocumentTypeId = int.Parse(reader.GetString(4));
                                    result.RegionCode = reader.GetString(5);
                                    result.ChildIdNo = reader.IsDBNull(6) ? null : reader.GetString(6);
                                    result.SupportDocument = "SD" + reader.GetString(7).PadLeft(2, '0');
                                    docs.Add(result);
                                }
                            }
                        }
                        if (docs.Count == 0) Thread.Sleep(1000);
                    }
                }
            }
            catch 
            {
                throw;
            }
            return docs;
        }

        public async Task<string> SetUploaded(string reference)
        {
            string result = "Ok";
            try
            {

                using (OracleConnection connection = new OracleConnection(LOConnectionstring))
                {

                    connection.Open();
                    String sql = $@"UPDATE REQUESTFORBANKPAYMENT SET DOCS_UPLOADED = 'Yes' where REFERENCE ='{reference.Trim()}'";
                    using (OracleCommand command = new OracleCommand(sql, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    sql = $@"UPDATE GRANTAPPCHILDSUPPORT SET DOCS_UPLOADED = 'Yes' where REFERENCE = '{reference.Trim()}'";
                    using (OracleCommand command = new OracleCommand(sql, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    sql = $@"UPDATE GRANTAPPFOSTERCHILD SET DOCS_UPLOADED = 'Yes' where REFERENCE = '{reference.Trim()}'";
                    using (OracleCommand command = new OracleCommand(sql, connection))
                    {
                        await command .ExecuteNonQueryAsync();
                    }
                    sql = $@"UPDATE GRANTAPPOLDPERSON SET DOCS_UPLOADED = 'Yes' where REFERENCE = '{reference.Trim()}'";
                    using (OracleCommand command = new OracleCommand(sql, connection))
                    {
                        await command .ExecuteNonQueryAsync();
                    }
                }
            }
            catch (OracleException e)
            {
                result = e.Message;
            }
            return result;
        }
        private int GetGrantType(string grantName)
        {
            int granttype = 0;
            switch (grantName.ToLower())
            {
                case "old age grant":
                    granttype = 2;
                    break;
                case "war veteran grant":
                    granttype = 3;
                    break;
                case "disability grant":
                    granttype = 4;
                    break;
                case "foster child grant":
                case "foster care grant":
                    granttype = 5;
                    break;
                case "combination grant":
                    granttype = 6;
                    break;
                case "government institution":
                    granttype = 7;
                    break;
                case "care dependency grant":
                    granttype = 8;
                    break;
                case "child support grant":
                    granttype = 9;
                    break;
                case "social relief of distress grant":
                    granttype = 11;
                    break;
                case "state maintenance grant":
                    granttype = 12;
                    break;
            }
            return granttype;
        }



    }
}
