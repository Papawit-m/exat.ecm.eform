using System.Data;
using System.Reflection;

namespace EXAT.ECM.EON.API.Models
{
    public class Utilities
    {
        private readonly IConfiguration Configuration;
        public Utilities(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static string AsposeFontsPath { get; set; }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DateTime _date_start = DateTime.Now;

            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
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
    }
}
