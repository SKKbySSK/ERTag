using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWTag.FLAC
{
    [Flags]
    enum MetadataBlockKind
    {
        LastBlock = 0b1000_0000_0000,
        StreamInfo = 0b0000_0000_0000,
        Padding = 0b0000_0000_0001,
        Application = 0b0000_0000_0010,
        SeekTable = 0b0000_0000_0011,
        VorbisComment = 0b0000_0000_0100,
        CueSheet = 0b0000_0000_0101,
        Picture = 0b0000_0000_0110,
        Reserved = 0b0000_0000_0111,
        Invalid = 0b0000_0000_1000,
    }

    static class MetadataBlockKindExtension
    {
        internal static MetadataBlockKind FromByte(this MetadataBlockKind block, byte Kind)
        {
            MetadataBlockKind kind = MetadataBlockKind.Invalid;

            string bin = Convert.ToString(Kind, 2).PadLeft(8, '0');
            int k = Convert.ToInt32("0" + bin.Substring(1, 7), 2);

            if (k >= 7 && k <= 126)
                kind = MetadataBlockKind.Reserved;
            else if (k > 126)
                kind = MetadataBlockKind.Invalid;
            else
                kind = (MetadataBlockKind)k;

            if (bin[0] == '1')
                kind |= MetadataBlockKind.LastBlock;

            return kind;
        }
    }
}
