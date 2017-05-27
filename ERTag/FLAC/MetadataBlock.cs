using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RWTag.FLAC
{
    class MetadataBlock
    {
        public MetadataBlockKind Kind;

        /// <summary>
        /// not include the size of the METADATA_BLOCK_HEADER
        /// </summary>
        public int Length;

        public byte[] Data;

        public static MetadataBlock FromStream(Stream Stream, Func<MetadataBlockKind, bool> ShouldRead)
        {
            byte[] header = new byte[4];
            int c = Stream.Read(header, 0, 4);
            if(c >= 4)
            {
                MetadataBlock block = new MetadataBlock();
                block.Kind = block.Kind.FromByte(header[0]);
                block.Length = BitConverter.ToInt32(new byte[] { header[3], header[2], header[1], 0 }, 0);
                bool r = ShouldRead(block.Kind);

                if (r)
                {
                    byte[] buffer = new byte[block.Length];
                    Stream.Read(buffer, 0, block.Length);
                    block.Data = buffer;
                }
                else
                    Stream.Seek(block.Length, SeekOrigin.Current);

                return block;
            }

            return null;
        }
    }
}
