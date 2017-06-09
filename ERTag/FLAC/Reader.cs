using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RWTag.FLAC
{
    class Reader : IDisposable
    {
        Encoding encode;
        Stream stream;
        BinaryReader br;

        public List<MetadataBlock> Blocks { get; } = new List<MetadataBlock>();

        public Reader(Stream Stream, Encoding Encode)
        {
            stream = Stream;
            encode = Encode;
        }

        public void Dispose()
        {
            br?.Dispose();
            br = null;

            stream.Dispose();
            stream = null;
        }

        public bool Init()
        {
            stream.Seek(0, SeekOrigin.Begin);
            br = new BinaryReader(stream, encode, true);

            char[] marker = br.ReadChars(4);
            bool s = marker.SequenceEqual(new char[] { 'f', 'L', 'a', 'C' });
            if (s)
            {
                Func<MetadataBlockKind, bool> recognizer = (kind) =>
                    {
                    switch (kind)
                    {
                        case MetadataBlockKind.VorbisComment:
                            return true;
                        case MetadataBlockKind.Picture:
                            return true;
                        default:
                            return false;
                    }
                };
                
                while (true)
                {
                    MetadataBlock block = MetadataBlock.FromStream(stream, recognizer);
                    if (block.Data != null)
                    {
                        switch (block.Kind)
                        {
                            case MetadataBlockKind.Picture:
                                Blocks.Add(new MetadataBlockPicture(block));
                                break;
                            case MetadataBlockKind.VorbisComment:
                                Blocks.Add(new MetadataBlockVorbisComment(block));
                                break;
                        }
                    }

                    if (block.Kind.HasFlag(MetadataBlockKind.LastBlock))
                        break;
                }
            }

            return s;
        }
    }
}
