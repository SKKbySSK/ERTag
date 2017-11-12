using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWTag
{
    public class EncodingProvider
    {
        public static Encoding UTF8 { get; set; } = EncodingProvider.UTF8;
        public static Encoding Unicode { get; set; } = EncodingProvider.Unicode;
        public static Encoding BigEndianUnicode { get; set; } = EncodingProvider.BigEndianUnicode;
        public static Encoding ShiftJis { get; set; }
        public static Encoding ISO88591 { get; set; }
    }
}
