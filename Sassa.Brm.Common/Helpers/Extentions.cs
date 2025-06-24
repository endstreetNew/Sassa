using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Spreadsheet;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Data.ViewModels;
using Sassa.BRM.Models;
using System.Data;
//using System.Data.Entity.Core.Objects;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Path = System.IO.Path;

namespace Sassa.Brm.Common.Helpers
{
    public static class Extentions
    {
        public static List<KeyValue> ToKeyValueList(this Dictionary<string, string> dictionary)
        {
            var grantTypes = new List<KeyValue>();

            foreach (var grantType in dictionary)
            {
                grantTypes.Add(new KeyValue
                {
                    Id = grantType.Key,
                    Name = grantType.Value
                });
            }

            return grantTypes;
        }
        public static List<KeyValue> ToKeyValueList(this Dictionary<decimal, string> dictionary)
        {
            var grantTypes = new List<KeyValue>();

            foreach (var grantType in dictionary)
            {
                grantTypes.Add(new KeyValue
                {
                    Id = grantType.Key.ToString(),
                    Name = grantType.Value
                });
            }

            return grantTypes;
        }
        public static DcFile FromDcFile(this DcFile toFile, DcFile fromFile)
        {
            toFile.AltBoxNo = fromFile.AltBoxNo;
            toFile.MisReboxDate = fromFile.MisReboxDate;
            toFile.MisReboxStatus = fromFile.MisReboxStatus;
            toFile.TdwBoxno = fromFile.TdwBoxno;
            toFile.TdwBoxTypeId = fromFile.TdwBoxTypeId;
            toFile.TdwBatch = fromFile.TdwBatch;
            toFile.TdwBoxArchiveYear = fromFile.TdwBoxArchiveYear;
            toFile.MiniBoxno = fromFile.MiniBoxno;
            toFile.AltBoxNo = fromFile.AltBoxNo;
            toFile.FileComment = fromFile.FileComment;
            toFile.PrintOrder = fromFile.PrintOrder;
            return toFile;
        }
        public static DcExclusion FromCsv(string csvLine, string etype, string eregion, string euser)
        {
            //string[] values = csvLine.Split(',');
            DcExclusion exclusion = new DcExclusion
            {
                ExclusionType = etype,
                ExclDate = DateTime.Now,
                IdNo = csvLine,
                RegionId = int.Parse(eregion),
                Username = euser
            };
            return exclusion;
        }
        //public static bool IsChildGrant(this string grant)
        //{
        //    return "59C".Contains(grant.Trim().ToUpper());
        //}
        public static bool IsNumeric(this string text)
        {
            double result;
            return double.TryParse(text, out result);
        }
        /// <summary>
        /// Used for PieChart Calculations
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string ToD(this double x)
        {
            return x.ToString().Replace(",", ".");
        }
        //public static string Add60Days(this string date)
        //{
        //    DateTime dt = DateTime.ParseExact(date.Replace("/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
        //    return dt.AddDays(60).ToString("yyyyMMdd");
        //}

        //public static string ToOracleDate(this string date)
        //{
        //    string[] parts = date.Split('/');
        //    string monthpart = parts[0].PadLeft(3 - parts[0].Length, '0');
        //    string daypart = parts[1].PadLeft(3 - parts[0].Length, '0');
        //    string yearpart = parts[2].Substring(0, 4);

        //    return monthpart + "/" + daypart + "/" + yearpart;
        //}

        public static DateTime ToDate(this string date, string fromFormat)
        {
            if (string.IsNullOrEmpty(date)) return DateTime.Now;
            return DateTime.ParseExact(date, fromFormat, CultureInfo.InvariantCulture);
        }

        public static string ToStandardDateString(this DateTime fromdate)
        {
            return ((DateTime)fromdate).ToString("dd/MMM/yy");
        }

