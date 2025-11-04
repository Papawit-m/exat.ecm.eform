using System.Data;
using System.Reflection;
using System.Text;
using System.Web;

namespace EXAT.ECM.FED.API.Models
{
    public class Utilities
    {
        private readonly IConfiguration Configuration;
        public Utilities(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static string AsposeFontsPath { get; set; }

        public static DataTable ToDataTable<T>(List<T> items)
        {
            DateTime _date_start = DateTime.Now;

            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                if (!dataTable.Columns.Contains(prop.Name)) // ✅ เช็กชื่อซ้ำก่อน
                {
                    //Setting column names as Property names
                    dataTable.Columns.Add(prop.Name);
                }
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public static T? ConvertValue<T>(string value) where T : struct
        {
            object? result = null;
            string type = typeof(T).Name;

            if (!string.IsNullOrEmpty(value))
            {
                value = value.Trim().ToUpper();
            }

            try
            {
                switch (type)
                {
                    case "Boolean": result = string.Format("{0}", value) == "X" ? true : Convert.ToBoolean(value); break;
                    case "Decimal": result = Convert.ToDecimal(value); break;
                    case "Int32": result = Convert.ToInt32(value); break;
                    case "DateTime":
                        {
                            if (string.Format("{0}", value).Contains("/"))
                                result = DateTime.ParseExact(string.Format("{0}", value), "dd/MM/yyyy", System.Globalization.CultureInfo.CurrentCulture);
                            else if (string.Format("{0}", value).Contains("-"))
                                result = DateTime.ParseExact(string.Format("{0}", value), "dd-MM-yyyy", System.Globalization.CultureInfo.CurrentCulture);
                            else if (string.Format("{0}", value).Length == 8)
                                result = DateTime.ParseExact(string.Format("{0}", value), "ddMMyyyy", System.Globalization.CultureInfo.CurrentCulture);
                            else
                                result = DateTime.ParseExact(string.Format("{0}", value), "dd-MM-yyyy", System.Globalization.CultureInfo.CurrentCulture);
                            break;
                        }
                        //case "Guid": result = string.IsNullOrEmpty(string.Format("{0}", value)) ? null : (Guid?)new Guid(string.Format("{0}", value)); break;
                        //case "Object":break;
                        //case "String":break;
                }
            }
            catch (Exception)
            {
                switch (type)
                {
                    case "Boolean": result = false; break;
                    case "Decimal": result = 0; break;
                    case "Int32": result = 0; break;
                }
            }
            return (T?)result;
        }

        public static string? CleansingData(string? value)
        {
            string? result = null;
            if (!string.IsNullOrEmpty(value))
            {
                result = value.Trim().ToUpper();
            }
            return result;

        }
        public static class ClobBinaryDecoder
        {
            private static readonly char[] TrimChars = new[] { '\r', '\n', '\t', ' ' };

            public static bool TryDecodeBase64Flexible(string? input, out byte[] bytes, out string? error)
            {
                bytes = Array.Empty<byte>();
                error = null;

                if (string.IsNullOrWhiteSpace(input))
                {
                    error = "CLOB is null/empty.";
                    return false;
                }

                try
                {
                    // 1) URL-decode เผื่อส่งผ่าน query/form โดยไม่ encode
                    var s = HttpUtility.UrlDecode(input) ?? input;

                    // 2) ตัด prefix data:*;base64, ถ้ามี
                    if (s.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                    {
                        var idx = s.IndexOf(',');
                        if (idx >= 0) s = s[(idx + 1)..];
                    }

                    // 3) Trim และลบ whitespace แทรก
                    s = s.Trim(TrimChars);
                    var sb = new StringBuilder(s.Length);
                    foreach (var c in s)
                    {
                        if (c == '\r' || c == '\n' || c == '\t' || c == ' ')
                            continue;
                        sb.Append(c);
                    }
                    s = sb.ToString();

                    // 4) URL-safe → standard
                    s = s.Replace('-', '+').Replace('_', '/');

                    // 5) เติม padding
                    var mod4 = s.Length % 4;
                    if (mod4 == 1)
                    {
                        error = "Invalid base64 length %4 == 1";
                        return false;
                    }
                    if (mod4 > 0) s = s.PadRight(s.Length + (4 - mod4), '=');

                    #if NET5_0_OR_GREATER
                    var maxLen = (s.Length * 3) / 4;
                    bytes = new byte[maxLen];
                    if (Convert.TryFromBase64String(s, bytes, out var written))
                    {
                        if (written != bytes.Length) Array.Resize(ref bytes, written);
                        return true;
                    }
                    error = "TryFromBase64String failed.";
                    bytes = Array.Empty<byte>();
                    return false;
                    #else
                                bytes = Convert.FromBase64String(s);
                                return true;
                    #endif
                }
                catch (Exception ex)
                {
                    error = $"Base64 decode threw: {ex.Message}";
                    bytes = Array.Empty<byte>();
                    return false;
                }
            }
        }


    }
}
