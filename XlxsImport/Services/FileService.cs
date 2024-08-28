using XlsxImport.Services;

namespace XlxsImport.Services
{
    public class FileService
    {

        RawSqlService _raw;
        XlsxHelper _import;
        public FileService(RawSqlService raw, XlsxHelper import)
        {
            _raw = raw;
            _import = import;
        }

        public async Task ImportXlsxFile(string fileName, string tableName)
        {
            //_import.WriteAuditList(fileName, "EC","A");
            //_import.WriteAuditList(fileName, "KZN", "C");
            //_import.WriteAuditList(fileName, "WC|NC|FS|NW|LIM|MPU|GAU", "C");
            _import.WriteAuditList(fileName, "GAU", "C");
            //foreach (var item in AuditList)
            //{
            //    //await _raw.ExecuteNonQuery($"Update DC_File set Application_Status = 'DESTROY' where Applicant_no = '{item}'");
            //    //await _raw.ExecuteNonQuery($"Update dc_socpen set status_code = 'DESTROY' where Beneficiary_id = '{item}'");

            //    await _raw.ExecuteNonQuery($"INSERT INTO AUDITTEMP (ID, Region) VALUES ('{6110075059080}', 'Region')");

            //}
        }
    }
}