        public static string ToCSV(this DataTable dtDataTable)
        {
            StringBuilder sb = new StringBuilder();
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            //StreamWriter sw = new StreamWriter(strFilePath, false);
            //headers  
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sb.Append(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append(Environment.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString()!.Trim();
                        if (value.Contains(","))
                        {
                            value = String.Format("\"{0}\"", value);

                            sb.Append(value);
                        }
                        else
                        {

                            sb.Append(rgx.Replace(dr[i].ToString()!.Trim(), ""));
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sb.Append(",");
                    }
                }
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="filename"></param>
        /// <param name="path">if null/empty will use IO.Path.GetTempPath()</param>
        /// <param name="extension">will use csv by default</param>
        public static void ToCsv(this IDataReader reader, string filename, ReportHeader header, string? path = null, string extension = "csv")
        {
            //Username, Region, Date Range and number of application
            //int nextResult = 0;
            //do
            //{
                var filePath = Path.Combine(string.IsNullOrEmpty(path) ? Path.GetTempPath() : path, string.Format("{0}.{1}", filename, extension));
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine(string.Join(",", Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList()));

                    int count = 0;
                    while (reader.Read())
                    {
                        writer.WriteLine(string.Join(",", Enumerable.Range(0, reader.FieldCount).Select(reader.GetValue).ToList()));
                        if (++count % 100 == 0)
                        {
                            writer.Flush();
                        }
                    }
                    writer.WriteLine($"");
                    writer.WriteLine($"{header.Region}|{header.Username}|{header.FromDate}|{header.ToDate}|{count}");
                }

                //filename = string.Format("{0}-{1}", filename, ++nextResult);
            //}
            //while (reader.NextResult());
        }
        public static void ToXlsx(this IDataReader reader, string filename, ReportHeader header, string? path = null, string extension = "xlsx")
        {
            //int nextResult = 0;
            List<string> lines = new();
            int count = 0;
            do
            {
                lines.Add(string.Join(",", Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList()));
                while (reader.Read())
                {
                    lines.Add(string.Join(",", Enumerable.Range(0, reader.FieldCount).Select(reader.GetValue).ToList()));
                }
                count++;
            }
            while (reader.NextResult()) ;
            //Username, Region, Date Range and number of application
            lines.Add($"");
            lines.Add($"{header.Region}|{header.Username}|{header.FromDate}|{header.ToDate}|{count}");
            var filePath = Path.Combine(string.IsNullOrEmpty(path) ? Path.GetTempPath() : path, string.Format("{0}.{1}", filename, extension));
            //filename = string.Format("{0}-{1}", filename, ++nextResult);
            XlsxHelper.ConvertCsvToXlsx(lines, filePath);
        }


        public static void ToXlsx<T>(this List<T> list,string fileName, string extension = "xlsx")
        {
            var properties = typeof(T).GetProperties().Where(p => !(p.GetGetMethod()?.IsVirtual ?? false)).ToList();

            int idx = 0;
            int lastIdx = properties.Count - 1;
            StringBuilder line = new StringBuilder();
            foreach (var prop in properties)
            {
                bool isLast = (idx == lastIdx);
                // Use isLast as needed
                idx++;
                if (isLast)
                {
                    line.Append(prop.Name + Environment.NewLine);
                }
                else
                {
                    line.Append(prop.Name + ",");
                }
            }
            List<string> lines = new();
            lines.Add(line.ToString());
            int count = 0;
            
            foreach (var item in list)
            {
                line = new StringBuilder();
                idx = 0;
                foreach (var prop in properties)
                {
                    bool isLast = (idx == lastIdx);
                    // Use isLast as needed
                    idx++;
                    if (isLast)
                    {
                        line.Append(prop.GetValue(item) + Environment.NewLine);
                    }
                    else
                    {
                        line.Append(prop.GetValue(item) + ",");
                    }
                }
                lines.Add(line.ToString());
                count++;
            }
            lines.Add($"");
            var filePath = Path.Combine(string.IsNullOrEmpty(StaticDataService.ReportFolder) ? Path.GetTempPath() : StaticDataService.ReportFolder, string.Format("{0}.{1}", fileName, extension));
            XlsxHelper.ConvertCsvToXlsx(lines, filePath);
        }
        public static void ToXlsx<T>(this List<T> list, string filename, ReportHeader header, string? path = null, string extension = "xlsx")
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            //Create Header
            StringBuilder line = new StringBuilder();
            for (int i = 0; i < properties.Length - 1; i++)
            {
                line.Append(properties[i].Name + ",");
            }
            var lastProp = properties[properties.Length - 1].Name;
            line.Append(lastProp + Environment.NewLine);

            List<string> lines = new();
            lines.Add(line.ToString());
            int count = 0;

            foreach(var item in list)
            {
                line = new StringBuilder();
                for (int i = 0; i < properties.Length - 1; i++)
                {
                    var prop = properties[i];
                    line.Append(prop.GetValue(item) + ",");
                }
                var llastProp = properties[properties.Length - 1];
                line.Append(llastProp.GetValue(item) + Environment.NewLine);
                lines.Add(line.ToString());
                count++;
            }
            lines.Add($"");
            lines.Add($"{header.Region}|{header.Username}|{header.FromDate}|{header.ToDate}|{count}");
            var filePath = Path.Combine(string.IsNullOrEmpty(path) ? Path.GetTempPath() : path, string.Format("{0}.{1}", filename, extension));
            XlsxHelper.ConvertCsvToXlsx(lines, filePath);
        }
        public static void ToCsv<T>(this List<T> list, string filename, ReportHeader header, string? path = null, string extension = "csv")
        {
            var filePath = Path.Combine(string.IsNullOrEmpty(path) ? Path.GetTempPath() : path, string.Format("{0}.{1}", filename, extension));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                //CreateHeader
                PropertyInfo[] properties = typeof(T).GetProperties();
                for (int i = 0; i < properties.Length - 1; i++)
                {
                    writer.Write(properties[i].Name + ",");
                }
                var lastProp = properties[properties.Length - 1].Name;
                writer.Write(lastProp + writer.NewLine);
                //This method creates all the value rows


                foreach (var item in list)
                {
                    properties = typeof(T).GetProperties();
                    for (int i = 0; i < properties.Length - 1; i++)
                    {
                        var prop = properties[i];
                        writer.Write(prop.GetValue(item) + ",");
                    }
                    var llastProp = properties[properties.Length - 1];
                    writer.Write(llastProp.GetValue(item) + writer.NewLine);
                }

                writer.WriteLine($"");
                writer.WriteLine($"{header.Region}|{header.Username}|{header.FromDate}|{header.ToDate}|{list.Count}");
                writer.Flush();
            }

        }

