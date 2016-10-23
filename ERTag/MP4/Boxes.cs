using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWTag.MP4
{
    //cref=http://atomicparsley.sourceforge.net/mpeg-4files.html

    public class Atoms : List<Atom> { }
    public class Atom
    {
        public Atom() { }

        public Atom(Encoding Encode, byte[] Header)
        {
            this.Encode = Encode;

            Array.Reverse(Header, 0, 4);
            Length = BitConverter.ToInt32(Header, 0);
            Name = Encode.GetString(Header, 4, 4);
        }

        public Atom(Encoding Encode, byte[] Length, byte[] Name)
        {
            this.Encode = Encode;
            this.Length = BitConverter.ToInt32(Length, 0);
            this.Name = Encode.GetString(Name, 0, 4);
        }

        public Atom(Encoding Encode)
        {
            this.Encode = Encode;
        }

        public Atoms Children { get; set; } = new Atoms();
        protected Encoding Encode { get; set; }
        public int Length { get; set; }
        public string Name { get; set; }
        public byte[] Data { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class StringAtom : Atom
    {
        public StringAtom(Atom BaseAtom)
        {
            Length = BaseAtom.Length;
            Name = BaseAtom.Name;
            Data = BaseAtom.Data;

            byte[] actual = new byte[Data.Length - 8];
            Array.Copy(Data, 8, actual, 0, actual.Length);
            Text = Encode.GetString(actual, 0, actual.Length);
        }

        public string Text { get; set; }
    }
}
