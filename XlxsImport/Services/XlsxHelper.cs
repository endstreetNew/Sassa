using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace XlsxImport.Services
{

    public class XlsxHelper
    {

        string brmcs;
        public XlsxHelper(IConfiguration config)
        {
            brmcs = config.GetConnectionString("BrmConnection");
        }
        /// <summary>
        ///  This is a demo to read the previously created basix.xlsx file
        /// </summary>
        public async Task ReadAuditList(string fileName, string targetColumnName = "C")
        {

            // Open the Excel file
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

                // Specify the column you want to read (e.g., column A)
                string targetColumn = "C"; // Change this to the desired column letter
                using (OracleConnection connection = new OracleConnection(brmcs))
                {
                    OracleCommand command = new OracleCommand("", connection);
                    command.Connection.Open();
                    foreach (var sheet in workbookPart.Workbook.Descendants<Sheet>().Where(s => "EC|WC|NC|FS|NW|LIM|MPU|GAU".Contains(s.Name)))
                    {
                        var worksheetPart1 = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                        var sheetData1 = worksheetPart1.Worksheet.Elements<SheetData>().First();
                        foreach (Row row in sheetData1.Elements<Row>())
                        {
                            foreach (Cell cell in row.Elements<Cell>())
                            {
                                // Check if the cell belongs to the target column
                                if (GetColumnName(cell.CellReference.Value) == targetColumn)
                                {
                                    string cellValue = GetCellValue(workbookPart, cell);
                                    if (cellValue.Trim().Length == 13) // Add your condition here (e.g., check if the cell value is not empty
                                    {
                                        command.CommandText = $"INSERT INTO AUDITTEMP (ID, Region) VALUES ('{cellValue}', '{sheet.Name}')";
                                        await command.ExecuteNonQueryAsync();
                                        //await _raw.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public void WriteAuditList(string fileName, string targetRegions, string targetColumn)
        {
            int rowCounter;
            // Open the Excel file
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

                // Specify the column you want to read (e.g., column A)
                using (OracleConnection connection = new OracleConnection(brmcs))
                {
                    //OracleCommand command = new OracleCommand("", connection);
                    //command.Connection.Open();
                    foreach (var sheet in workbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == "EXTRAGAU")) //targetRegions.Contains(s.Name)))
                    {
                        DataTable dataTable = new DataTable("AuditTemp");
                        dataTable.Columns.Add("ID", typeof(string));
                        dataTable.Columns.Add("Region", typeof(string));
                        var worksheetPart1 = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                        var sheetData1 = worksheetPart1.Worksheet.Elements<SheetData>().First();
                        rowCounter = 0;
                        foreach (Row row in sheetData1.Elements<Row>())
                        {
                            foreach (Cell cell in row.Elements<Cell>())
                            {
                                // Check if the cell belongs to the target column
                                if (GetColumnName(cell.CellReference.Value) == targetColumn)
                                {
                                    string cellValue = GetCellValue(workbookPart, cell);
                                    if (cellValue.Trim().Length == 13) // Add your condition here (e.g., check if the cell value is not empty
                                    {
                                        //command.CommandText = $"INSERT INTO AUDITTEMP (ID, Region) VALUES ('{cellValue}', '{sheet.Name}')";
                                        //await command.ExecuteNonQueryAsync();
                                        DataRow dr = dataTable.NewRow();
                                        dr["ID"] = cellValue;
                                        dr["Region"] = "GAU";
                                        dataTable.Rows.Add(dr);
                                        rowCounter++;
                                    }
                                }
                            }
                            if (rowCounter > 999)
                            {
                                connection.Open();
                                OracleBulkCopy objbulk = new OracleBulkCopy(connection);
                                //assign Destination table name
                                objbulk.DestinationTableName = "AUDITTEMP";
                                objbulk.ColumnMappings.Add("ID", "ID");
                                objbulk.ColumnMappings.Add("Region", "REGION");
                                //insert bulk Records into DataBase.
                                objbulk.WriteToServer(dataTable);
                                connection.Close();
                                rowCounter = 0;
                                dataTable.Clear();
                            }
                        }
                    }
                }
            }
        }
        // Helper method to get the column name from the cell reference (e.g., "A1" -> "A")
        private static string GetColumnName(string cellReference)
        {
            return new string(cellReference.TrimEnd("0123456789".ToCharArray()));
        }

        // Helper method to get the actual cell value (handles shared strings, numeric values, etc.)
        private static string GetCellValue(WorkbookPart workbookPart, Cell cell)
        {
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                SharedStringTablePart sharedStringPart = workbookPart.SharedStringTablePart;
                return sharedStringPart.SharedStringTable.ElementAt(int.Parse(cell.CellValue.Text)).InnerText;
            }
            else
            {
                return cell.CellValue.Text;
            }
        }


        public static IEnumerable<string> GetColumnNames(string filePath)
        {
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                var sheet = workbookPart.Workbook.Descendants<Sheet>().First();
                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);

                // Get the first row (assumed to contain column headers)
                var firstRow = worksheetPart.Worksheet.Descendants<Row>().FirstOrDefault();
                if (firstRow != null)
                {
                    return firstRow.Elements<Cell>().Select(cell => GetCellValue(workbookPart, cell));
                }

                return Enumerable.Empty<string>();
            }
        }

        //private static string GetCellValue(WorkbookPart workbookPart, Cell cell)
        //{
        //    // Your logic to handle cell values (e.g., shared strings, numeric values, etc.)
        //    // Implement this method based on your specific requirements
        //    // ...
        //    return cell.InnerText;
        //}


    }
}
