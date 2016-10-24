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

            Atoms atoms = Parse(false);


            return tag;
        }

        public override void SetStream(Stream Stream)
        {
            this.Stream.SetStream(Stream);
        }

        public override void Write(RWTag.Tag Tag)
        {
            Atoms atoms = Parse(true);
            atoms.RemoveAt(3);
            atoms.RemoveAt(2);
            atoms.RemoveAt(0);

            Stream.Position = 0;
            Stream.SetLength(atoms.GetLength());
            Stream.Write(atoms.ToBytes(), 0, atoms.GetLength());
            Stream.Flush();
        }

        private Atoms Parse(bool ReadData)
        {
            Stream.Seek(0, SeekOrigin.Begin);

            Atoms atoms = new Atoms();

            byte[] header = new byte[8];
            while(Stream.Read(header, 0, 8) == 8)
            {
                Atom atom = new Atom(Encode, Stream.Position - 8, header);
                DeepSearch(atom, ReadData);
                atoms.Add(atom);
            }

            return atoms;
        }

        private long DeepSearch(Atom Atom, bool ReadData)
        {
            long count = 0;

            byte[] header = new byte[8];
            while (Atom.Length > count)
            {
                Stream.Read(header, 0, 8);
                count += 8;

                Atom child = new Atom(Encode, Stream.Position - 8, header);
                System.Diagnostics.Debug.WriteLine(child.Name + " - " + child.Offset);
                if (IsCorrectName(child.Name))
                {
                    count += DeepSearch(child, ReadData);
                    Atom.Children.Add(child);
                }
                else
                {
                    count = Atom.Length;

                    switch (Atom.Name)
                    {
                        case "meta":
                            Atom.Data = new byte[Atom.Length - 8];
                            Array.Copy(header, 0, Atom.Data, 0, 8);
                            Stream.Read(Atom.Data, 8, Atom.Length - 16);
                            metaAtom meta = new metaAtom(Atom);
                            Atom = meta;
                            break;
                        default:
                            if (ReadData)
                            {
                                Atom.Data = new byte[Atom.Length - 8];
                                Array.Copy(header, 0, Atom.Data, 0, 8);
                                Stream.Read(Atom.Data, 8, Atom.Length - 16);
                            }
                            else
                                Stream.Seek(Atom.Length - 16, SeekOrigin.Current);
                            break;
                    }
                    break;
                }
            }

            return count;
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
