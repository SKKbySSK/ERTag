using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RWTag.MP4
{
    public class Tag : BaseTag
    {
        public Regex AtomNameRegex { get; set; } = new Regex("^[a-z][a-z][a-z][a-z]$");

        public string[] ReduceAtoms { get; set; } = new string[] { "free", "mdat", "data" };

        public Tag(SettableStream Stream) : base(Stream) { Encode = Encoding.UTF8; }

        public override string[] Extensions { get; } = new string[] { ".mp4", ".m4a" };

        public override bool IsReadable()
        {
            return true;
        }

        public override RWTag.Tag Read()
        {
            RWTag.Tag tag = new RWTag.Tag();

            Atoms atoms = Parse();


            return tag;
        }

        public override void SetStream(Stream Stream)
        {
            this.Stream.SetStream(Stream);
        }

        public override void Write(RWTag.Tag Tag)
        {
            throw new NotImplementedException();
        }

        private Atoms Parse()
        {
            Stream.Seek(0, SeekOrigin.Begin);

            Atoms atoms = new Atoms();
            BinaryReader br = new BinaryReader(Stream, Encode, true);
            byte[] header = new byte[8];
            while(br.Read(header, 0, 8) == 8)
            {
                Atom atom = new Atom(Encode, header);
                if (!ReduceAtoms.Contains(atom.Name) && atom.Length - 8 > 0 && Stream.Length > Stream.Position + atom.Length)
                {
                    atom.Data = br.ReadBytes(atom.Length - 8);

                    MemoryStream ms = new MemoryStream(atom.Data);
                    DeepSearch(atom, ms);
                    ms.Dispose();

                    atoms.Add(atom);
                }
            }

            return atoms;
        }

        private void DeepSearch(Atom Atom, MemoryStream DataStream)
        {
            if (DataStream.Length - DataStream.Position < 8) return;
             
            BinaryReader br = new BinaryReader(DataStream, Encode, true);
            Atom atom = new Atom(Encode, br.ReadBytes(8));

            if (AtomNameRegex.IsMatch(atom.Name) && atom.Length - 8 > 0)
            {
                System.Diagnostics.Debug.WriteLine(atom.Name);
                if (!ReduceAtoms.Contains(atom.Name))
                {
                    if (DataStream.Length <= DataStream.Position + atom.Length)
                        return;
                    Atom.Children.Add(atom);
                    atom.Data = br.ReadBytes(atom.Length - 8);
                    br.Dispose();

                    MemoryStream ms = new MemoryStream(atom.Data);
                    DeepSearch(atom, ms);
                }
            }

            DeepSearch(Atom, DataStream);
        }
    }
}
