using System;
using System.IO;
using System.Reactive.Subjects;

namespace UnitystationLauncher.Infrastructure
{
    public class ProgressStream : Stream
    {
        public ProgressStream(Stream inner)
        {
            Inner = inner;
        }

        public Stream Inner {get;set;}
        public IObservable<long> Progress => progress;
        private readonly Subject<long> progress = new Subject<long>();
        private long position;

        public override bool CanRead => Inner.CanRead;

        public override bool CanSeek => Inner.CanSeek;

        public override bool CanWrite => Inner.CanWrite;

        public override long Length => Inner.Length;

        public override long Position { get => Inner.Position; set => Inner.Position = value; }

        public override void Flush()
        {
            Inner.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            position += count;
            progress.OnNext(position);
            return Inner.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            Inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Inner.Write(buffer, offset, count);
        }
    }
}