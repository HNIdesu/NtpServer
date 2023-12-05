using System.Runtime.InteropServices;

namespace HNIdesu.IO
{
    public class BinaryWriter:IDisposable
    { 
        public Stream BaseStream { get;private set; }
        public bool IsBigEndian { get; set; } = false;
        public BinaryWriter(Stream stream)
        {
            BaseStream = stream;
        }

        public void WriteByte(byte item)
        {
            BaseStream.WriteByte(item);
        }

        public void WriteInt32(int item)
        {
            byte[] array = new byte[4];
            for(int i = 0; i <4; i++)
            {
                array[i] = (byte)item;
                item >>= 8;
            }
            if (IsBigEndian)
                array=array.Reverse().ToArray();
            BaseStream.Write(array, 0, 4);
        }

        public void WriteUInt32(uint item)
        {
            byte[] array = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                array[i] = (byte)item;
                item >>= 8;
            }
            if (IsBigEndian)
                array=array.Reverse().ToArray();
            BaseStream.Write(array, 0, 4);
        }

        public void WriteInt16(short item)
        {
            byte[] array = new byte[2];
            for (int i = 0; i < 2; i++)
            {
                array[i] = (byte)item;
                item >>= 8;
            }
            if (IsBigEndian)
                array=array.Reverse().ToArray();
            BaseStream.Write(array, 0, 2);
        }

        public void WriteUInt16(ushort item)
        {
            byte[] array = new byte[2];
            for (int i = 0; i <2; i++)
            {
                array[i] = (byte)item;
                item >>= 8;
            }
            if (IsBigEndian)
                array=array.Reverse().ToArray();
            BaseStream.Write(array, 0, 2);
        }

        public void WriteInt64(long item)
        {
            byte[] array = new byte[8];
            for (int i = 0; i <8; i++)
            {
                array[i] = (byte)item;
                item >>= 8;
            }
            if (IsBigEndian)
                array=array.Reverse().ToArray();
            BaseStream.Write(array, 0, 8);
        }

        public void WriteUInt64(ulong item)
        {
            byte[] array = new byte[8];
            for (int i = 0; i <8; i++)
            {
                array[i] = (byte)item;
                item >>= 8;
            }
            if (IsBigEndian)
                array=array.Reverse().ToArray();
            BaseStream.Write(array, 0, 8);
        }

        public void WriteBytes(byte[] item)
        {
            BaseStream.Write(item, 0, item.Length);
        }

        public void WriteMarshal(object structure)
        {
            int size = Marshal.SizeOf(structure);
            IntPtr ptr= Marshal.AllocHGlobal(size);
            byte[] buffer = new byte[size];
            Marshal.StructureToPtr(structure, ptr, false);
            Marshal.Copy(ptr, buffer, 0, size);
            Marshal.FreeHGlobal(ptr);
            WriteBytes(buffer);
        }

        public void Close()
        {
            BaseStream.Close();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
