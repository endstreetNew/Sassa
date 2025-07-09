using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Sassa.XlsxService
{
    public static class XlsxHelper
    {
        /// <summary>
        ///  Read .xlsx destroy list from the specified column in an Excel file.
        /// </summary>
        public static List<string> ReadDestroyList(string fileName, string targetColumnName = "A")
        {
            var DestroyList = new List<string>();

            // Open the Excel file
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
            {
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart!;
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

                // Specify the column you want to read (e.g., column A)
                string targetColumn = "A"; // Change this to the desired column letter


                foreach (Row row in sheetData.Elements<Row>())
                {
                    foreach (Cell cell in row.Elements<Cell>())
                    {
                        // Check if the cell belongs to the target column
                        if (GetColumnName(cell.CellReference!.Value!) == targetColumn)
                        {
                            string cellValue = GetCellValue(workbookPart, cell);
                            if (cellValue.Trim().Length == 13) // Add your condition here (e.g., check if the cell value is not empty
                            {
                                DestroyList.Add(cellValue);
                            }
                        }
                    }
                }
            }
            return DestroyList;
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
                SharedStringTablePart sharedStringPart = workbookPart.SharedStringTablePart!;
                return sharedStringPart.SharedStringTable.ElementAt(int.Parse(cell.CellValue!.Text)).InnerText;
            }
            else
            {
                return cell.CellValue!.Text;
            }
        }
        public static void ConvertCsvToXlsx(List<string> lines, string xlsxPath)
        {
            using var spreadsheet = SpreadsheetDocument.Create(xlsxPath, SpreadsheetDocumentType.Workbook);
            var workbookPart = spreadsheet.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();
            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(sheetData);

            var sheets = spreadsheet.WorkbookPart!.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Sheet1"
            });

            foreach (var line in lines)
            {
                var row = new Row();
                var cells = line.Split(',');
                foreach (var cell in cells)
                {
                    row.Append(new Cell
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(cell)
                    });
                }
                sheetData.Append(row);
            }

            workbookPart.Workbook.Save();
        }
    }
}
