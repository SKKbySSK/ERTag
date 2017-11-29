using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace RWTag
{
    public enum ImageMIME
    {
        jpeg, png, bmp, gif, none
    }

    public struct Tag
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }
        public int Track { get; set; }
        public int TotalTrack { get; set; }
        public int DiscNumber { get; set; }
        public int TotalDiscNumber { get; set; }
        public string Genre { get; set; }
        public byte[] Image { get; set; }
        public ImageMIME ImageMIMEType { get; set; }
        public string ImageDescription { get; set; }
        public string Lyrics { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }

    public class TagReader : IDisposable
    {
        public TagReader() { Collection = new TagPrioritizedCollection(); }
        public TagReader(BaseTag[] Tags) { Collection = new TagPrioritizedCollection(Tags); }
        public TagPrioritizedCollection Collection { get; set; }

        public void Dispose()
        {
            for(int i = 0;Collection.Count > i; i++)
            {
                Collection[i].Dispose();
            }
        }

        public Tag GetTag(Stream Stream, string Extension)
        {
            Tag tag = new Tag();
            string ext = Extension.ToLower();
            for (int i = 0;Collection.Count > i; i++)
            {
                try
                {
                    if(Collection[i].Extensions.Select(tex => tex.ToLower()).Contains(ext))
                    {
                        Collection[i].SetStream(Stream);
                        tag = Collection[i].Read();
                        if (!string.IsNullOrEmpty(tag.Name)) return tag;
                    }
                }
                catch (Exception) { }
            }

            return tag;
        }

        public Tag GetTag(Stream Stream)
        {
            Tag tag = new Tag();
            for (int i = 0; Collection.Count > i; i++)
            {
                try
                {
                    Collection[i].SetStream(Stream);
                    tag = Collection[i].Read();
                    if (!string.IsNullOrEmpty(tag.Name)) return tag;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }

            return tag;
        }

        public void SetTag(Stream Stream, string Extension, Tag Tag)
        {
            string ext = Extension.ToLower();
            for (int i = 0; Collection.Count > i; i++)
            {
                try
                {
                    for (int j = 0; Collection[i].Extensions.Length > j; j++)
                    {
                        string sup = Collection[i].Extensions[j].ToLower();
                        if (ext == sup)
                        {
                            Collection[i].SetStream(Stream);
                            Collection[i].Write(Tag);
                            return;
                        }
                    }
                }
                catch (Exception) { }
            }
        }
    }

    public class TagPrioritizedCollection : ICollection<BaseTag>
    {
        private List<BaseTag> Tags;
        public TagPrioritizedCollection()
        {
            Tags = new List<BaseTag>(new BaseTag[] 
            {
                new MP3.ID3v2.Tag(new SettableStream()),
                new MP3.ID3v1.Tag(new SettableStream()),
                new MP4.Tag(new SettableStream()),
                new FLAC.Tag(new SettableStream())
            });
        }
        public TagPrioritizedCollection(BaseTag[] Tags) { this.Tags = new List<BaseTag>(Tags); }

        public BaseTag this[int i]
        {
            get { return Tags[i]; }
            set { Tags[i] = value; }
        }

        public int Count
        {
            get { return Tags.Count; }
        }

        public bool IsReadOnly { get; } = false;

        public void Add(BaseTag item)
        {
            Tags.Add(item);
        }

        public void Clear()
        {
            Tags.Clear();
        }

        public bool Contains(BaseTag item)
        {
            return Tags.Contains(item);
        }

        public void CopyTo(BaseTag[] array, int arrayIndex)
        {
            Tags.CopyTo(array, arrayIndex);
        }

        public IEnumerator<BaseTag> GetEnumerator()
        {
            return Tags.GetEnumerator();
        }

        public bool Remove(BaseTag item)
        {
            return Tags.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this;
        }

        public bool SetPriority(BaseTag item)
        {
            bool s = false;
            if (!Tags.Contains(item)) return false;
            Tags.Remove(item);
            Tags.Insert(0, item);
            return s;
        }
    }

    public class SettableStream : Stream
    {
        private Stream bs;
        public SettableStream() { }
        public SettableStream(Stream BaseStream) { bs = BaseStream; }

        public void SetStream(Stream Stream)
        {
            bs = Stream;
        }

        public override bool CanRead
        {
            get
            {
                if (bs == null) return false;
                else return bs.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (bs == null) return false;
                else return bs.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (bs == null) return false;
                else return bs.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                if (bs == null) return 0;
                else return bs.Length;
            }
        }

        public override long Position
        {
            get
            {
                if (bs == null) return 0;
                else return bs.Position;
            }

            set
            {
                if (bs != null)
                    bs.Position = value;
            }
        }

        public override void Flush()
        {
            if (bs != null) bs.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (bs == null) return 0;
            else return bs.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (bs == null) return 0;
            else return bs.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            if (bs != null) bs.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (bs != null) bs.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (bs != null) bs.Dispose();
            base.Dispose(disposing);
        }
    }

    public abstract class BaseTag : IDisposable
    {
        protected SettableStream Stream { get; set; }
        public BaseTag(SettableStream Stream) { this.Stream = Stream; }
        public BaseTag(Stream BaseStream) { Stream = new SettableStream(BaseStream); }

        public Encoding Encode { get; protected set; } = Encoding.Unicode;

        public virtual void SetStream(Stream Stream)
        {
            this.Stream.SetStream(Stream);
        }

        public abstract bool IsReadable();
        public abstract string[] Extensions { get; }
        public abstract Tag Read();
        public abstract void Write(Tag Tag);
        
        protected virtual void Release()
        {

        }

        public void Dispose()
        {
            Release();
            Stream.Dispose();
        }
    }
    
    public class TagReaderException : Exception
    {
        public TagReaderException() { }
        public TagReaderException(string message) : base(message) { }
    }
    
    public class TagWriterException : Exception
    {
        public TagWriterException() { }
        public TagWriterException(string message) : base(message) { }
    }
}
