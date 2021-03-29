using System;
using System.IO;
using System.Reactive.Subjects;

namespace UnitystationLauncher.Models
{
    public class ProgressStream : Stream
    {
        public ProgressStream(Stream inner)
        {
            Inner = inner;
        }

        public Stream Inner { get; set; }
        public IObservable<long> Progress => _progress;
        private readonly Subject<long> _progress = new Subject<long>();
        private long _position;

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
            var r = Inner.Read(buffer, offset, count);
            _position += r;
            _progress.OnNext(_position);
            return r;
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