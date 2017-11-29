using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RWTag.Utils;

namespace RWTag.MP3.ID3v1
{
    public class Tag : BaseTag
    {
        const int Length = 128;

        public override string[] Extensions { get; } = new string[] { ".mp3" };

        public Tag(SettableStream Stream) : base(Stream) { Encode = EncodingProvider.ShiftJis; }

        public enum Version { v1, v1_1, Unknown }
        public Version GetVersion()
        {
            if (Stream.Length < Length) return Version.Unknown;

            if (Reader.CheckID(Stream) == false)
                return Version.Unknown;

            Stream.Seek(-3, SeekOrigin.End);
            int b = Stream.ReadByte();
            if (b == 0x00) return Version.v1_1;
            else return Version.v1;
        }

        public override bool IsReadable()
        {
            if (Stream.Length <= 0) return false;
            Stream.Seek(-3, SeekOrigin.End);
            int b = Stream.ReadByte();
            return GetVersion() != Version.Unknown;
        }

        public override RWTag.Tag Read()
        {
            return Reader.Read(Stream, GetVersion(), Encode);
        }

        public override void Write(RWTag.Tag Tag)
        {
            if (Reader.CheckID(Stream)) Stream.Seek(-Length, SeekOrigin.End);
            else Stream.Seek(0, SeekOrigin.End);

            byte[] buffer = new byte[Length];
            BinaryWriter bw = new BinaryWriter(Stream, Encode);
            List<byte> data = new List<byte>(buffer.Length);
            data.AddRange(new byte[] { 0x54, 0x41, 0x47 }); //TAG
            data.AddRange(ByteConverter.GetFilledBytes(Tag.Title, Encode, 0, 30));
            data.AddRange(ByteConverter.GetFilledBytes(Tag.Artist, Encode, 0, 30));
            data.AddRange(ByteConverter.GetFilledBytes(Tag.Album, Encode, 0, 30));
            data.AddRange(ByteConverter.GetFilledBytes(Tag.Date.Year.ToString(), Encode, 0, 4));
            data.AddRange(ByteConverter.GetFilledBytes(Tag.Comment, Encode, 0, 28));
            data.Add(0);
            data.Add((byte)Tag.Track);

            if (string.IsNullOrEmpty(Tag.Genre))
                data.Add(0);
            else
            {
                int index = -1;
                for (int i = 0; Reader.Genres.Length > i; i++)
                    if (Reader.Genres[i] == Tag.Genre)
                    {
                        index = i;
                        break;
                    }
                if (index > -1)
                    data.Add((byte)index);
                else
                    data.Add(0);
            }

            bw.Write(data.ToArray());
            Stream.Flush();
        }
    }

    internal static class Reader
    {
        const int Length = 128;
        public static RWTag.Tag Read(Stream Stream, Tag.Version Version, Encoding Encode)
        {
            if (Version == Tag.Version.Unknown) throw new TagReaderException("対応していないタグです");
            RWTag.Tag tag = new RWTag.Tag();
            BinaryReader reader = new BinaryReader(Stream, Encode, true);

            Stream.Seek(-Length, SeekOrigin.End);
            tag.Name = new string(reader.ReadChars(3));
            if (tag.Name != "TAG") throw new TagReaderException("識別子が正しくありません");
            tag.Title = ByteConverter.GetString(reader, 30, Encode).TrimEnd(new char[] { '\0' });
            tag.Artist = ByteConverter.GetString(reader, 30, Encode).TrimEnd(new char[] { '\0' });
            tag.Album = ByteConverter.GetString(reader, 30, Encode).TrimEnd(new char[] { '\0' });

            string comment;
            int date;
            SearchTagCommentAndDate(Stream, Version, Encode, out comment, out date);
            if (date > 0) tag.Date = new DateTime(date, 1, 1);
            tag.Comment = comment.TrimEnd(new char[] { '\0' });

            if (Version == Tag.Version.v1_1)
                tag.Track = reader.ReadSByte();

            sbyte genre = reader.ReadSByte();
            if (Genres.Length > genre) tag.Genre = Genres[genre];

            reader.Dispose();

            return tag;
        }

