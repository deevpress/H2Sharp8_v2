using System.Data;
using System.IO;

namespace H2Example.Extensions
{
    public static class DataTableExtensions
    {
        public static string ToXml(this DataTable table)
        {
            var o = new MemoryStream();
            table.WriteXml(o);
            var t = new StringWriter();
            o.Close();
            o = new MemoryStream(o.GetBuffer());
            var reader = new StreamReader(o);
            return reader.ReadToEnd();
        }
    }
}
