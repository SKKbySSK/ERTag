using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWTag.Utils
{
    public static class EncodeSupport
    {
        /// <summary>
        /// UTF-16LE, UTF-16BE, UTF-8の何れかを返します
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Encoding DetectEncodingFromBOM(byte[] bytes)
        {
            if (bytes.Length < 2)
            {
                return null;
            }
            if ((bytes[0] == 0xfe) && (bytes[1] == 0xff))
            {
                return new UnicodeEncoding(true, true);
            }
            if ((bytes[0] == 0xff) && (bytes[1] == 0xfe))
            {
                return new UnicodeEncoding(false, true);
            }
            if (bytes.Length < 3)
            {
                return null;
            }
            if ((bytes[0] == 0xef) && (bytes[1] == 0xbb) && (bytes[2] == 0xbf))
            {
                return new UTF8Encoding(true, true);
            }
            if (bytes.Length < 4)
            {
                return null;
            }

            return null;
        }
    }
}
