using Aspose.Words.Replacing;
using Aspose.Words.Tables;
using Aspose.Words;
using System.Data;
using System.Reflection;

namespace EXAT.ECM.LCI.API.Helper
{

    #region ReplaceWords
    public class ReplaceWords
    {
        public void ReplaceNodeText(Node node, Dictionary<string, ReplaceObject> replaceObj)
        {
            if (replaceObj == null) return;

            FindReplaceOptions defaultOptions = new FindReplaceOptions();
            defaultOptions.MatchCase = false;
            defaultOptions.FindWholeWordsOnly = false;

            FindReplaceOptions defaultHtmlOptions = new FindReplaceOptions();
            defaultHtmlOptions.MatchCase = false;
            defaultHtmlOptions.FindWholeWordsOnly = false;
            defaultHtmlOptions.ReplacingCallback = new ReplaceWithHtml(defaultHtmlOptions);

            if (node != null && replaceObj != null && replaceObj.Count > 0)
            {
                foreach (string keyname in replaceObj.Keys)
                {
                    // Key
                    node.Range.Replace("[" + keyname + "]"
                                // value
                                , (replaceObj[keyname] != null && replaceObj[keyname].Value != null ? (!string.IsNullOrEmpty(replaceObj[keyname].Format) ? string.Format("{0:" + replaceObj[keyname].Format + "}", replaceObj[keyname].Value) : Convert.ToString(replaceObj[keyname].Value)) : "")
                                // option
                                , (replaceObj[keyname].Options != null ? replaceObj[keyname].Options : (replaceObj[keyname].IsHtml ? defaultHtmlOptions : defaultOptions)));
                }
            }
        }
        public void ReplaceNodeText(Node node, List<Dictionary<string, ReplaceObject>> replaceObjs)
        {
            if (replaceObjs == null) return;

            FindReplaceOptions defaultOptions = new FindReplaceOptions();
            defaultOptions.MatchCase = false;
            defaultOptions.FindWholeWordsOnly = false;

            FindReplaceOptions defaultHtmlOptions = new FindReplaceOptions();
            defaultHtmlOptions.MatchCase = false;
            defaultHtmlOptions.FindWholeWordsOnly = false;
            defaultHtmlOptions.ReplacingCallback = new ReplaceWithHtml(defaultHtmlOptions);

            if (node != null && replaceObjs != null && replaceObjs.Count > 0)
            {
                foreach (var replaceObj in replaceObjs)
                {
                    foreach (string keyname in replaceObj.Keys)
                    {
                        // Key
                        node.Range.Replace("[" + keyname + "]"
                                    // value
                                    , (replaceObj[keyname] != null && replaceObj[keyname].Value != null ? (!string.IsNullOrEmpty(replaceObj[keyname].Format) ? string.Format("{0:" + replaceObj[keyname].Format + "}", replaceObj[keyname].Value) : Convert.ToString(replaceObj[keyname].Value)) : "")
                                    // option
                                    , (replaceObj[keyname].Options != null ? replaceObj[keyname].Options : (replaceObj[keyname].IsHtml ? defaultHtmlOptions : defaultOptions)));
                    }
                }
            }
        }
        public void ReplaceNodeDataRow(Node node, string bookMarkName, List<Dictionary<string, ReplaceObject>> replaceObjs)
        {
            Bookmark bm = node.Range.Bookmarks[bookMarkName];

            bool isInBookmark = bm != null;
            if (isInBookmark)
            {
                Table itemTable = (Table)bm.BookmarkStart.GetAncestor(NodeType.Table);
                bool isInTable = itemTable != null; //Date 01/07/2024 piyanuch.n  debug null.
                if (isInTable)
                {
                    // เพิ่มตรงนี้เพื่อให้หัวคอลัมน์แสดงซ้ำทุกหน้า
                    if (itemTable.Rows.Count > 0)
                        itemTable.FirstRow.RowFormat.HeadingFormat = true;

                    Row itemRow = (Row)bm.BookmarkStart.GetAncestor(NodeType.Row);
                    Row coppyItemRow = (Row)itemRow.Clone(true);
                    int itemRowIndex = itemTable.Rows.IndexOf(itemRow);

                    //itemRow.Remove();

                    if (replaceObjs == null || replaceObjs.Count == 0)
                    {
                        itemRow.Remove();
                    }
                    else
                    {
                        for (int i = (replaceObjs.Count - 1); i >= 0; i--)
                        {

                            Row newItemRow = (Row)coppyItemRow.Clone(true);
                            itemTable.Rows.Insert((itemRowIndex + 1), newItemRow);
                            ReplaceNodeText(newItemRow, replaceObjs[i]);
                            //if (i == 0)
                            //    itemRow.Remove();
                        }
                    }
                }
            }
        }
        public void RemoveRowWithSpecificBookmark(Document doc, string bookmarkName)
        {


            // ตรวจสอบว่า Bookmark มีอยู่จริงหรือไม่
            if (doc.Range.Bookmarks[bookmarkName] != null)
            {
                Bookmark bookmark = doc.Range.Bookmarks[bookmarkName];
                Node currentNode = bookmark.BookmarkStart;

                // หา ParentNode ที่เป็น Row
                while (currentNode != null && !(currentNode is Row))
                {
                    currentNode = currentNode.ParentNode;
                }

                // ตรวจสอบว่า Row มี Parent ที่เป็น Table จริงหรือไม่
                if (currentNode is Row row && row.ParentNode is Table table)
                {
                    row.Remove();
                }
            }
        }
        public List<Dictionary<string, ReplaceObject>> ConvertDataToReplaceObject<T>(T data)
        {
            List<Dictionary<string, ReplaceObject>> result = new List<Dictionary<string, ReplaceObject>>();
            Dictionary<string, ReplaceObject> dicObj = null;
            try
            {
                DataTable dataTable = ToDataTable(data);

                foreach (DataRow dr in dataTable.Rows)
                {
                    dicObj = new Dictionary<string, ReplaceObject>();
                    foreach (DataColumn dc in dataTable.Columns)
                    {
                        if (dc.ColumnName.Contains("_HTML"))
                            dicObj.Add(dc.ColumnName/*.Replace("_html", "")*/, new ReplaceObject() { Value = dr[dc], Format = "", IsHtml = true });
                        else
                            dicObj.Add(dc.ColumnName, new ReplaceObject() { Value = dr[dc], Format = "", IsHtml = false });
                    }

                    if (dicObj != null && dicObj.Count() > 0)
                    {
                        result.Add(dicObj);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        public List<Dictionary<string, ReplaceObject>> ConvertDataToReplaceObject<T>(List<T> data)
        {
            List<Dictionary<string, ReplaceObject>> result = new List<Dictionary<string, ReplaceObject>>();
            Dictionary<string, ReplaceObject> dicObj = null;
            try
            {
                DataTable dataTable = ToDataTable(data);

                foreach (DataRow dr in dataTable.Rows)
                {
                    dicObj = new Dictionary<string, ReplaceObject>();
                    foreach (DataColumn dc in dataTable.Columns)
                    {
                        if (dc.ColumnName.Contains("_HTML"))
                            dicObj.Add(dc.ColumnName/*.Replace("_html", "")*/, new ReplaceObject() { Value = dr[dc], Format = "", IsHtml = true });
                        else
                            dicObj.Add(dc.ColumnName, new ReplaceObject() { Value = dr[dc], Format = "", IsHtml = false });
                    }

                    if (dicObj != null && dicObj.Count() > 0)
                    {
                        result.Add(dicObj);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        private DataTable ToDataTable<T>(T item)
        {
            DateTime _date_start = DateTime.Now;

            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            //PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                if (prop.PropertyType.Namespace == "System.Collections.Generic")
                {
                    continue;
                }
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }

            //var values = new object[Props.Length];
            var values = new object[dataTable.Columns.Count];
            for (int i = 0; i < Props.Length; i++)
            {
                if (Props[i].PropertyType.Namespace == "System.Collections.Generic")
                {
                    continue;
                }
                //inserting property values to datatable rows
                values[i] = Props[i].GetValue(item, null);
            }
            dataTable.Rows.Add(values);

            //put a breakpoint here and check datatable
            return dataTable;
        }
        private DataTable ToDataTable<T>(List<T> items)
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
        public void FIRSTReplaceNodeDataRow(Node node, string bookMarkName, List<Dictionary<string, ReplaceObject>> replaceObjs)
        {
            Console.WriteLine($"🔍 Checking for Bookmark: {bookMarkName}");

            Bookmark bm = node.Range.Bookmarks[bookMarkName];

            bool isInBookmark = bm != null;
            if (!isInBookmark)
            {
                Console.WriteLine($"❌ Bookmark '{bookMarkName}' not found.");
                return;
            }

            Table itemTable = (Table)bm.BookmarkStart.GetAncestor(NodeType.Table);
            bool isInTable = itemTable != null; // Debug null case

            if (!isInTable)
            {
                Console.WriteLine($"❌ Bookmark '{bookMarkName}' is not inside a table.");
                return;
            }

            Row itemRow = (Row)bm.BookmarkStart.GetAncestor(NodeType.Row);
            if (itemRow == null)
            {
                Console.WriteLine($"❌ Bookmark '{bookMarkName}' is not inside a row.");
                return;
            }
            Row coppyItemRow = (Row)itemRow.Clone(true);
            int itemRowIndex = itemTable.Rows.IndexOf(itemRow);

            Console.WriteLine($"📌 Original row index: {itemRowIndex}");

            //itemRow.Remove();
            //Console.WriteLine($"⚠️ Removed original row.");

            if (replaceObjs == null || replaceObjs.Count == 0)
            {
                Console.WriteLine($"⚠️ No data found for '{bookMarkName}', keeping table structure intact.");
                return;
            }

            Console.WriteLine($"🔄 Replacing with {replaceObjs.Count} new rows...");

            // Loop through data from last to first to preserve order
            for (int i = (replaceObjs.Count - 1); i >= 0; i--)
            {
                Row newItemRow = (Row)coppyItemRow.Clone(true);
                itemTable.Rows.Insert((itemRowIndex + 1), newItemRow);
                ReplaceNodeText(newItemRow, replaceObjs[i]);
                Console.WriteLine($"✅ Inserted row {i + 1} at index {itemRowIndex + 1}");
            }

            Console.WriteLine($"🎯 Successfully replaced data for '{bookMarkName}'.");
        }
    }
    #endregion

    #region ReplaceObject
    public class ReplaceObject
    {
        public object Value { get; set; }
        public string Format { get; set; }
        public bool IsHtml { get; set; }
        public FindReplaceOptions Options { get; set; }
    }
    #endregion

    #region ReplaceWithHtml
    public class ReplaceWithHtml : IReplacingCallback
    {
        internal ReplaceWithHtml(FindReplaceOptions options)
        {
            mOptions = options;
        }

        ReplaceAction IReplacingCallback.Replacing(ReplacingArgs args)
        {
            DocumentBuilder builder = new DocumentBuilder((Document)args.MatchNode.Document);
            builder.MoveTo(args.MatchNode);
            builder.InsertHtml(args.Replacement);
            args.Replacement = "";
            return ReplaceAction.Replace;
        }

        private readonly FindReplaceOptions mOptions;
    }
    #endregion

}
