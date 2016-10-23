using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RWTag.MP3.ID3v2
{
    internal class Three : ID3v2Reader
    {
        private const byte SynchsafeDefault = 0;

        public Three(Stream Stream, Encoding Encode) : base(Stream, Encode)
        {
        }

        public override RWTag.Tag Read()
        {
            RWTag.Tag tag = new RWTag.Tag();
            tag.Data = new Dictionary<string, object>();
            tag.Date = new DateTime();

            Stream.Seek(3, SeekOrigin.Begin);
            tag.Name = "ID3v2." + Stream.ReadByte() + "." + Stream.ReadByte();

            int size = TotalTagSize;
            Stream.Seek(10, SeekOrigin.Begin);
            BinaryReader br = new BinaryReader(Stream, Encode, true);
            List<Frame> Frames = Parse(br, size);

            Date date = new Date();
            for (int i = 0; Frames.Count > i; i++)
            {
                try
                {
                    tag.Data.Add(Frames[i].ID, Frames[i]);
                    StringFrame sf = Frames[i] as StringFrame;
                    if (sf != null)
                    {
                        switch (sf.ID)
                        {
                            case "TIT2":
                                tag.Title = sf.Text;
                                break;

                            case "TPE1":
                                tag.Artist = sf.Text;
                                break;

                            case "TALB":
                                tag.Album = sf.Text;
                                break;

                            case "TYER":
                                int year;
                                if (int.TryParse(sf.Text, out year))
                                    date.Year = year;
                                else
                                    date.Year = 0;
                                break;

                            case "COMM":
                                tag.Comment = sf.Text;
                                break;

                            case "TRCK":
                                int trck, ttrck;
                                ParseNumbers(sf.Text, out trck, out ttrck);
                                tag.Track = trck;
                                tag.TotalTrack = ttrck;
                                break;

                            case "TCON":
                                tag.Genre = sf.Text;
                                break;

                            case "TPOS":
                                int dn, tdn;
                                ParseNumbers(sf.Text, out dn, out tdn);
                                tag.DiscNumber = dn;
                                tag.TotalDiscNumber = tdn;
                                break;
                        }
                    }

                    USLTFrame lyrics = Frames[i] as USLTFrame;
                    if (lyrics != null)
                        tag.Lyrics = lyrics.Lyrics;

                    APICFrame apic = Frames[i] as APICFrame;
                    if (apic != null)
                        tag.Image = apic.PictureData;
                }
                catch (Exception) { }
            }
            tag.Date = date.GetDateTime();
            br.Dispose();

            return tag;
        }

        public override void Write(RWTag.Tag Tag, byte[] RawData)
        {
            List<Frame> frames = new List<Frame>();
            if (!string.IsNullOrEmpty(Tag.Title)) frames.Add(new StringFrame("TIT2", Tag.Title));

            if (!string.IsNullOrEmpty(Tag.Artist)) frames.Add(new StringFrame("TPE1", Tag.Artist));

            if (!string.IsNullOrEmpty(Tag.Album)) frames.Add(new StringFrame("TALB", Tag.Album));

            if (!string.IsNullOrEmpty(Tag.Genre)) frames.Add(new StringFrame("TCON", Tag.Genre));

            if (Tag.Date.Year > 0) frames.Add(new StringFrame("TYER", Tag.Date.Year.ToString()));

            if (Tag.Track > 0)
            {
                if (Tag.TotalTrack > 0)
                    frames.Add(new StringFrame("TRCK", Tag.Track + "/" + Tag.TotalTrack));
                else
                    frames.Add(new StringFrame("TRCK", Tag.Track.ToString()));
            }

            if (Tag.DiscNumber > 0)
            {
                if (Tag.TotalDiscNumber > 0)
                    frames.Add(new StringFrame("TPOS", Tag.DiscNumber + "/" + Tag.TotalDiscNumber));
                else
                    frames.Add(new StringFrame("TPOS", Tag.DiscNumber.ToString()));
            }

            if (Tag.ImageMIMEType != ImageMIME.none && Tag.Image != null)
            {
                if (Tag.Image.Length > 0)
                {
                    frames.Add(new APICFrame("image/" + Enum.GetName(typeof(ImageMIME), Tag.ImageMIMEType), 3, Tag.ImageDescription, Tag.Image));
                }
            }

            Stream.Seek(0, SeekOrigin.Begin);
            BinaryWriter bw = new BinaryWriter(Stream, Encode, true);

            List<byte> wdata = new List<byte>();
            for (int i = 0; frames.Count > i; i++)
            {
                wdata.AddRange(FrameToByte(frames[i], Encode));
            }

            wdata.InsertRange(0, CreateHeader(wdata.Count));

            wdata.AddRange(RawData);

            bw.Write(wdata.ToArray());

            bw.Flush();
            bw.Dispose();
        }

        private byte[] CreateHeader(int size)
        {
            byte[] Header = new byte[10];
            Header[0] = 0x49; //I
            Header[1] = 0x44; //D
            Header[2] = 0x33; //3
            Header[3] = 0x03; //v2.3.0
            Header[4] = 0x00; //v2.3.0
            Header[5] = 0x00; //TODO Flag

            byte[] sizeb = Utils.ByteConverter.GetSynchsafeBytes(size);
            Header[6] = sizeb[0];
            Header[7] = sizeb[1];
            Header[8] = sizeb[2];
            Header[9] = sizeb[3];

            return Header;
        }

        private List<Frame> Parse(BinaryReader br, int Size)
        {
            List<Frame> frames = new List<Frame>();

            try
            {
                int count = 0;
                byte[] fid = new byte[4];
                while (Size > count)
                {
                    if (br.Read(fid, 0, 4) == 4)
                    {
                        try
                        {
                            string ID = Utils.ByteConverter.GetString(fid, Encode).Trim(new char[] { '\0' });
                            if (ID.Length < 4) break;
                            Frame f = new Frame();
                            f.ID = ID;

                            //v2.3はSynchsafeでない
                            f.Size = Utils.ByteConverter.GetIntFromHexadecimal(br, 4);

                            f.Flag = br.ReadBytes(2);
                            f.Data = br.ReadBytes(f.Size);

                            count += 10 + f.Size;

                            if (ID.StartsWith("T") && ID != "TXXX")
                            {
                                byte[] datawe = new byte[f.Data.Length - 1];
                                Array.Copy(f.Data, 1, datawe, 0, f.Data.Length - 1);
                                frames.Add(new StringFrame(f, GetEncode(f.Data[0]), GetBOM(datawe)));
                            }
                            else if (ID == "USLT")
                                frames.Add(new USLTFrame(f, GetEncode(f.Data[0])));
                            else if (ID == "APIC")
                                frames.Add(new APICFrame(f, GetEncode(f.Data[0])));
                            else
                                frames.Add(f);
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception) { }

            return frames;
        }

        private void ParseNumbers(string Text, out int First, out int Second)
        {
            if (int.TryParse(Text, out First))
                Second = 0;
            else if (Text.Contains("/"))
            {
                int index = Text.IndexOf('/');
                First = int.Parse(Text.Substring(0, index));
                Second = int.Parse(Text.Substring(index + 1, Text.Length - index - 1));
            }
            else
            {
                First = 0;
                Second = 0;
            }
        }

        private class APICFrame : Frame
        {
            public APICFrame(string MIMEType, byte PictureType, string Description, byte[] PictureData)
            {
                ID = "APIC";
                Flag = new byte[2];
                this.MIMEType = MIMEType;
                this.PictureType = PictureType;
                this.Description = Description;
                this.PictureData = PictureData;
                UpdateFrame();
            }

            public APICFrame(Frame BaseFrame, Encoding Encode)
            {
                ID = BaseFrame.ID;
                Size = BaseFrame.Size;
                Flag = BaseFrame.Flag;
                Data = BaseFrame.Data;
                BinaryReader br = new BinaryReader(new MemoryStream(BaseFrame.Data), Encode);
                br.ReadByte();

                MIMEType = Utils.ByteConverter.GetString(Utils.ByteConverter.GetBytesToReachNull(br, true), Encode);
                PictureType = br.ReadByte();
                Description = Utils.ByteConverter.GetString(Utils.ByteConverter.GetBytesToReachNull(br, true), Encode);
                PictureData = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));

                br.Dispose();
            }

            public string Description { get; set; }
            public string MIMEType { get; set; }
            public byte[] PictureData { get; set; }
            public byte PictureType { get; set; }

            private Encoding GetISO88591()
            {
                return Encoding.GetEncoding("ISO-8859-1");
            }

            private void UpdateFrame()
            {
                List<byte> data = new List<byte>();
                data.Add(0); //MIME Encoding(ISO-8859-1)
                data.AddRange(GetISO88591().GetBytes(MIMEType));
                data.Add(0);
                data.Add(PictureType);
                if (!string.IsNullOrEmpty(Description))
                    data.AddRange(GetISO88591().GetBytes(Description));
                data.Add(0);
                data.AddRange(PictureData);

                Size = data.Count;
                Data = data.ToArray();
            }
        }

        private class Date
        {
            public int Day { get; set; } = 1;
            public int Month { get; set; } = 1;
            public int Year { get; set; } = 1;

            public DateTime GetDateTime()
            {
                if (Year < 100) Year += 2000;
                if (Year < 1000) Year += 1000;
                return new DateTime(Year, Month, Day);
            }
        }

        private class StringFrame : Frame
        {
            private string tex;

            public StringFrame(string ID, string Text)
            {
                this.ID = ID;
                Flag = new byte[] { 0, 0 };
                this.Text = Text;
            }

            public StringFrame(Frame BaseFrame, Encoding Encode, BOM BOM)
            {
                ID = BaseFrame.ID;
                Size = BaseFrame.Size;
                Flag = BaseFrame.Flag;
                Data = BaseFrame.Data;
                this.BOM = BOM;
                BinaryReader br = new BinaryReader(new MemoryStream(BaseFrame.Data), Encode);

                br.ReadByte();
                int red = 1;
                if (BOM == BOM.UTF8)
                {
                    br.ReadBytes(3);
                    red += 3;
                }
                if (BOM == BOM.UTF16BE || BOM == BOM.UTF16LE)
                {
                    br.ReadBytes(2);
                    red += 2;
                }

                tex = Utils.ByteConverter.GetString(br.ReadBytes(Size - red), Encode).Trim(new char[] { '\0' });
                br.Dispose();
            }

            public string Text
            {
                get { return tex; }
                set
                {
                    if (value == null) value = "";
                    tex = value;

                    List<byte> Dbytes = new List<byte>();

                    int num;
                    bool s = int.TryParse(tex, out num);
                    bool appendNULL = false;

                    Encode = Encoding.Unicode;
                    if (!s)
                    {
                        if (ID == "TRCK" || ID == "TPOS")
                        {
                            Encode = Encoding.GetEncoding("ISO-8859-1");
                            BOM = BOM.Unknwon;
                            appendNULL = true;
                        }
                        else
                        {
                            Encode = new UnicodeEncoding(false, true);
                            BOM = BOM.UTF16LE;
                        }

                        Dbytes.Add(GetEncode());
                        Dbytes.AddRange(GetBOM());
                    }
                    else
                    {
                        Dbytes.Add(0);
                        Encode = Encoding.GetEncoding("ISO-8859-1");
                    }

                    Dbytes.AddRange(Encode.GetBytes(tex));
                    if (appendNULL) Dbytes.Add(0);
                    Size = Dbytes.Count;
                    Data = Dbytes.ToArray();
                }
            }

            private BOM BOM { get; set; } = BOM.Unknwon;
            private Encoding Encode { get; set; }

            private byte[] GetBOM()
            {
                byte[] bytes = Utils.ByteConverter.RemoveZero(BitConverter.GetBytes((int)BOM));
                Array.Reverse(bytes);
                return bytes;
            }

            private byte GetEncode()
            {
                if (Encode.WebName.ToUpper() == "ISO-8859-1") return 0;
                if (Encode.WebName.ToUpper() == "UTF-16" || Encode.WebName.ToUpper() == "UTF-16LE") return 1;
                if (Encode.WebName.ToUpper() == "UTF-16BE") return 2;
                if (Encode.WebName.ToUpper() == "UTF-8") return 3;
                return 0;
            }
        }

        private class USLTFrame : Frame
        {
            public USLTFrame(Frame BaseFrame, Encoding Encode)
            {
                ID = BaseFrame.ID;
                Size = BaseFrame.Size;
                Flag = BaseFrame.Flag;
                Data = BaseFrame.Data;
                BinaryReader br = new BinaryReader(new MemoryStream(BaseFrame.Data), Encode);
                br.ReadByte();
                Language = Utils.ByteConverter.GetString(br.ReadBytes(3), Encode);
                br.ReadByte();
                Lyrics = Utils.ByteConverter.GetString(br.ReadBytes(Size - 5), Encode);
                br.Dispose();
            }

            public string Language { get; set; }
            public string Lyrics { get; set; }
        }
    }
}