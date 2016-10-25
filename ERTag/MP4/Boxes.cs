using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWTag.MP4
{
    //cref=http://atomicparsley.sourceforge.net/mpeg-4files.html
    //cref=http://www.mp4ra.org/atoms.html
    public class Atoms : List<Atom>
    {
        public Atom FindAtomByPath(string Path)
        {
            string[] paths = Path.Split(new char[] { '/', '\\' },
                StringSplitOptions.RemoveEmptyEntries);

            Atoms Parent = this;
            for(int i = 0;paths.Length > i; i++)
            {
                Atom ret = Find(paths[i], Parent);

                if (paths.Length - 1 == i)
                    return ret;

                if (ret != null)
                    Parent = ret.Children;
                else
                    return null;
            }

            return null;
        }

        public void SetAtomByPath(Atom Atom, string Path)
        {
            string[] paths = Path.Split(new char[] { '/', '\\' },
                StringSplitOptions.RemoveEmptyEntries);

            Atoms Parent = this;
            for (int i = 0; paths.Length > i; i++)
            {
                int ind = FindIndex(paths[i], Parent);

                if (paths.Length - 1 == i)
                {
                    Parent[ind] = Atom;
                    UpdateAtomData();
                }

                if (ind > -1)
                    Parent = Parent[ind].Children;
                else
                    return;
            }
        }

        private Atom Find(string Name, Atoms Parent)
        {
            for(int i = 0;Parent.Count > i; i++)
            {
                if (Parent[i].Name == Name)
                    return Parent[i];
            }
            return null;
        }

        public int FindIndex(string Name, Atoms Parent)
        {
            for (int i = 0; Parent.Count > i; i++)
            {
                if (Parent[i].Name == Name)
                    return i;
            }
            return -1;
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; Count > i; i++)
                bytes.AddRange(this[i].ToBytes());
            return bytes.ToArray();
        }

        public int GetLength()
        {
            long len = 0;
            for(int i = 0;Count > i; i++)
            {
                len += this[i].GetTotalLength();
            }

            return (int)len;
        }

        /// <summary>
        /// 追加されているAtomのLength, Offsetを更新します。
        /// </summary>
        public void UpdateAtomData()
        {
            Update(0, this);
        }

        private void Update(long CurrentOffset, Atoms Children)
        {
            for(int i = 0;Children.Count > i; i++)
            {
                Atom atom = Children[i];
                atom.Length = atom.GetTotalLength();
                atom.Offset = CurrentOffset;

                CurrentOffset += atom.Length;
                Update(CurrentOffset, atom.Children);
            }
        }
    }

    public class Atom
    {
        public Atom() { }

        public Atom(Encoding Encode, long Offset, byte[] Header)
        {
            this.Encode = Encode;
            this.Offset = Offset;
            Array.Reverse(Header, 0, 4);
            Length = BitConverter.ToInt32(Header, 0);
            Name = Encode.GetString(Header, 4, 4);
        }

        public Atom(Encoding Encode, long Offset, byte[] Length, byte[] Name)
        {
            this.Encode = Encode;
            this.Offset = Offset;
            this.Length = BitConverter.ToInt32(Length, 0);
            this.Name = Encode.GetString(Name, 0, 4);
        }

        public Atom(Encoding Encode)
        {
            this.Encode = Encode;
        }

        public Atoms Children { get; internal set; } = new Atoms();
        public Encoding Encode { get; internal set; }
        public int Length { get; internal set; }
        public long Offset { get; internal set; }
        public string Name { get; internal set; }

        // サイズはヘッダー(8bytes)を除いた大きさ
        public byte[] Data { get; internal set; }

        public override string ToString()
        {
            return Name;
        }

        public virtual byte[] ToBytes()
        {
            List<byte> ret = new List<byte>();
            byte[] sizeb = BitConverter.GetBytes(Length);
            Array.Reverse(sizeb);
            ret.AddRange(sizeb);
            ret.AddRange(Encode.GetBytes(Name));

            if(Children.Count > 0)
            {
                for (int i = 0; Children.Count > i; i++)
                    ret.AddRange(Children[i].ToBytes());
            }
            else
            {
                ret.AddRange(Data);
            }

            return ret.ToArray();
        }

        public int GetTotalLength()
        {
            int size = 8;
            if (Children.Count > 0)
            {
                for(int i = 0;Children.Count > i; i++)
                {
                    size += Children[i].GetTotalLength();
                }
            }
            else
                size = Data.Length;

            return size;
        }
    }

    public class metaAtom : Atom
    {
        const int hdlrSpace = 4;

        public metaAtom(Atom metaAtom)
        {
            if (metaAtom.Name == "meta" && metaAtom.Data.Length > 0)
            {
                Encode = metaAtom.Encode;
                Name = metaAtom.Name;
                Length = metaAtom.Length;
                Data = metaAtom.Data;
                MemoryStream ms = new MemoryStream(metaAtom.Data, hdlrSpace, metaAtom.Length - hdlrSpace - 8);

                byte[] hdlrHeader = new byte[8];
                ms.Read(hdlrHeader, 0, 8);
                hdlr = new Atom(metaAtom.Encode, metaAtom.Offset + hdlrSpace, hdlrHeader);
                hdlr.Data = new byte[hdlr.Length];
                ms.Read(hdlr.Data, 0, hdlr.Length - 8);

                ilst = new ilstFrame(Encode, ms);
                ms.Dispose();

                //TODO To Bytes
            }
            else
                throw new TagReaderException("Failed to read meta data");
        }

        public RWTag.Tag GetTag()
        {
            RWTag.Tag tag = new RWTag.Tag();

            if (ilst.Data.Count > 0)
                tag.Name = "MP4";

            for(int i = 0;ilst.Data.Count > i; i++)
            {
                ilstFrame.ilstAtom f = ilst.Data[i];
                string name = BitConverter.ToString(f.Name).Replace("-", "");

                byte[] data = f.Value;
                int len = f.Length - 16;
                switch (name)
                {
                    case "A96E616D":
                        tag.Title = Encode.GetString(data, 0, len);
                        break;
                    case "A9616C62":
                        tag.Album = Encode.GetString(data, 0, len);
                        break;
                    case "A9415254":
                        tag.Artist = Encode.GetString(data, 0, len);
                        break;
                    case "A967656E":
                        tag.Genre = Encode.GetString(data, 0, len);
                        break;
                    case "A9646179":
                        tag.Date = new DateTime(int.Parse(Encode.GetString(data, 0, len)), 1, 1);
                        break;
                    case "A96C7972":
                        tag.Lyrics = Encode.GetString(data, 0, len);
                        break;
                    case "636F7672":
                        tag.Image = data;
                        tag.ImageMIMEType = ImageMIME.none;
                        break;
                    case "74726B6E":
                        tag.Track = data[3];
                        tag.TotalTrack = data[5];
                        break;
                    case "6469736B":
                        tag.DiscNumber = data[3];
                        tag.TotalDiscNumber = data[5];
                        break;
                }
            }


            return tag;
        }

        public void SetTag(RWTag.Tag Tag)
        {
            this.ilst.Data.Clear();

            List<ilstFrame.ilstAtom> ilst = new List<ilstFrame.ilstAtom>();
            if (!string.IsNullOrEmpty(Tag.Title)) ilst.Add(GenerateAtom("A96E616D", Tag.Title));
            if (!string.IsNullOrEmpty(Tag.Album)) ilst.Add(GenerateAtom("A9616C62", Tag.Album));
            if (!string.IsNullOrEmpty(Tag.Artist)) ilst.Add(GenerateAtom("A9415254", Tag.Artist));
            if (!string.IsNullOrEmpty(Tag.Genre)) ilst.Add(GenerateAtom("A967656E", Tag.Genre));
            if (!string.IsNullOrEmpty(Tag.Lyrics)) ilst.Add(GenerateAtom("A96C7972", Tag.Lyrics));

            int len = 12;
            len += hdlr.Length;
            for (int i = 0; ilst.Count > i; i++)
                len += ilst[i].Length;

            Data = new byte[len];
            MemoryStream ms = new MemoryStream(Data);
            for (int i = 0; ilst.Count > i; i++)
                ms.Write(ilst[i].ToBytes(), 0, ilst[i].Length);
            ms.Dispose();

            Length = len;

            this.ilst.Data = ilst;
        }

        private ilstFrame.ilstAtom GenerateAtom(string Hex, string Value)
        {
            ilstFrame.ilstAtom atom = new ilstFrame.ilstAtom(Encode);

            const int minSize = 24;
            List<byte> atomb = new List<byte>();

            byte[] value = Encode.GetBytes(Value);

            atomb.AddRange(BitConverter.GetBytes(minSize + value.Length));
            atomb.AddRange(GetBytesFromString(Hex));
            atomb.AddRange(BitConverter.GetBytes(minSize + value.Length - 8));
            atomb.AddRange(new byte[8]);
            atomb.AddRange(value);

            atom.Value = atomb.ToArray();
            atom.Length = atomb.Count;
            atom.VersionAndFlags = new byte[4];
            atom.Name = GetBytesFromString(Hex);

            return atom;
        }

        private byte[] GetBytesFromString(string HexString)
        {
            if (HexString.Length % 2 == 0)
            {
                byte[] bytes = new byte[HexString.Length / 2];
                for (int i = 0; bytes.Length > i; i++)
                    bytes[i] = Convert.ToByte(HexString.Substring(i * 2, 2), 16);
                return bytes;
            }
            else
                return null;
        }

        public override byte[] ToBytes()
        {
            List<byte> ret = new List<byte>();
            byte[] sizeb = BitConverter.GetBytes(Length);
            Array.Reverse(sizeb);
            ret.AddRange(sizeb);
            ret.AddRange(Encode.GetBytes(Name));
            ret.AddRange(hdlr.ToBytes());
            ret.AddRange(ilst.ToBytes());

            return ret.ToArray();
        }

        public override string ToString()
        {
            return "metaAtom";
        }

        public Atom hdlr { get; set; }
        public ilstFrame ilst { get; set; }
    }

    public class ilstFrame
    {
        public ilstFrame(Encoding Encode, MemoryStream ms)
        {
            this.Encode = Encode;
            byte[] sizeb = new byte[4];
            ms.Read(sizeb, 0, 4);
            Array.Reverse(sizeb);
            Length = BitConverter.ToInt32(sizeb, 0);

            byte[] nameb = new byte[4];
            ms.Read(nameb, 0, 4);
            Name = Encode.GetString(nameb, 0, 4);

            Parse(ms);
        }

        private void Parse(MemoryStream ms)
        {
            Data = new List<ilstAtom>();
            int count = 0;

            byte[] sizeb = new byte[4];
            while(Length > count)
            {
                if (ms.Read(sizeb, 0, 4) < 4)
                    break;

                count += 4;
                Array.Reverse(sizeb);
                int len = BitConverter.ToInt32(sizeb, 0);

                if (len < 8)
                    break;

                byte[] bytes = new byte[len];
                ms.Read(bytes, 0, len - 4);
                ilstAtom atom = new ilstAtom(Encode, bytes);
                Data.Add(atom);
                count += len - 4;
            }
        }

        private Encoding Encode { get; set; }
        public int Length { get; set; }
        public string Name { get; set; }
        public List<ilstAtom> Data { get; set; }

        public byte[] ToBytes()
        {
            Length = 0;
            List<byte> bytes = new List<byte>();
            for(int i = 0;Data.Count > i; i++)
            {
                byte[] ilstatom = Data[i].ToBytes();
                bytes.AddRange(ilstatom);
                Length += ilstatom.Length;
            }

            bytes.InsertRange(0, Encode.GetBytes(Name));

            byte[] sb = new byte[4];
            sb = BitConverter.GetBytes(Length);
            Array.Reverse(sb);

            bytes.InsertRange(0, sb);
            Length += 8;

            return bytes.ToArray();
        }

        public class ilstAtom
        {
            public ilstAtom(Encoding Encode) { this.Encode = Encode; }

            public ilstAtom(Encoding Encode, byte[] Bytes)
            {
                this.Encode = Encode;
                MemoryStream ms = new MemoryStream(Bytes);

                Name = new byte[4];
                ms.Read(Name, 0, 4);

                byte[] sizeb = new byte[4];
                ms.Read(sizeb, 0, 4);
                Array.Reverse(sizeb);
                Length = BitConverter.ToInt32(sizeb, 0);

                if (Length < 16)
                    return;

                ms.Seek(4, SeekOrigin.Current);

                VersionAndFlags = new byte[4];
                ms.Read(VersionAndFlags, 0, 4);
                ms.Seek(4, SeekOrigin.Current);

                Value = new byte[Length - 16];
                ms.Read(Value, 0, Length - 16);
            }

            private Encoding Encode { get; set; }
            public int Length { get; set; }
            public byte[] Name { get; set; }
            public byte[] VersionAndFlags { get; set; }
            public byte[] Value { get; set; }

            public byte[] ToBytes()
            {
                List<byte> ret = new List<byte>();
                byte[] sizeb = BitConverter.GetBytes(Length);
                Array.Reverse(sizeb);
                ret.AddRange(sizeb);
                ret.AddRange(Name);

                byte[] minisb = new byte[4];
                minisb = BitConverter.GetBytes(Length - 8);
                Array.Reverse(minisb);
                ret.AddRange(minisb);
                ret.AddRange(new byte[] { 0x64, 0x61, 0x74, 0x61 });
                ret.AddRange(VersionAndFlags);
                ret.AddRange(new byte[] { 0, 0, 0, 0 }); //4bytes free space
                ret.AddRange(Value);

                return ret.ToArray();
            }

            public int GetLength()
            {
                return ToBytes().Length;
            }

            public override string ToString()
            {
                return Encode.GetString(Name, 0, 4);
            }
        }
    }
}