        public static string GetDigitId(this string id)
        {
            StringBuilder SB = new StringBuilder();
            foreach (var c in id.ToCharArray())
            {
                if (c.ToString().IsNumeric())
                {
                    SB.Append(c);
                }
            }
            var idno = SB.ToString();
            if (idno.Length != 13) throw new Exception("Invalid ID No.");
            return idno;
        }

        private static void CreateHeader<T>(List<T> list, StringBuilder sw)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            for (int i = 0; i < properties.Length - 1; i++)
            {
                sw.Append(properties[i].Name + ",");
            }
            var lastProp = properties[properties.Length - 1].Name;
            sw.Append(lastProp + Environment.NewLine);
        }

        private static void CreateRows<T>(List<T> list, StringBuilder sw)
        {
            foreach (var item in list)
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                for (int i = 0; i < properties.Length - 1; i++)
                {
                    var prop = properties[i];
                    sw.Append(prop.GetValue(item) + ",");
                }
                var lastProp = properties[properties.Length - 1];
                sw.Append(lastProp.GetValue(item) + Environment.NewLine);
            }
        }


        public static string CreateCSV<T>(this List<T> list)
        {
            StringBuilder sb = new StringBuilder();

            CreateHeader(list, sb);
            CreateRows(list, sb);

            return sb.ToString();

        }

        //public static string CreateXLSX<T>(this List<T> list, string worksheetName)//, IEnumerable<string[]> csvLines)
        //{
        //    IEnumerable<string[]> csvLines = list.CreateCSVAR<T>().AsEnumerable();

        //    if (csvLines == null || csvLines.Count() == 0)
        //    {
        //        return "";
        //    }
        //    var r = CreateSpreadSheet(csvLines, worksheetName);

        //    return r;

        //    //MemoryStream memoryStream = new MemoryStream();
        //    // using (SpreadsheetDocument package = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook, true))
        //    // {
        //    //     package.AddWorkbookPart();
        //    //     package.WorkbookPart.Workbook = new Workbook();
        //    //     package.WorkbookPart.AddNewPart<WorksheetPart>();
        //    //     SheetData xlSheetData = new SheetData();
        //    //     foreach (var line in csvLines)
        //    //     {
        //    //         Row xlRow = new Row();
        //    //         string Cvalue;
        //    //         foreach (var col in line)
        //    //         {
        //    //             Cvalue = "";
        //    //             if (col != null) Cvalue = col.ToString();
        //    //             Cell xlCell = new Cell(new InlineString(new Text(Cvalue))) { DataType = CellValues.InlineString };
        //    //             xlRow.Append(xlCell);
        //    //         }
        //    //         xlSheetData.Append(xlRow);
        //    //     }
        //    //     package.WorkbookPart.WorksheetParts.First().Worksheet = new Worksheet(xlSheetData);
        //    //     package.WorkbookPart.WorksheetParts.First().Worksheet.Save();


        //    //     // create the worksheet to workbook relation
        //    //     package.WorkbookPart.Workbook.AppendChild(new Sheets());
        //    //     package.WorkbookPart.Workbook.GetFirstChild<Sheets>().AppendChild(new Sheet()
        //    //     {
        //    //         Id = package.WorkbookPart.GetIdOfPart(package.WorkbookPart.WorksheetParts.First()),

        //    //         SheetId = 1,

        //    //         Name = worksheetName

        //    //     });

        //    //     package.WorkbookPart.Workbook.Save();
        //    //     package.Close();
        //    // }
        //    // using (StreamReader reader = new StreamReader(memoryStream))
        //    // {
        //    //     memoryStream.Position = 0;
        //    //     return reader.ReadToEnd();
        //    // }
        //    // return Encoding.UTF8.GetString(memoryStream.ToArray());

        //    // the stream is complete here
        //    //AsMemoryStream = new MemoryStream();
        //    //documentStream.CopyTo(AsMemoryStream);
        //}

        private static void CreateHeaderAR<T>(List<T> list, string[] sw, PropertyInfo[] properties)
        {

            for (int i = 0; i < properties.Length - 1; i++)
            {
                sw[i] = properties[i].Name;
            }
            var lastProp = properties[properties.Length - 1].Name;
            sw[properties.Length - 1] = lastProp + Environment.NewLine;
        }


        //private static void CreateRowsAR<T>(List<T> list, string[] sw, PropertyInfo[] properties)
        //{
        //    foreach (var item in list)
        //    {

        //        for (int i = 0; i < properties.Length - 1; i++)
        //        {
        //            var prop = properties[i];
        //            sw[i] = (string)prop.GetValue(item);
        //        }
        //        var lastProp = properties[properties.Length - 1];
        //        sw[properties.Length - 1] = (string)lastProp.GetValue(item) + Environment.NewLine;
        //    }
        //}


        //public static IEnumerable<string[]> CreateCSVAR<T>(this List<T> list)
        //{
        //    PropertyInfo[] properties = typeof(T).GetProperties();
        //    List<string[]> result = new List<string[]>();
        //    string[] sb = new string[properties.Length];

        //    CreateHeaderAR(list, sb, properties);
        //    result.Add(sb);
        //    foreach (var item in list)
        //    {
        //        sb = new string[properties.Length];
        //        for (int i = 0; i < properties.Length - 1; i++)
        //        {
        //            var prop = properties[i];
        //            sb[i] = (string)prop.GetValue(item);
        //        }
        //        var lastProp = properties[properties.Length - 1];
        //        sb[properties.Length - 1] = (string)lastProp.GetValue(item) + Environment.NewLine;
        //        result.Add(sb);
        //    }

        //    return result;

        //}
        //public static string CreateSpreadSheet(IEnumerable<string[]> csvLines, string worksheetName)
        //{
        //    MemoryStream mem = new MemoryStream();
        //    SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.
        //            Create(mem, SpreadsheetDocumentType.Workbook);

        //    WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
        //    workbookpart.Workbook = new Workbook();
        //    spreadsheetDocument.WorkbookPart.AddNewPart<WorksheetPart>();
        //    SheetData xlSheetData = new SheetData();
        //    foreach (var line in csvLines)
        //    {
        //        Row xlRow = new Row();
        //        string Cvalue;
        //        foreach (var col in line)
        //        {
        //            Cvalue = "";
        //            if (col != null) Cvalue = col.ToString();
        //            Cell xlCell = new Cell(new InlineString(new Text(Cvalue))) { DataType = CellValues.InlineString };
        //            xlRow.Append(xlCell);
        //        }
        //        xlSheetData.Append(xlRow);
        //    }
        //    spreadsheetDocument.WorkbookPart.WorksheetParts.First().Worksheet = new Worksheet(xlSheetData);
        //    spreadsheetDocument.WorkbookPart.WorksheetParts.First().Worksheet.Save();


        //    // create the worksheet to workbook relation
        //    spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
        //    spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().AppendChild(new Sheet()
        //    {
        //        Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(spreadsheetDocument.WorkbookPart.WorksheetParts.First()),

        //        SheetId = 1,

        //        Name = worksheetName

        //    });

        //    spreadsheetDocument.WorkbookPart.Workbook.Save();
        //    spreadsheetDocument.Dispose();

        //    // Close the document.
        //    return Encoding.UTF8.GetString(mem.ToArray());
        //    //return mem;
        //}

        public static string ApplicationStatusFromMIS(this string misAS)
        {
            switch (misAS)
            {
                case "Main File":
                    return "MAIN";
                case "Main LC:":
                    return "LC-MAIN";
                case "Archive File":
                case "NULL":
                case null:
                    return "ARCHIVE";
                case "Archive LC":
                    return "LC-ARCHIVE";
                case "Special":
                    return "SPECIAL";
                case "Special LC":
                    return "SPECIAL";
                default:
                    return "ARCHIVE";
            }
        }
        public static string GrantTypeFromMIS(this string misGrantType)
        {
            if (string.IsNullOrEmpty(misGrantType))
            {
                return string.Empty;
            }

            switch (misGrantType)
            {
                case "1": return "0";
                case "2": return "1";
                case "10": return "C";
                case "11": return "S";
                case "O": return "0";
                default: return misGrantType;
            }
        }



        //private static string GetImageString(string imgpath)
        //{
        //    using (Image img = Image.FromFile(imgpath))
        //    {
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            img.Save(ms, img.RawFormat);
        //            byte[] imgBytes = ms.ToArray();
        //            return Convert.ToBase64String(imgBytes);
        //        }
        //    }
        //}

        //public static IEnumerable<TSource> DistinctBy<TSource, TKey> (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        //{
        //    HashSet<TKey> seenKeys = new HashSet<TKey>();
        //    foreach (TSource element in source)
        //    {
        //        if (seenKeys.Add(keySelector(element)))
        //        {
        //            yield return element;
        //        }
        //    }
        //}


    }
}
