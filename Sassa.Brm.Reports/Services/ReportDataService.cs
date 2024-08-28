using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using razor.Components.Models;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Data.ViewModels;
using Sassa.BRM.Models;
using Sassa.BRM.ViewModels;
//using Sassa.eDocs.Data.Migrations;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sassa.Brm.Common.Services
{
    public class ReportDataService
    {

        private string connectionString = string.Empty;
        private string reportFolder;
        public Dictionary<string, string> reportList;

        ProgressService _ogs;
        //BRMDbService db;
        StaticService sservice;

        public ReportDataService(IConfiguration config, IWebHostEnvironment env, ProgressService ogs,  StaticService Sservice)
        {
            connectionString = config.GetConnectionString("BrmConnection");
            sservice = Sservice;
            //reportFolder = config.GetSection("Folders").GetChildren().GetValue("Reports");
            reportFolder = Path.Combine(env.ContentRootPath, config["Folders:Reports"]);
            if (!Directory.Exists(reportFolder))
            {
                Directory.CreateDirectory(reportFolder);
            }
            else
            {
                string[] files = Directory.GetFiles(reportFolder);

                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.LastAccessTime < DateTime.Now.AddMonths(-1))
                    {
                        fi.Delete();
                    }
                }
            }
            //var rr = .Bind(folders);
            reportList = new Dictionary<string, string>();
            //reportList.Add("1", "Destruction List");
            //reportList.Add("2", "Destruction Status");
            reportList.Add("3", "Not Captured Report");
            reportList.Add("4", "Active Users");
            reportList.Add("5", "Activity Log");
            reportList.Add("6", "Activity By Action per User");
            reportList.Add("7", "Performance Report");
            reportList.Add("8", "Missing File Summary");
            reportList.Add("9", "Monthly Scanning Report");
            reportList.Add("10", "Deleted files Report");
            reportList.Add("11", "Manual Capture Report");
            reportList.Add("12", "Missing Files Report");

            //db = _db;

            _ogs = ogs;
        }

        public async Task SaveCsvReport(string dateFrom, string dateTo, string rIndex, string office_id, string office_type, string region_id, string grant_type, string filename, string status = "", string sql = "")
        {
            ReportHeader header = new ReportHeader();
            header.FromDate = dateFrom;
            header.ToDate = dateTo;
            int usernameIndex = filename.IndexOf('-') + 1;
            header.Username = filename.Substring(usernameIndex, filename.Substring(usernameIndex).IndexOf("-"));//{_session.Office.RegionCode}-{_session.SamName.ToUpper()}-
            header.Region = StaticDataService.RegionName(region_id);

            System.Data.DataTable dt = new System.Data.DataTable();

            string regionSQL = "";
            string region1SQL = "";
            string region2SQL = "";
            string OfficeSQL = "";
            string statusSQL = "";

            if (dateFrom.StartsWith("<"))
            {
                dateFrom = "0000";
            }

            if (!string.IsNullOrEmpty(status))
            {
                statusSQL = " AND D.STATUS = '" + status + "' ";
            }

            if (!string.IsNullOrEmpty(region_id))
            {
                regionSQL = $" AND b.PROVINCE = '{region_id}'";
                region1SQL = $" AND REGION = '{region_id}'";
                region2SQL = $" AND A.REGION_ID = '{region_id}'";
            }

            if (!String.IsNullOrEmpty(office_id) && !string.IsNullOrEmpty(region_id))
            {
                //PaypointSQL = " AND b.SEC_PAYPOINT ='" + office_id + "' ";
                OfficeSQL = $" AND A.OFFICE_ID = '{office_id}'";
            }
            try
            {
                using (OracleConnection con = new OracleConnection(connectionString))
                {
                    using (OracleCommand cmd = con.CreateCommand())
                    {
                        cmd.BindByName = true;
                        cmd.CommandTimeout = 0;
                        cmd.FetchSize *= 8;
                        switch (rIndex)
                        {
                            case "1"://Destruction List
                                if (dateFrom.StartsWith("<"))
                                {

                                    dateFrom = "0000";
                                }

                                cmd.CommandText = "SELECT REGION_NAME, Pension_No,NAME,SURNAME, GRANT_TYPE, STATUS FROM DC_DESTRUCTION D JOIN contentserver.DC_REGION R ON D.REGION_ID = R.REGION_ID " +
                                                   string.Format("WHERE SUBSTR(DESTRUCTIO_DATE,1,4) =  '{0}' ", dateFrom) +
                                                   (region_id == "" ? "" : string.Format("AND D.REGION_ID = {0} ", region_id)) +
                                                   (status == "" ? "" : string.Format("AND D.STATUS = '{0}' ", status)) +
                                                   "ORDER BY REGION_NAME";
                                break;
                            case "2"://Destruction Status
                                cmd.CommandText = string.Format("SELECT * FROM " +
                                "(" +
                                "SELECT SUBSTR(D.DESTRUCTIO_DATE,1,4) AS YEAR,R.REGION_NAME,D.STATUS FROM contentserver.DC_destruction D " +
                                "JOIN contentserver.DC_REGION R ON D.REGION_ID = R.REGION_ID " +
                                "Where SUBSTR(DESTRUCTIO_DATE,1,4) = '{1}' " + (region_id == "" ? ")" : string.Format("AND D.REGION_ID = {0}) ", region_id)) +
                                "PIVOT (  Count(STATUS)  FOR STATUS IN ('Selected', 'Excluded', 'TDWFound', 'TDWNotFound', 'Approved', 'Destroyed', 'Exception')" +
                                ")" +
                                "ORDER BY YEAR,REGION_NAME", region_id, dateFrom);
                                break;
                            case "3"://Missing Files new
                                cmd.CommandText = "SELECT r.Region_NAME, b.Paypoint, g.TYPE_NAME, b.Application_date, b.BENEFICIARY_ID,b.CHILD_ID,b.NAME,b.SURNAME FROM DC_SOCPEN b  " +
                                    "INNER join DC_REGION r ON b.REGION_ID = r.REGION_ID " +
                                    "INNER JOIN DC_GRANT_TYPE g ON b.GRANT_TYPE = g.TYPE_ID " +
                                    "Where b.status_code = 'ACTIVE'  AND b.CAPTURE_REFERENCE is null " +
                                    "AND b.TDW_REC IS NULL and b.Mis_file is null and b.ECMIS_FILE is null and b.OGA_date is null" +
                                $" AND b.Application_date >= to_date('{dateFrom}', 'dd/mm/YYYY')" +
                                $" and b.Application_date <= to_date('{dateTo}', 'dd/mm/YYYY')" +
                                (string.IsNullOrEmpty(grant_type) ? "" : $" AND b.GRANT_TYPE = '{grant_type}'") +
                                (string.IsNullOrEmpty(region_id) ? "" : $" AND b.REGION_ID = '{region_id}'") +
                                " ORDER BY b.REGION_ID,b.Application_date";
                                break;
                            case "4"://Active Users
                                cmd.CommandText = $"select r.REGION_NAME as Region, o.OFFICE_NAME as Office, a.USERNAME as User_Name, max(a.ACTIVITY_DATE) as Last_Active " +
                                    "from DC_ACTIVITY a " +
                                    "inner join DC_REGION r on a.Region_id = r.Region_ID " +
                                    "inner join DC_OFFICE_KUAF_LINK k on a.Username = k.USERNAME " +
                                    "inner Join DC_LOCAL_OFFICE o on k.OFFICE_ID = o.OFFICE_ID " +
                                    (string.IsNullOrEmpty(region_id) ? "" : $" Where a.REGION_ID = '{region_id}'") +
                                    "group by r.REGION_NAME, o.OFFICE_NAME, a.USERNAME " +
                                    "order by r.Region_Name,o.Office_Name, a.USERNAME";
                                break;
                            case "5"://User Activity
                                cmd.CommandText = "SELECT DISTINCT LO.OFFICE_NAME, A.USERNAME, A.AREA, A.ACTIVITY, A.ACTIVITY_DATE " +
                                  "FROM contentserver.DC_ACTIVITY A " +
                                  "INNER JOIN contentserver.DC_LOCAL_OFFICE LO ON A.OFFICE_ID = LO.OFFICE_ID " +
                                  "INNER JOIN contentserver.DC_LOCAL_OFFICE LOR ON LO.REGION_ID = LOR.REGION_ID " +
                                  $"WHERE A.ACTIVITY_DATE >= to_date('{dateFrom}', 'dd/mm/YYYY') AND A.ACTIVITY_DATE <= (to_date('" + dateTo + "', 'dd/mm/YYYY') + 1) " +
                                  (string.IsNullOrEmpty(region_id) ? "" : $" AND a.REGION_ID = '{region_id}'") +
                                  (string.IsNullOrEmpty(office_id) ? "" : $" AND A.OFFICE_ID = '{office_id}'") +
                                  "ORDER by A.USERNAME , A.ACTIVITY_DATE";
                                break;
                            case "6"://Activity Pivot
                                cmd.CommandText = $@"SELECT * FROM
                                        (SELECT DISTINCT  CONCAT( LO.OFFICE_NAME ,CONCAT(' - ', A.USERNAME)) AS USERNAME, A.AREA, A.ACTIVITY_DATE FROM contentserver.DC_ACTIVITY A " +
                                               "JOIN contentserver.DC_LOCAL_OFFICE LO ON A.OFFICE_ID = LO.OFFICE_ID " +
                                               "JOIN contentserver.DC_LOCAL_OFFICE LOR ON LO.REGION_ID = LOR.REGION_ID " +
                                               "WHERE A.ACTIVITY_DATE >= to_date('" + dateFrom + "', 'dd/mm/YYYY') " +
                                               "AND A.ACTIVITY_DATE <= (to_date('" + dateTo + "', 'dd/mm/YYYY') + 1) " +
                                               (string.IsNullOrEmpty(region_id) ? "" : $" AND a.REGION_ID = '{region_id}'") +
                                               (string.IsNullOrEmpty(office_id) ? "" : $" AND A.OFFICE_ID = '{office_id}'") +
                                               " ORDER by A.USERNAME " +
                                               " ) " +
                                            "PIVOT ( " +
                                              " Count(ACTIVITY_DATE) " +
                                              " FOR AREA " +
                                              " IN ('Capture-File','Capture-LC','Capture-SRD', 'Reboxing-File','Reboxing-LC','Reboxing-SRD','Batching','BoxAudit', 'QCFile') " +
                                            Environment.NewLine + " )";
                                break;
                            case "7"://Performance report
                                DcLocalOffice office = sservice.GetLocalOffice(office_id);
                                if (office.OfficeType == "RMC")
                                {
                                    cmd.CommandText = $@"SELECT * FROM
                                            (
                                            SELECT CONCAT( LO.OFFICE_NAME ,CONCAT(' - ', A.USERNAME)) AS USERNAME, A.AREA, A.ACTIVITY_DATE 
                                            FROM contentserver.DC_ACTIVITY A 
                                            JOIN contentserver.DC_LOCAL_OFFICE LO ON A.OFFICE_ID = LO.OFFICE_ID 
                                            WHERE A.ACTIVITY_DATE >= to_date('{dateFrom}', 'dd/mm/YYYY') 
                                            AND A.ACTIVITY_DATE <= (to_date('{dateTo}', 'dd/mm/YYYY') + 1) " +
                                                (string.IsNullOrEmpty(region_id) ? "" : $" AND a.REGION_ID = '{region_id}'") +
                                                (string.IsNullOrEmpty(office_id) ? "" : $" AND A.OFFICE_ID = '{office_id}'") +
                                                ") PIVOT (  Count(ACTIVITY_DATE)  FOR AREA  IN ('Capture-File','Capture-LC','Capture-SRD', 'Reboxing-File','Reboxing-LC','Reboxing-SRD','Batching','Enquiry') )";
                                }
                                else
                                {
                                    cmd.CommandText = $@"SELECT * FROM
                                            (
                                            SELECT CONCAT( LO.OFFICE_NAME ,CONCAT(' - ', A.USERNAME)) AS USERNAME, A.AREA, A.ACTIVITY_DATE 
                                            FROM contentserver.DC_ACTIVITY A 
                                            JOIN contentserver.DC_LOCAL_OFFICE LO ON A.OFFICE_ID = LO.OFFICE_ID 
                                            WHERE A.ACTIVITY_DATE >= to_date('{dateFrom}', 'dd/mm/YYYY') 
                                            AND A.ACTIVITY_DATE <= (to_date('{dateTo}', 'dd/mm/YYYY') + 1) " +
                                            (string.IsNullOrEmpty(region_id) ? "" : $" AND a.REGION_ID = '{region_id}'") +
                                            (string.IsNullOrEmpty(office_id) ? "" : $" AND A.OFFICE_ID = '{office_id}'") +
                                            ") PIVOT (  Count(ACTIVITY_DATE)  FOR AREA  IN ('Capture-File','Capture-LC','Capture-SRD', 'Batching','Enquiry') )";
                                }
                                break;
                            case "9"://Scanning Report
                                cmd.CommandText = $@"SELECT
                                                    r.Region_name as Region,
                                                    o.Office_name as Office,
                                                    dc.beneficiary_id,
                                                    dc.name,
                                                    dc.surname,
                                                    g.Type_name as Grant_name,
                                                    dc.Child_id,
                                                    dc.Capture_Reference as CLM_Number,
                                                    dc.Capture_date,
                                                    dc.Application_Date AS SOCPEN_CAPTURE_DATE,
                                                    dc.Scan_Date,
                                                    dc.CS_DATE as Content_Server_Date,
                                                    dc.TDW_REC as TDW_RECEIVE_DATE,
                                                    dc.OGA_Date, 
                                                    dc.Exception
                                                FROM
                                                    dc_socpen dc
                                                    join DC_REGION r on r.Region_ID = dc.region_id 
                                                    join DC_LOCAL_OFFICE o on o.OFFICE_ID = dc.localoffice_id 
                                                    join DC_GRANT_TYPE g on g.Type_ID = dc.grant_type
                                                    where dc.localoffice_id ='{office_id}'
                                                    and dc.Application_Date >= to_date('{dateFrom}', 'dd/mm/YYYY') 
                                                    and dc.Application_Date <= to_date('{dateTo}', 'dd/mm/YYYY')";
                                break;
                            case "10":
                                cmd.CommandText = $@"Select Updated_date as UpdatedDate,UPDATED_BY_AD as UserName,UNQ_File_NO as ClmNo,'Deleted' as Action, lo.Office_name as Location,Applicant_no as IdNumber,BRM_BARCODE as BrmBarcode,FILE_COMMENT as Reason
                                    from DC_FILE_DELETED d
                                    join DC_Local_office lo on lo.Office_id = d.office_id
                                    where Updated_date >= to_date('{dateFrom}', 'dd/mm/YYYY') 
                                    and Updated_date <= to_date('{dateTo}', 'dd/mm/YYYY') ";
                                break;
                            case "11":
                                cmd.CommandText = $@"select ACTIVITY_DATE,USERNAME,f.UNQ_FILE_NO as CLM_NO,ACTIVITY,r.Region_Name as LOCATION,f.Applicant_no as ID,Child_ID_No as CHILD_ID, BRM_BARCODE,g.Type_name as Grant_type,f.scan_datetime as SCANDATE,f.File_Comment as DESCRIPTION
                                                    from dc_file f
                                                    join dc_activity a on a.UNQ_FILE_NO =  f.UNQ_FILE_NO 
                                                    join dc_region r on r.region_id = f.Region_id
                                                    join dc_grant_type  g on f.grant_Type = g.type_id
                                                    where f.File_Comment Like 'Freecapture%'
                                                    AND f.Region_id = '{region_id}'";
                                break;

                            //Audit Report
                            case "12":
                                cmd.CommandText = $@"SELECT  r.Region_NAME,b.BENEFICIARY_ID , g.TYPE_NAME, max(b.Application_date) AS Application_date,b.NAME,b.SURNAME , b.Paypoint FROM DC_SOCPEN b --,b.NAME,b.SURNAME , b.Paypoint 
                                                INNER join DC_REGION r ON b.REGION_ID = r.REGION_ID
                                                INNER JOIN DC_GRANT_TYPE g ON b.GRANT_TYPE = g.TYPE_ID AND b.Grant_TYPE <> 'S'
                                                Where b.status_code = 'ACTIVE'
                                                AND b.CAPTURE_REFERENCE is null AND b.TDW_REC IS NULL and b.Mis_file is null and b.ECMIS_FILE is null and b.OGA_DATE is null  
                                                and (BENEFICIARY_ID, Grant_Type) not in (select beneficiary_id, Grant_Type from dc_socpen s Where b.beneficiary_id = s.beneficiary_id and b.Grant_TYPE = s.Grant_Type AND instr('C59', b.Grant_TYPE) > 0 AND (s.CAPTURE_REFERENCE is not null or s.TDW_REC IS not NULL or s.Mis_file is not null or s.ECMIS_FILE is not null or s.OGA_DATE is not null) )" +
                                                $" AND b.Application_date >= to_date('{dateFrom}', 'dd/mm/YYYY')" +
                                                $" and b.Application_date <= to_date('{dateTo}', 'dd/mm/YYYY')" +
                                            (string.IsNullOrEmpty(grant_type) ? "" : $" AND b.GRANT_TYPE = '{grant_type}'") +
                                            (string.IsNullOrEmpty(region_id) ? "" : $" AND b.REGION_ID = '{region_id}'") +
                                            " group by r.Region_NAME,b.BENEFICIARY_ID, g.TYPE_NAME,b.NAME,b.SURNAME , b.Paypoint " +
                                            " ORDER BY r.region_name";
                                break;
                            default:

                                break;
                        }

                        con.Open();
                        using (OracleDataReader reader = (OracleDataReader)await cmd.ExecuteReaderAsync())
                        {
                            reader.ToCsv(filename, header, reportFolder);
                        }
                    }
                    con.Close();
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task SaveReport(string rIndex, ReportPeriod from, ReportPeriod to, string regionId, string fileName)
        {
            ReportHeader header = new ReportHeader();
            header.FromDate = from.FromDate.ToShortDateString();
            header.ToDate = to.ToDate.ToShortDateString();
            int usernameIndex = fileName.IndexOf('-') + 1;
            header.Username = fileName.Substring(usernameIndex, fileName.Substring(usernameIndex).IndexOf("-"));//{_session.Office.RegionCode}-{_session.SamName.ToUpper()}-
            header.Region = StaticDataService.RegionName(regionId);

            switch (rIndex)
            {
                case "8"://Missing Summary
                    List<MissingFile> result = await _ogs.MissingProgress(from, to, regionId);
                    result.ToCsv<MissingFile>(fileName, header, reportFolder);
                    break;
                default:
                    break;
            }


        }

        public PagedResult<CsvListItem> GetFiles(string regionCode, string username, int page)
        {
            PagedResult<CsvListItem> result = new PagedResult<CsvListItem>();
            string[] files = new string[0];
            try
            {
                files = Directory.GetFiles(reportFolder, $"{regionCode}-{username.ToUpper()}*");
            }
            catch //(Exception ex)
            {

            }

            result.count = files.Count();
            foreach (string filePath in files)
            {
                result.result.Add(new CsvListItem(Path.GetFileName(filePath)));
            }
            //Page the result..
            result.result = result.result.OrderByDescending(r => r.ReportDate).Skip((page - 1) * 12).Take(12).ToList();
            return result;
        }

        public PagedResult<CsvListItem> GetTdwFiles(string regionCode, string username, int page)
        {
            PagedResult<CsvListItem> result = new PagedResult<CsvListItem>();
            string[] files = new string[0];
            try
            {
                files = Directory.GetFiles(reportFolder, $"{regionCode}-{username.ToUpper()}-TDW*");
            }
            catch //(Exception ex)
            {

            }

            result.count = files.Count();
            foreach (string filePath in files)
            {
                result.result.Add(new CsvListItem(Path.GetFileName(filePath)));
            }
            //Page the result..
            result.result = result.result.OrderByDescending(r => r.ReportDate).Skip((page - 1) * 12).Take(12).ToList();
            return result;
        }

        public PagedResult<CsvListItem> GetBoxHisTory(string regionCode, string username, int page)
        {
            PagedResult<CsvListItem> result = new PagedResult<CsvListItem>();
            string[] files = new string[0];
            try
            {
                files = Directory.GetFiles(reportFolder, $"{regionCode}-{username.ToUpper()}*");
            }
            catch //(Exception ex)
            {

            }

            result.count = files.Count();
            foreach (string filePath in files)
            {
                result.result.Add(new CsvListItem(Path.GetFileName(filePath)));
            }
            //Page the result..
            result.result = result.result.OrderByDescending(r => r.ReportDate).Skip((page - 1) * 12).Take(12).ToList();
            return result;
        }

        public void DeleteReport(string fileName)
        {
            File.Delete($"{reportFolder}\\{fileName}");
        }

        public async Task SaveHtmlReport(string data, string fileName)
        {
            string filename = $"{reportFolder}\\{fileName}.html";
            await File.WriteAllTextAsync(filename, data);
        }

    }


}
