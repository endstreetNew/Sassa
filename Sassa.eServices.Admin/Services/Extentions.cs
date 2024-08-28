using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sassa.eServices.Admin.Services
{
    public static class Extentions
    {

        public static string ToBase64(this String str)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
        }
        public static bool IsValidCellnumber(this String str)
        {
            //str= str.TrimStart('+');
            if (!str.StartsWith("0")) return false;
            if (str.Length < 10 || str.Length > 10) return false;
            if (!str.All(char.IsNumber)) return false;
            return true;
        }

        public static bool IsValidId(this string id)
        {
            id = id.Replace(" ", "");
            if (id.Length != 13) return false;
            //            For this explanation I am going to use ID number 8605065397083

            int OddSum = 0;
            int EvenSum = 0;
            int EvenStringSum = 0;
            int EvenOddSum = 0;
            StringBuilder sb = new StringBuilder();
            string EvenString = "";
            //a) Add all the even digits of the ID number in the odd positions(except for the last number, which is the control digit):
            //8 + 0 + 0 + 5 + 9 + 0 = 22
            for (int i = 0; i < 11; i++)
            {
                if (i % 2 != 0) continue;
                OddSum += int.Parse(id.Substring(i, 1));
            }
            //b) Take all the odd digits as one number and multiply that by 2:
            //656378 * 2 = 1312756
            for (int i = 0; i < 12; i++)
            {
                if (i % 2 == 0) continue;
                sb.Append(id.Substring(i, 1));
            }
            EvenSum = int.Parse(sb.ToString()) * 2;
            EvenString = EvenSum.ToString();
            //c) Add the digits of this number together(in b)
            //1 + 3 + 1 + 2 + 7 + 5 + 6 = 25
            for (int i = 0; i <= EvenString.Length - 1; i++)
            {
                EvenStringSum += int.Parse(EvenString.Substring(i, 1));
            }
            //d) Add the answer of C to the answer of A
            //22 + 25 = 47
            EvenOddSum = OddSum + EvenStringSum;
            //e) Subtract the second character from D from 10, this number should now equal the control character
            //10 - 7 = 3 = control character(3)
            if (10 - int.Parse(EvenOddSum.ToString().Substring(1, 1)) == int.Parse(id.Substring(12, 1)) || (int.Parse(EvenOddSum.ToString().Substring(1, 1)) == 0 && int.Parse(id.Substring(12, 1)) == 0))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// date from dd/MMM/yy 
        /// </summary>
        /// <param name="date"></param>
        /// <param name="fromFormat"></param>
        /// <returns></returns>
        public static DateTime? ToDate(this string date)
        {
            if (string.IsNullOrEmpty(date)) return null;
            return DateTime.ParseExact(date, "dd/MMM/yy", CultureInfo.InvariantCulture);
        }


        public static bool IsValidPdf(this byte[] bytes)
        {
            var header = new[] { bytes[0], bytes[1], bytes[2], bytes[3] };
            var isHeaderValid = header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46; //%PDF
            //var trailer = new[] { bytes[bytes.Length - 5], bytes[bytes.Length - 4], bytes[bytes.Length - 3], bytes[bytes.Length - 2], bytes[bytes.Length - 1] };
            //var isTrailerValid = trailer[0] == 0x25 && trailer[1] == 0x25 && trailer[2] == 0x45 && trailer[3] == 0x4f && trailer[4] == 0x46; //%%EOF
            return isHeaderValid;// && isTrailerValid;
        }

        public static List<T> ToList<T>(this DataTable table) where T : class, new()
        {
            try
            {
                List<T> list = new List<T>();
                foreach (var row in table.AsEnumerable())
                {
                    T obj = new T();
                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                            propertyInfo.SetValue(obj, Convert.ChangeType(row[prop.Name], propertyInfo.PropertyType), null);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    list.Add(obj);
                }
                return list;
            }
            catch
            {
                return null;
            }
        }

        public static string ToD(this double x) 
        { 
            return x.ToString().Replace(",",".");
        }
    }
}
