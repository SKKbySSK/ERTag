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
    //cref=http://d.hatena.ne.jp/SofiyaCat/20080430
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
                len += this[i].GetTotalLength();

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

        public Atom(Encoding Encode, long Offset, int Length, string Name)
        {
            this.Encode = Encode;
            this.Offset = Offset;
            this.Length = Length;
            this.Name = Name;
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

            Length = 0;

            if (Children.Count > 0)
            {
                for (int i = 0; Children.Count > i; i++)
                {
                    byte[] child = Children[i].ToBytes();
                    Length += child.Length;
                    ret.AddRange(child);
                }
            }
            else
            {
                Length += Data.Length;
                ret.AddRange(Data);
            }

            Length += 8;

            ret.InsertRange(0, Encode.GetBytes(Name));

            byte[] sizeb = BitConverter.GetBytes(Length);
            Array.Reverse(sizeb);
            ret.InsertRange(0, sizeb);

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
                size = Data.Length + 8;

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
                Offset = metaAtom.Offset;

                Parse();
            }
            else
                throw new TagReaderException("Failed to read meta data");
        }

        private void Parse()
        {
            MemoryStream ms = new MemoryStream(Data, hdlrSpace, Length - hdlrSpace - 8);

            byte[] hdlrHeader = new byte[8];
            ms.Read(hdlrHeader, 0, 8);
            hdlr = new Atom(Encode, Offset + hdlrSpace, hdlrHeader);
            hdlr.Data = new byte[hdlr.Length - 8];
            ms.Read(hdlr.Data, 0, hdlr.Length - 8);

            ilst = new ilstFrame(Encode, ms);

            ms.Dispose();
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
            List<ilstFrame.ilstAtom> ilst = new List<ilstFrame.ilstAtom>();
            if (!string.IsNullOrEmpty(Tag.Title)) ilst.Add(GenerateAtom("A96E616D", Tag.Title));
            if (!string.IsNullOrEmpty(Tag.Album)) ilst.Add(GenerateAtom("A9616C62", Tag.Album));
            if (!string.IsNullOrEmpty(Tag.Artist)) ilst.Add(GenerateAtom("A9415254", Tag.Artist));
            if (!string.IsNullOrEmpty(Tag.Genre)) ilst.Add(GenerateAtom("A967656E", Tag.Genre));
            if (!string.IsNullOrEmpty(Tag.Lyrics)) ilst.Add(GenerateAtom("A96C7972", Tag.Lyrics));
            ReplaceArray(ilst);

            List<byte> data = new List<byte>(new byte[4]);
            data.AddRange(hdlr.ToBytes());
            data.AddRange(this.ilst.ToBytes());

            Data = data.ToArray();

            Length = data.Count + 8;
        }

        private void ReplaceArray(List<ilstFrame.ilstAtom> SourceAtoms)
        {
            for(int i = 0; ilst.Data.Count > i; i++)
            {
                for(int j = 0;SourceAtoms.Count > j; j++)
                {
                    if (ilst.Data[i].Name.SequenceEqual(SourceAtoms[j].Name))
                        ilst.Data[i] = SourceAtoms[j];
                }
            }
        }

        private ilstFrame.ilstAtom GenerateAtom(string Hex, string Value)
        {
            ilstFrame.ilstAtom atom = new ilstFrame.ilstAtom(Encode);
            
            List<byte> atomb = new List<byte>();

            byte[] value = Encode.GetBytes(Value);

            atom.Value = value;
            atom.VersionAndFlags = new byte[4];
            atom.Name = GetBytesFromString(Hex);
            atom.Length = atomb.Count + 20;

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
            ret.AddRange(Encode.GetBytes(Name));
            ret.AddRange(Data);

            Length = ret.Count + 4;
            
            byte[] sizeb = BitConverter.GetBytes(Length);
            Array.Reverse(sizeb);
            ret.InsertRange(0, sizeb);

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
            while(Length - 8 > count)
            {
                if (ms.Read(sizeb, 0, 4) < 4)
                    break;
                
                Array.Reverse(sizeb);
                int len = BitConverter.ToInt32(sizeb, 0);

                if (len < 8)
                    break;

                byte[] bytes = new byte[len - 4];
                ms.Read(bytes, 0, len - 4);
                ilstAtom atom = new ilstAtom(Encode, bytes);
                Data.Add(atom);
                count += len;
            }
        }

        private Encoding Encode { get; set; }
        public int Length { get; set; }
        public string Name { get; set; }
        public List<ilstAtom> Data { get; set; }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(Encode.GetBytes(Name));

            for(int i = 0;Data.Count > i; i++)
            {
                byte[] ilstatom = Data[i].ToBytes();
                bytes.AddRange(ilstatom);
            }

            Length = bytes.Count + 4;

            byte[] sb = new byte[4];
            sb = BitConverter.GetBytes(Length);
            Array.Reverse(sb);
            bytes.InsertRange(0, sb);

            return bytes.ToArray();
        }

        public class ilstAtom
        {
            public ilstAtom(Encoding Encode) { this.Encode = Encode; }

            public ilstAtom(Encoding Encode, byte[] Bytes)
            {
                this.Encode = Encode;
                using (MemoryStream ms = new MemoryStream(Bytes))
                {
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
            }

            private Encoding Encode { get; set; }
            public int Length { get; set; }
            public byte[] Name { get; set; }
            public byte[] VersionAndFlags { get; set; }
            public byte[] Value { get; set; }

            public byte[] ToBytes()
            {
                if (Encode.GetBytes("free").SequenceEqual(Name))
                {
                    return null;
                }

                List<byte> ret = new List<byte>(Name);
                
                ret.AddRange(new byte[] { 0x64, 0x61, 0x74, 0x61 });
                ret.AddRange(VersionAndFlags);
                ret.AddRange(new byte[] { 0, 0, 0, 0 }); //4bytes free space
                ret.AddRange(Value);

                byte[] minib = new byte[4];
                minib = BitConverter.GetBytes(ret.Count);
                Array.Reverse(minib);
                ret.InsertRange(4, minib);

                byte[] sizeb = new byte[4];
                sizeb = BitConverter.GetBytes(ret.Count + 4);
                Array.Reverse(sizeb);
                ret.InsertRange(0, sizeb);

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

    public class SampleAtom
    {
        //オフセットは4byte連番で構成されてる
        //4byteの空間がatomの後にある
        //mdatはatomの直後からチャンクがある
        public SampleAtom(Atom mdat, Atom stco, Atom stsz)
        {
            this.mdat = mdat;
            this.stco = stco;
            this.stsz = stsz;

            coPos += 4;
            ChunckOffsetCount = GetInt32(stco.Data, coPos);
            coPos += 4;

            szPos += 8;
            SizeCount = GetInt32(stsz.Data, szPos);
            szPos += 4;

            LeftOver = ChunckOffsetCount;

            Samples = new Sample[ChunckOffsetCount];

            int bo = GetInt32(stco.Data, coPos);
            coPos += 4;

            for(int i = 0;ChunckOffsetCount > i; i++)
            {
                Samples[i] = ReadSampleTable(bo, out bo);
            }

            Offset = mdat.Offset + 8;

            UpdateSampleData();
        }

        private void UpdateSampleData()
        {
            int boff = (int)Offset;
            for(int i = 0; ChunckOffsetCount > i; i++)
            {
                Samples[i].Offset = boff;
                boff +=  Samples[i].GetTotalSize();
            }
        }

        public Atom GetChunckOffsetAtom()
        {
            List<byte> data = new List<byte>(new byte[4]);

            byte[] coCount = BitConverter.GetBytes(ChunckOffsetCount);
            Array.Reverse(coCount);
            data.AddRange(coCount);

            for (int i = 0; Samples.Length > i; i++)
                data.AddRange(Samples[i].GetOffsetBytes());

            stco.Data = data.ToArray();
            stco.Length = data.Count + 8;

            return stco;
        }

        private long Offset;
        private Atom stsz, stco, mdat;
        private int ChunckOffsetCount, SizeCount, LeftOver;

        private Sample[] Samples;

        //stszは名前の後に8byte不明な場所があり、そのあと4byteがサンプル数その後から4byte連続サイズ
        //
        int coPos = 0, szPos = 0;
        private Sample ReadSampleTable(int BaseOffset, out int NewOffset)
        {
            if (stco.Data.Length == coPos)
            {
                List<int> lsizes = new List<int>();
                
                while(stsz.Length - 8 > szPos)
                {
                    int size = GetInt32(stsz.Data, szPos);
                    lsizes.Add(size);
                    szPos += 4;
                }

                NewOffset = 0;
                return new Sample(BaseOffset, lsizes);
            }

            int chunk_offset = GetInt32(stco.Data, coPos);
            coPos += 4;

            List<int> sizes = new List<int>();

            int count = 0;
            while(chunk_offset - BaseOffset > count)
            {
                int size = GetInt32(stsz.Data, szPos);
                sizes.Add(size);
                count += size;
                szPos += 4;
            }

            NewOffset = chunk_offset;
            
            return new Sample(BaseOffset, sizes);
        }

        private int GetInt32(byte[] Size, int Offset)
        {
            byte[] asb = new byte[4];
            Array.Copy(Size, Offset, asb, 0, 4);
            Array.Reverse(asb);

            return BitConverter.ToInt32(asb, 0);
        }

        private struct Sample
        {
            public Sample(int Offset, List<int> Size)
            {
                this.Offset = Offset;
                this.Size = Size;
            }

            public int Offset { get; set; }
            public List<int> Size { get; set; }

            public int GetTotalSize()
            {
                int size = 0;
                for (int i = 0; Size.Count > i; i++)
                    size += Size[i];

                return size;
            }

            public byte[] GetOffsetBytes()
            {
                byte[] ret = BitConverter.GetBytes(Offset);
                Array.Reverse(ret);
                return ret;
            }
        }
    }
}
