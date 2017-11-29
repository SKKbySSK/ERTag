using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWTag.FLAC
{
    public class Tag : BaseTag
    {
        public Tag(SettableStream Stream) : base(Stream)
        {
        }

        public override string[] Extensions { get; } = new string[] { ".flac" };

        public override bool IsReadable()
        {
            throw new NotImplementedException();
        }

        public override RWTag.Tag Read()
        {
            RWTag.Tag tag = new RWTag.Tag();
            Reader reader = new Reader(Stream, EncodingProvider.UTF8);
            reader.Init();

            foreach(MetadataBlock block in reader.Blocks)
            {
                switch (block)
                {
                    case MetadataBlockPicture pict:
                        tag.ImageDescription = pict.Description;
                        tag.Image = pict.Picture;
                        break;
                    case MetadataBlockVorbisComment vorbis:
                        ParseComments(ref tag, vorbis);
                        break;
                }
            }

            reader.Dispose();
            reader = null;

            return tag;
        }

        void ParseComments(ref RWTag.Tag Tag, MetadataBlockVorbisComment vorbis)
        {
            foreach(string comment in vorbis.Comments)
            {
                int eqInd = comment.IndexOf('=');
                string key = comment.Substring(0, eqInd).ToLower().Replace(" ", "");
                string val = comment.Substring(eqInd + 1, comment.Length - eqInd - 1);

                switch (key)
                {
                    case "title":
                        Tag.Title = val;
                        break;
                    case "album":
                        Tag.Album = val;
                        break;
                    case "artist":
                        Tag.Artist = val;
                        break;
                    case "genre":
                        Tag.Genre = val;
                        break;
                    case "description":
                        Tag.Comment = val;
                        break;
                    case "discnumber":
                        Tag.DiscNumber = int.Parse(val);
                        break;
                    case "tracknumber":
                        Tag.Track = int.Parse(val);
                        break;
                }
            }
        }

        void NumberParse(string Base, out int Current, out int Total)
        {
            Current = 0;
            Total = 0;
            if (!int.TryParse(Base, out Current))
            {
                string current = "";
                bool f = false;
                foreach(char c in Base)
                {
                    if (c != '0' | c != '1' | c != '2' | c != '3' | c != '4' | c != '5' |
                        c != '6' | c != '7' | c != '8' | c != '9')
                    {
                        if (!f) Current = int.Parse(current);
                        else
                        {
                            Total = int.Parse(current);
                            return;
                        }
                        current = "";
                    }
                    else
                        current += c;
                }
            }
        }

        public override void Write(RWTag.Tag Tag)
        {
            throw new NotImplementedException();
        }
    }
}
