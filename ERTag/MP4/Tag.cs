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

            byte[] header = new byte[8];
            while(Stream.Read(header, 0, 8) == 8)
            {
                Atom atom = new Atom(Encode, header);
                DeepSearch(atom);
                atoms.Add(atom);
            }

            return atoms;
        }

        private void DeepSearch(Atom Atom)
        {
            byte[] header = new byte[8];
            while (Stream.Read(header, 0, 8) == 8)
            {
                Atom child = new Atom(Encode, header);
                if (IsCorrectName(child.Name))
                {
                    DeepSearch(child);
                    Atom.Children.Add(child);
                }
                else
                {
                    Stream.Seek(Atom.Length - 16, SeekOrigin.Current);
                    break;
                }
            }
        }

        private bool IsCorrectName(string Name)
        {
            if (Name.Length == 4)
            {
                int min = 'a', max = 'z';
                for(int i = 0;Name.Length > i; i++)
                {
                    if (min > Name[i]) return false;
                    if (max < Name[i]) return false;
                }

                return true;
            }
            else return false;
        }
    }
}
