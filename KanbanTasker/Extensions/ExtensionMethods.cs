using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KanbanTasker.Extensions
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts a string in date format into a nullable DateTimeOffset type
        /// </summary>
        /// <param name="dateTimeStr"></param>
        /// <returns></returns>
        public static DateTimeOffset? ToNullableDateTimeOffset(this string dateTimeStr)
        {
            DateTimeOffset? dt = null;
            DateTime dt2;
            if (dateTimeStr != null && dateTimeStr is string)
            {
                bool success = DateTime.TryParse(dateTimeStr, out dt2);
                if (success)
                    dt = dt2;
                else
                    return dt;
            }
            return dt;
        }

        public static byte[] ToByteArray(this Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            for (int totalBytesCopied = 0; totalBytesCopied < stream.Length;)
                totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
            return buffer;
        }
    }
}