        private static void SearchTagCommentAndDate(Stream Stream, Tag.Version Version, Encoding Encode, out string Comment, out int Date)
        {
            BinaryReader reader = new BinaryReader(Stream, Encode, true);
            Stream.Seek(-35, SeekOrigin.End);

            bool s = int.TryParse(ByteConverter.GetString(reader, 4, Encode), out Date);
            if (!s) Date = -1;
            
            if (Version == Tag.Version.v1)
                Comment = ByteConverter.GetString(reader, 30, Encode);
            else
            {
                Comment = ByteConverter.GetString(reader, 28, Encode);
                reader.ReadByte();
            }

            reader.Dispose();
        }

        public static bool CheckID(Stream Stream)
        {
            if (Stream.Length < Length) return false;
            Stream.Seek(-Length, SeekOrigin.End);
            byte[] tag = new byte[3];
            Stream.Read(tag, 0, 3);

            return tag.SequenceEqual(new byte[] { 0x54, 0x41, 0x47 });
        }

        #region Genres
        internal static string[] Genres = new string[]
        {
            "ブルース",
            "Classic Rock",
            "カントリー",
            "ダンス",
            "ディスコ",
            "ファンク",
            "グランジ",
            "ヒップホップ",
            "ジャズ",
            "メタル",
            "ニューエイジ",
            "オールディーズ",
            "その他",
            "ポップ",
            "R&B",
            "ラップ",
            "レゲエ",
            "ロック",
            "テクノ",
            "Industrial",
            "オルタナティブ",
            "スカ",
            "デスメタル",
            "w:Pranks",
            "サウンドトラック",
            "Euro-Techno",
            "環境",
            "Trip-hop",
            "ボーカル",
            "Jazz+Funk",
            "フュージョン",
            "トランス",
            "クラシカル",
            "Instrumental",
            "Acid",
            "ハウス",
            "ゲームミュージック",
            "Sound Clip",
            "ゴスペル",
            "ノイズ",
            "Alt. Rock",
            "バス",
            "ソウル",
            "パンク",
            "Space",
            "Meditative",
            "Instrumental pop",
            "Instrumental rock",
            "フォークソング",
            "Gothic",
            "Darkwave",
            "Techno-Industrial",
            "Electronic",
            "Pop-Folk",
            "Eurodance",
            "Dream",
            "サザン・ロック",
            "喜劇",
            "Cult",
            "Gangsta",
            "Top ",
            "Christian Rap",
            "Pop/Funk",
            "Jungle",
            "Native American",
            "Cabaret",
            "ニューウェーブ",
            "Psychedelic",
            "Rave",
            "Showtunes",
            "Trailer",
            "Lo-Fi",
            "Tribal",
            "Acid Punk",
            "アシッドジャズ",
            "ポルカ",
            "Retro",
            "Musical",
            "Rock & Roll",
            "ハードロック",
            "フォーク",
            "フォークロック",
            "National Folk",
            "スウィング",
            "Fast Fusion",
            "ビバップ",
            "ラテン",
            "Revival",
            "Celtic",
            "ブルーグラス",
            "Avantgarde",
            "Gothic Rock",
            "プログレッシブ・ロック",
            "Psychedelic Rock",
            "Symphonic Rock",
            "Slow Rock",
            "Big Band",
            "Chorus",
            "Easy Listening",
            "アコースティック",
            "ユーモア",
            "Speech",
            "シャンソン",
            "オペラ",
            "Chamber Music",
            "ソナタ",
            "交響曲",
            "Booty Bass",
            "Primus",
            "Porn Groove",
            "Satire",
            "Slow Jam",
            "Club",
            "タンゴ",
            "Samba",
            "Folklore",
            "バラッド",
            "Power Ballad",
            "Rhythmic Soul",
            "Freestyle",
            "Duet",
            "パンク・ロック",
            "Drum Solo",
            "ア・カペラ",
            "Euro-House",
            "Dance Hall",
            "Goa",
            "ドラムンベース",
            "Club-House",
            "Hardcore",
            "Terror",
            "Indie",
            "ブリットポップ",
            "Negerpunk",
            "Polsk Punk",
            "Beat",
            "Christian gangsta rap",
            "ヘヴィメタル",
            "ブラックメタル",
            "クロスオーバー",
            "Contemporary Christian",
            "Christian Rock",
            "Merengue",
            "Salsa",
            "スラッシュメタル",
            "アニメ",
            "JPop",
            "Synthpop"
        };
        #endregion
    }
}
