using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWTag.FLAC
{
    class MetadataBlockVorbisComment : MetadataBlock
    {
        public int VendorLength;
        public string VendorString;
        public int CommentCount;
        public string[] Comments;

        public MetadataBlockVorbisComment(MetadataBlock Block)
        {
            int i = 0;
            VendorLength = BitConverter.ToInt32(Block.Data, i);
            i += 4;

            VendorString = Encoding.UTF8.GetString(Block.Data, i, VendorLength);
            i += VendorLength;

            CommentCount = BitConverter.ToInt32(Block.Data, i);
            i += 4;

            Comments = new string[CommentCount];
            for (int c = 0; CommentCount > c; c++)
            {
                int cl = BitConverter.ToInt32(Block.Data, i);
                i += 4;

                Comments[c] = Encoding.UTF8.GetString(Block.Data, i, cl);
                i += cl;
            }

        }
    }
}
