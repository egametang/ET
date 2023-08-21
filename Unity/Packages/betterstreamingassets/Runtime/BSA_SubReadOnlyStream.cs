// Better Streaming Assets, Piotr Gwiazdowski <gwiazdorrr+github at gmail.com>, 2017

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Better.StreamingAssets
{
    internal class SubReadOnlyStream: Stream
    {
        private readonly long m_offset;
        private readonly bool m_leaveOpen;

        private long? m_length;
        private Stream m_actualStream;
        private long m_position;
        
        public SubReadOnlyStream(Stream actualStream, bool leaveOpen = false)
        {
            if (actualStream == null)
                throw new ArgumentNullException("superStream");

            m_actualStream = actualStream;
            m_leaveOpen = leaveOpen;
        }

        public SubReadOnlyStream(Stream actualStream, long offset, long length, bool leaveOpen = false)
            : this(actualStream, leaveOpen)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length");

            Debug.Assert(offset <= actualStream.Length);
            Debug.Assert(actualStream.Length >= length);
            Debug.Assert(offset + length <= actualStream.Length);

            m_offset = offset;
            m_position = offset;
            m_length = length;
        }

        public override long Length
        {
            get
            {
                ThrowIfDisposed();

                if (!m_length.HasValue)
                    m_length = m_actualStream.Length - m_offset;
                
                return m_length.Value;
            }
        }

        public override long Position
        {
            get
            {
                ThrowIfDisposed();
                return m_position - m_offset;
            }
            set
            {
                ThrowIfDisposed();
                m_position = m_offset + value;
            }
        }

        public override bool CanRead { get { return m_actualStream.CanRead; } }

        public override bool CanSeek { get { return m_actualStream.CanSeek; } }

        public override bool CanWrite { get { return false; } }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfCantRead();
            ThrowIfDisposed();

            if ( m_actualStream.Position != m_position )
                m_actualStream.Seek(m_position, SeekOrigin.Begin);

            if ( m_length.HasValue )
            {
                var endPosition = m_offset + m_length.Value;
                if (m_position + count > endPosition)
                {
                    count = (int)(endPosition - m_position);
                }
            }

            int bytesRead = m_actualStream.Read(buffer, offset, count);
            m_position += bytesRead;
            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfDisposed();

            if ( origin == SeekOrigin.Begin )
            {
                m_position = m_actualStream.Seek(m_offset + offset, SeekOrigin.Begin);
            }
            else if ( origin == SeekOrigin.End )
            {
                m_position = m_actualStream.Seek(m_offset + Length + offset, SeekOrigin.Begin);
            }
            else
            {
                m_position = m_actualStream.Seek(offset, SeekOrigin.Current);
            }
            return m_position - m_offset;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        // Close the stream for reading.  Note that this does NOT close the superStream (since
        // the substream is just 'a chunk' of the super-stream
        protected override void Dispose(bool disposing)
        {
            if ( disposing )
            {
                if (m_actualStream != null)
                {
                    if (!m_leaveOpen)
                        m_actualStream.Dispose();

                    m_actualStream = null;
                }
            }


            base.Dispose(disposing);
        }

        private void ThrowIfDisposed()
        {
            if (m_actualStream == null)
                throw new ObjectDisposedException(GetType().ToString(), "");
        }

        private void ThrowIfCantRead()
        {
            if (!CanRead)
                throw new NotSupportedException();
        }
    }
}
