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
        public bool RemoveAtom(string Name, bool SearchChildren)
        {
            if (Name.Length == 4)
            {
                if (!SearchChildren)
                {
                    for (int i = 0; Count > i; i++)
                    {
                        if (this[i].Name == Name)
                        {
                            RemoveAt(i);
                            UpdateAtomData();
                            return true;
                        }
                    }

                    return false;
                }
                else
                {
                    //TODO Implement
                    return false;
                }
            }
            else
                return false;
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
            Data = new Dictionary<byte[], ilstAtom>();
            int count = 0;

            byte[] sizeb = new byte[4];
            while(Length > count)
            {
                ms.Read(sizeb, 0, 4);
                Array.Reverse(sizeb);
                int len = BitConverter.ToInt32(sizeb, 0);

                byte[] bytes = new byte[len];
                len += ms.Read(bytes, 0, len - 4);
                ilstAtom atom = new ilstAtom(Encode, bytes);
                Data.Add(atom.Name, atom);
                count += len;
            }
        }

        private Encoding Encode { get; set; }
        public int Length { get; set; }
        public string Name { get; set; }
        public Dictionary<byte[], ilstAtom> Data { get; set; }

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

            public override string ToString()
            {
                return Encode.GetString(Name, 0, 4);
            }
        }
    }
}
