using System.Runtime.InteropServices;

namespace HNIdesu.IO
{
    public class BinaryReader(Stream stream) : IDisposable
    {
        public Stream BaseStream { get; set; } = stream;
        public bool IsBigEndian { get; set; } = false;

        public uint ReadUInt32()
        {
            byte[] buffer = new byte[4];
            BaseStream.Read(buffer, 0, 4);
            if (IsBigEndian)
                buffer=buffer.Reverse().ToArray();
            return BitConverter.ToUInt32(buffer, 0);
        }

        public ulong ReadUInt64()
        {
            byte[] buffer = new byte[8];
            BaseStream.Read(buffer, 0, 8);
            if (IsBigEndian)
                buffer=buffer.Reverse().ToArray();
            return BitConverter.ToUInt64(buffer, 0);
        }

        public int ReadInt32()
        {
            byte[] buffer = new byte[4];
            BaseStream.Read(buffer, 0, 4);
            if (IsBigEndian)
                buffer=buffer.Reverse().ToArray();
            return BitConverter.ToInt32(buffer, 0);
        }

        public long ReadInt64()
        {
            byte[] buffer = new byte[8];
            BaseStream.Read(buffer, 0, 8);
            if (IsBigEndian)
                buffer=buffer.Reverse().ToArray();
            return BitConverter.ToInt64(buffer, 0);
        }

        public short ReadInt16()
        {
            byte[] buffer = new byte[2];
            BaseStream.Read(buffer, 0, 2);
            if (IsBigEndian)
                buffer=buffer.Reverse().ToArray();
            return BitConverter.ToInt16(buffer, 0);
        }

        public ushort ReadUInt16()
        {
            byte[] buffer = new byte[2];
            BaseStream.Read(buffer, 0, 2);
            if (IsBigEndian)
                buffer=buffer.Reverse().ToArray();
            return BitConverter.ToUInt16(buffer,0);
        }


        public void Read(Action<BinaryReader> action)
        {
            action(this);
        }
        public T Read<T>(Func<BinaryReader,T> func)
        {
            return func(this);
        }


        public byte[] ReadBytes(int count)
        {
            byte[] buffer = new byte[count];
            BaseStream.Read(buffer, 0, count);
            return buffer;
        }

        public byte[] ReadBytes(int count,int offset,SeekOrigin origin)
        {
            long mark = BaseStream.Position;
            BaseStream.Seek(offset, origin);
            byte[] buffer = new byte[count];
            BaseStream.Read(buffer, 0, count);
            BaseStream.Position = mark;
            return buffer;
        }


        public int ReadByte()
        {
            return BaseStream.ReadByte();
        }



        public void SeekAndRead(long offset,SeekOrigin seekOrigin,Action<BinaryReader> action)
        {
            long mark= BaseStream.Position;
            BaseStream.Seek(offset,seekOrigin);
            action(this);
            BaseStream.Position=mark;
            return;
        }

        public void Close()
        {
            BaseStream.Close();          
        }

        /// <summary>
        /// 读取出定长结构体
        /// </summary>
        /// <typeparam name="T">结构体的类型</typeparam>
        /// <param name="size">结构体的大小</param>
        /// <returns></returns>
        public T ReadMarshal<T>(int size) where T : struct
        {
            byte[] buffer = new byte[size];
            int readed= BaseStream.Read(buffer, 0, size);
            if (readed == 0)
                throw new EndOfStreamException();
            IntPtr ptr= Marshal.AllocHGlobal(size);
            Marshal.Copy(buffer, 0, ptr, size);
            T temp=Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);
            return temp;
        }

        public void Dispose() => Close();
    }
}
