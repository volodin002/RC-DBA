using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;


namespace RC.DBA
{
    public class DbReaderStream : Stream
    {
        private DbDataReader reader;
        private int columnIndex;
        private long position;

        public DbReaderStream(DbDataReader reader, int columnIndex) {
            this.reader = reader;
            this.columnIndex = columnIndex;
        }

        public override long Position { get => position; set => throw new NotImplementedException(); }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long bytesRead = reader.GetBytes(columnIndex, position, buffer, offset, count);
            position += bytesRead;

            return (int)bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();


        public override bool CanRead => true;
        public override bool CanSeek => false;
        
        public override bool CanWrite => false;
                
        public override void Flush() =>throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();


        protected override void Dispose(bool disposing)
        {
            if (disposing && null != reader)
            {
                reader.Dispose();
                reader = null;
            }
            
            base.Dispose(disposing);
        }


    }

    public class DbStreamUpload : Stream
    {
        private long position;

        public DbCommand InsertCommand { get; set; }
        public DbCommand UpdateCommand { get; set; }
        public DbParameter InsertDataParam { get; set; }
        public DbParameter UpdateDataParam { get; set; }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] data = buffer;
            if (offset != 0 ||
                count != buffer.Length)
            {
                data = new byte[count];
                Array.Copy(buffer, offset, data, 0, count);
            }
            if (0 == position &&
                null != InsertCommand)
            {
                InsertDataParam.Value = data;
                InsertCommand.ExecuteNonQuery();
            }
            else
            {
                UpdateDataParam.Value = data;
                UpdateCommand.ExecuteNonQuery();
            }
            position += count;
        }

        public override long Position { get => position; set => position = value; }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        public override bool CanRead => false;
        public override bool CanWrite => true;

        public override bool CanSeek => false;

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void Flush()
        {
        }

        public override long Length => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();





    }
}
