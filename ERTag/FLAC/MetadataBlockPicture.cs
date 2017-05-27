using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RWTag.FLAC
{
    class MetadataBlockPicture : MetadataBlock
    {
        public int PictureType;
        public int MimeLength;
        public string Mime;
        public int DescriptionLength;
        public string Description;
        public int Width;
        public int Height;
        public int BPP;
        public int GifIndex;
        public int PictureLength;
        public byte[] Picture;

        public MetadataBlockPicture(MetadataBlock Block)
        {
            int i = 0;
            Array.Reverse(Block.Data, i, 4);
            PictureType = BitConverter.ToInt32(Block.Data, i);
            i += 4;

            Array.Reverse(Block.Data, i, 4);
            MimeLength = BitConverter.ToInt32(Block.Data, i);
            i += 4;

            Mime = Encoding.UTF8.GetString(Block.Data, i, MimeLength);
            i += MimeLength;
            
            Array.Reverse(Block.Data, i, 4);
            DescriptionLength = BitConverter.ToInt32(Block.Data, i);
            i += 4;

            Description = Encoding.UTF8.GetString(Block.Data, i, DescriptionLength);
            i += DescriptionLength;

            Array.Reverse(Block.Data, i, 4);
            Width = BitConverter.ToInt32(Block.Data, i);
            i += 4;

            Array.Reverse(Block.Data, i, 4);
            Height = BitConverter.ToInt32(Block.Data, i);
            i += 4;

            Array.Reverse(Block.Data, i, 4);
            BPP = BitConverter.ToInt32(Block.Data, i);
            i += 4;

            Array.Reverse(Block.Data, i, 4);
            GifIndex = BitConverter.ToInt32(Block.Data, i);
            i += 4;

            Array.Reverse(Block.Data, i, 4);
            PictureLength = BitConverter.ToInt32(Block.Data, i);
            i += 4;

            Picture = new byte[PictureLength];
            Array.Copy(Block.Data, i, Picture, 0, PictureLength);
        }
    }
}
