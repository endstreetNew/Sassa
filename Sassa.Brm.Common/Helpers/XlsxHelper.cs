using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.Linq;

namespace Sassa.Brm.Common.Helpers
{

    public static class XlsxHelper
    {
        /// <summary>
        ///  This is a demo to read the previously created basix.xlsx file
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


        public static IEnumerable<string> GetColumnNames(string filePath)
        {
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart!;
                var sheet = workbookPart!.Workbook.Descendants<Sheet>().First();
                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);

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
