//using DocumentFormat.OpenXml;
//using DocumentFormat.OpenXml.Packaging;
//using DocumentFormat.OpenXml.Spreadsheet;
using Oracle.ManagedDataAccess.Client;
using Sassa.BRM.Models;
using System.Data;
//using System.Data.Entity.Core.Objects;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Sassa.BRM.Helpers
{
    public static class Extentions
    {
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
        public static bool IsChildGrant(this string grant)
        {
            return "59C".Contains(grant.Trim().ToUpper());
        }

        public static bool IsNumeric(this string text)
        {
            double result;
            return double.TryParse(text, out result);
        }

        public static string Add60Days(this string date)
        {
            DateTime dt = DateTime.ParseExact(date.Replace("/", ""), "yyyyMMdd", CultureInfo.InvariantCulture);
            return dt.AddDays(60).ToString("yyyyMMdd");
        }

        public static string ToOracleDate(this string date)
        {
            string[] parts = date.Split('/');
            string monthpart = parts[0].PadLeft(3 - parts[0].Length, '0');
            string daypart = parts[1].PadLeft(3 - parts[0].Length, '0');
            string yearpart = parts[2].Substring(0, 4);

            return monthpart + "/" + daypart + "/" + yearpart;
        }

        public static DateTime? ToDate(this string date, string fromFormat)
        {
            if (string.IsNullOrEmpty(date)) return null;
            return DateTime.ParseExact(date, fromFormat, CultureInfo.InvariantCulture);
        }

        public static string ToStandardDateString(this DateTime? fromdate)
        {
            if (fromdate == null) return "";
            return ((DateTime)fromdate).ToString("dd/MMM/yy");
        }
        public static string ToStandardDateString(this string fromdate, string fromformat)
        {
            fromformat = fromformat.Replace("-", "/").Replace("mm", "MM"); ;
            fromdate = fromdate.Replace("-", "/");
            if (string.IsNullOrEmpty(fromdate)) return "";
            return ((DateTime)fromdate.ToDate(fromformat)!).ToString("dd/MMM/yy");
        }

        public static string ChangeDateStringFormat(this string fromdate, string fromformat, string toFormat)
        {
            if (string.IsNullOrEmpty(fromdate)) return "";
            return ((DateTime)fromdate.ToDate(fromformat)!).ToString(toFormat);
        }

        public static Decimal? ToDecimal(this string val)
        {
            decimal result;
            if (Decimal.TryParse(val, out result))
            {
                return result;
            }
            return null;
        }
        public static String ToEncodedString(this Stream stream, Encoding? enc = null)
        {
            enc = enc ?? Encoding.UTF8;

            byte[] bytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(bytes, 0, (int)stream.Length);
            string data = enc.GetString(bytes);

            return enc.GetString(bytes);
        }

        public static int ParseOrZero(this string source)
        {
            int result = 0;
            int.TryParse(source, out result);
            return result;

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
        public static void ToCsv(this IDataReader reader, string filename, string? path = null, string extension = "csv")
        {
            int nextResult = 0;
            do
            {
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
                }

                filename = string.Format("{0}-{1}", filename, ++nextResult);
            }
            while (reader.NextResult());
        }

        public static void ToCsv<T>(this List<T> list, string filename, string? path = null, string extension = "csv")
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
                writer.Flush();
            }

        }

        public static DataTable ToTable(this OracleDataReader reader)
        {
            DataTable files = new DataTable();
            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();
            DataColumn column;
            foreach (var col in columns)
            {
                column = new DataColumn();
                column.DataType = System.Type.GetType("System.String");
                column.ColumnName = col;
                files.Columns.Add(column);
            }

            while (reader.Read())
            {
                DataRow row = files.NewRow();
                foreach (var col in columns)
                {
                    row[col] = reader[col].ToString();
                }
                files.Rows.Add(row);
            }
            return files;
        }

        /// <summary>
        /// Formats "YYYYMMDD" to YYYY/MM/DD
        /// </summary>
        /// <param name="appdate"></param>
        /// <returns></returns>
        public static string FormatORDate(this string appdate)
        {
            string myYYYY = appdate.Substring(0, 4);
            string myMM = appdate.Substring(4, 2);
            string myDD = appdate.Substring(6, 2);

            return myYYYY + "/" + myMM + "/" + myDD;
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

    }
}
