using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.Helpers.Extensions
{
    public static class CustomExtensionMethods
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTimeStr"></param>
        /// <returns></returns>
        public static DateTimeOffset? ToNullableDateTimeOffset(this string dateTimeStr)
        {
            DateTimeOffset? dt = null;
            DateTime dt2;
            if (dateTimeStr != null && dateTimeStr is string)
            {
                var stringToConvert = dateTimeStr as string;
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
