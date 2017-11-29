using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RWTag.MP3.ID3v2
{
    public class Tag : BaseTag
    {
        public Tag(SettableStream Stream) : base(Stream)
        {
            Encode = EncodingProvider.ShiftJis;
        }

        public enum Version { v2_2, v2_3, v2_4, Unknown }

        public override string[] Extensions { get; } = new string[] { ".mp3" };

        private int TotalTagSize
        {
            get
            {
                Stream.Seek(6, SeekOrigin.Begin);
                byte[] buf = new byte[4];
                Stream.Read(buf, 0, 4);
                int size = Utils.ByteConverter.GetIntFromSynchsafe(buf);
                return size;
            }
        }

        public override bool IsReadable()
        {
            return Reader.CheckID(Stream) && GetVersion() != Version.Unknown;
        }

        public override RWTag.Tag Read()
        {
            RWTag.Tag tag = new RWTag.Tag();

            switch (GetVersion())
            {
                case Version.v2_3:
                case Version.v2_2:
                case Version.v2_4:
                    Three three = new Three(Stream, Encode);
                    tag = three.Read();
                    break;
            }

            return tag;
        }

        public override void Write(RWTag.Tag Tag)
        {
            byte[] raw;
            if (Reader.CheckID(Stream))
            {
                BinaryReader reader = new BinaryReader(Stream, Encode, true);

                int seek = 10 + TotalTagSize;
                Stream.Seek(seek, SeekOrigin.Begin);
                raw = reader.ReadBytes((int)Stream.Length - seek);
            }
            else
            {
                raw = new byte[Stream.Length];
                Stream.Seek(0, SeekOrigin.Begin);
                if (Stream.Length >= int.MaxValue)
                    while (Stream.Read(raw, 0, int.MaxValue) > 0) { }
                else
                    Stream.Read(raw, 0, (int)Stream.Length);
            }

            switch (GetVersion())
            {
                case Version.v2_3:
                    Three three = new Three(Stream, Encode);
                    three.Write(Tag, raw);
                    break;

                case Version.Unknown:
                    Three unk = new Three(Stream, Encode);
                    unk.Write(Tag, raw);
                    break;
            }
        }

        private Version GetVersion()
        {
            if (Stream.Length < 5) return Version.Unknown;
            Stream.Seek(3, SeekOrigin.Begin);
            BinaryReader br = new BinaryReader(Stream, Encode, true);
            sbyte major = br.ReadSByte();
            sbyte minot = br.ReadSByte();

            br.Dispose();

            switch (major)
            {
                case 2:
                    return Version.v2_2;

                case 3:
                    return Version.v2_3;

                case 4:
                    return Version.v2_4;

                default:
                    return Version.Unknown;
            }
        }
    }

    internal static class Reader
    {
        public static bool CheckID(Stream Stream)
        {
            if (Stream.Length < 3) return false;
            Stream.Seek(0, SeekOrigin.Begin);
            byte[] buf = new byte[3];
            Stream.Read(buf, 0, 3);

            return buf.SequenceEqual(new byte[] { 0x49, 0x44, 0x33 });
        }
    }

    //cref=http://eleken.y-lab.org/report/other/mp3tags.shtml
    internal abstract class ID3v2Reader
    {
        public ID3v2Reader(Stream Stream, Encoding Encode)
        {
            this.Stream = Stream;
            this.Encode = Encode;
        }

        public ID3v2Reader(Stream stream)
        {
            Stream = stream;
        }

        protected enum BOM { UTF16LE = 0xFFFE, UTF16BE = 0xFEFF, UTF8 = 0xEFBBFF, Unknwon = 0 }

        protected Encoding Encode { get; set; }

        protected string[] PictureType { get; } = new string[]
        {
            "Other", "32x32 pixels 'file icon' (PNG only)", "Other file icon",
            "Cover (front)","Cover (back)","Leaflet page","Media (e.g. lable side of CD)",
            "Lead artist/lead performer/soloist","Artist/performer","Conductor","Band/Orchestra",
            "Composer","Lyricist/text writer","Recording Location","During recording","During performance",
            "Movie/video screen capture","A bright coloured fish","Illustration","Band/artist logotype","Publisher/Studio logotype"
        };

        protected Stream Stream { get; set; }

        protected int ReadSynchsafe()
        {
            byte[] buf = new byte[4];
            Stream.Read(buf, 0, 4);
            int size = Utils.ByteConverter.GetIntFromSynchsafe(buf);
            return size;
        }

        public abstract RWTag.Tag Read();

        public abstract void Write(RWTag.Tag Tag, byte[] RawData);

        protected string FindPictureType(int Index)
        {
            if (Index > 0 && PictureType.Length > Index) return PictureType[Index];
            else return "Unknown";
        }

        protected byte[] FrameToByte(Frame Frame, Encoding Encode)
        {
            Frame.Size = Frame.Data.Length;

            List<byte> buffer = new List<byte>();
            buffer.AddRange(Encode.GetBytes(Frame.ID));
            buffer.AddRange(Utils.ByteConverter.ZeroPadding(BitConverter.GetBytes(Frame.Size), 4, true));
            buffer.AddRange(Frame.Flag);
            buffer.AddRange(Frame.Data);

            return buffer.ToArray();
        }

        protected BOM GetBOM(byte[] Encoding)
        {
            if (Encoding.Length >= 2)
            {
                byte[] UTF16 = new byte[2];
                UTF16[0] = Encoding[0];
                UTF16[1] = Encoding[1];

                if (UTF16.SequenceEqual(new byte[] { 0xFF, 0xFE })) return BOM.UTF16LE;
                if (UTF16.SequenceEqual(new byte[] { 0xFE, 0xFF })) return BOM.UTF16BE;
                if (Encoding.Length >= 3)
                {
                    byte[] UTF8 = new byte[3];
                    UTF8[0] = Encoding[0];
                    UTF8[1] = Encoding[1];
                    UTF8[2] = Encoding[2];
                    if (UTF8.SequenceEqual(new byte[] { 0xEF, 0xBB, 0xBF })) return BOM.UTF8;
                }
            }

            return BOM.Unknwon;
        }

        protected byte[] GetBOM(BOM BOM)
        {
            byte[] bytes = Utils.ByteConverter.RemoveZero(BitConverter.GetBytes((int)BOM));
            return bytes;
        }

        protected Encoding GetEncode(byte Byte)
        {
            try
            {
                switch (Byte)
                {
                    case 0:
                        return EncodingProvider.ISO88591;

                    case 1:
                        return EncodingProvider.Unicode;

                    case 2:
                        return EncodingProvider.BigEndianUnicode;

                    case 3:
                        return EncodingProvider.UTF8;

                    default:
                        return EncodingProvider.ShiftJis;
                }
            }
            catch (Exception) { return EncodingProvider.ShiftJis; }
        }

        protected byte GetEncode(Encoding Encode)
        {
            if (Encode.WebName == "ISO-8859-1") return 0;
            if (Encode.WebName == "UTF-16" || Encode.WebName == "UTF-16LE") return 1;
            if (Encode.WebName == "UTF-16BE") return 2;
            if (Encode.WebName == "UTF-8") return 3;
            return 0;
        }

        protected byte[] GetPadding(int Length)
        {
            return new byte[Length];
        }

        protected byte[] GetPadding()
        {
            return new byte[32];
        }

        protected class Frame
        {
            public byte[] Data { get; set; }
            public byte[] Flag { get; set; }
            public string ID { get; set; }
            public int Size { get; set; }
        }
    }
}