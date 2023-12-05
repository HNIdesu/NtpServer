using System.Text;

namespace HNIdesu.Net.Ntp
{
    public struct NtpTimeStamp
    {
        private static readonly long UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        public uint Seconds;
        public uint Fraction;
        public NtpTimeStamp(uint seconds,uint fraction = 0)
        {
            Seconds = seconds;
            Fraction = fraction;
        }
        public static NtpTimeStamp FromDateTime(DateTime dateTime)
        {
            double seconds = (dateTime.Ticks- UnixEpoch) / (double)TimeSpan.TicksPerSecond+2208988800;
            uint second = (uint)seconds;
            uint fraction =(uint)((seconds - second) * 0x100000000);
            return new NtpTimeStamp(second, fraction);
        }
        public DateTime ToDateTime() {
            double x = Seconds;
            x -= 2208988800;
            x += Fraction / (double)0x100000000;
            return new DateTime(Convert.ToInt64(x * TimeSpan.TicksPerSecond + UnixEpoch));
        } 
    }

    public struct NtpShortTimeFormat
    {
        public ushort Seconds;
        public ushort Fraction;
        public NtpShortTimeFormat(ushort seconds, ushort fraction = 0)
        {
            Seconds = seconds;
            Fraction = fraction;
        }

        public TimeSpan ToTimeSpan()
        {
            double second = Seconds;
            second += Fraction / (double)0x10000;
            return new TimeSpan(Convert.ToInt64(second * TimeSpan.TicksPerSecond));
        }
    }
    public class NtpPacket
    {
        public enum NtpProtocolMode
        { Reserved, SymmetricActive, SymmetricPassive, Client, Server, Broadcast, NtpControl, ReservedForPrivateUsage }
        
        private byte _LeapInducator;
        private byte _VersionNumber=4;
        private byte _ProtocolMode;
        private byte _Stratum;
        private byte _Poll;
        private byte _Precision;
        private NtpShortTimeFormat _RootDelay;
        private NtpShortTimeFormat _RootDispersion;
        private readonly byte[] _ReferenceIdentifier = new byte[4];
        private NtpTimeStamp _ReferenceTimestamp;
        private NtpTimeStamp _OriginateTimestamp;
        private NtpTimeStamp _ReceiveTimestamp;
        private NtpTimeStamp _TransmitTimestamp;

        /// <summary>
        /// Time at the server when the request arrived from the client, in NTP timestamp format.
        /// </summary>
        public NtpTimeStamp ReceiveTimestamp
        {
            get => _ReceiveTimestamp;
            set => _ReceiveTimestamp=value;
        }
        /// <summary>
        /// Time at the server when the response left for the client, in NTP timestamp format.
        /// </summary>
        public NtpTimeStamp TransmitTimestamp
        {
            get => _TransmitTimestamp;
            set => _TransmitTimestamp =value;
        }
        /// <summary>
        /// Time at the client when the request departed for the server, in NTP timestamp format.
        /// </summary>
        public NtpTimeStamp OriginateTimestamp
        {
            get => _OriginateTimestamp;
            set => _OriginateTimestamp = value;
        }
        /// <summary>
        /// Time when the system clock was last set or corrected, in NTP timestamp format.
        /// </summary>
        public NtpTimeStamp ReferenceTimestamp
        {
            get => _ReferenceTimestamp;
            set => _ReferenceTimestamp =value;
        }
        /// <summary>
        /// 
        /// </summary>
        public string ReferenceIdentifier
        {
            get => Encoding.ASCII.GetString(_ReferenceIdentifier);
            set
            {
                byte[] source = Encoding.ASCII.GetBytes(value);
                if (source.Length > 0 && source.Length <= 4)
                    Encoding.ASCII.GetBytes(value).CopyTo(_ReferenceIdentifier, 0);
                else
                    throw new NotSupportedException("The reference identifier must not exceed 4 characters");
            }
        }

        /// <summary>
        /// Total dispersion to the reference clock
        /// </summary>
        public NtpShortTimeFormat RootDispersion
        {
            get => _RootDispersion;
            set => _RootDispersion = value;
        }
        /// <summary>
        /// Total round-trip delay to the reference clock
        /// </summary>
        public NtpShortTimeFormat RootDelay
        {
            get => _RootDelay;
            set => _RootDelay = value;
        }
        /// <summary>
        /// Unknown
        /// </summary>
        public byte Precision
        {
            get => _Precision;
            set => _Precision = value;
        }
        /// <summary>
        /// Unknown
        /// </summary>
        public byte Poll
        {
            get => _Poll;
            set => _Poll = value;
        }

        /// <summary>
        /// Warning of an impending leap second to be inserted or deleted in the last minute of the current month
        /// 0: no warning
        /// 1: last minute of the day has 61 seconds
        /// 2: last minute of the day has 59 seconds
        /// 3: unknown (clock unsynchronized)  
        /// </summary>
        public byte LeapInducator
        {
            get => _LeapInducator;
            set => _LeapInducator = value;
        }

        /// <summary>
        /// Version number of ntp protcol
        /// </summary>
        public byte VersionNumber
        {
            get => _VersionNumber;
            set => _VersionNumber = value;
        }

        /// <summary>
        /// Stratum
        /// 0: unspecified or invalid
        /// 1: primary server (e.g., equipped with a GPS receiver)
        /// 2-15: secondary server (via NTP)
        /// 16: unsynchronized
        /// 17-255: reserved
        /// </summary>
        public byte Stratum
        {
            get => _Stratum;
            set => _Stratum = value;
        }

        public NtpProtocolMode ProtocolMode
        {
            get => (NtpProtocolMode)_ProtocolMode;
            set => _ProtocolMode = (byte)value;
        }


        public static NtpPacket Parse(Stream stream)
        {
            NtpPacket packet = new NtpPacket();
            using (var br=new IO.BinaryReader(stream) { IsBigEndian=true})
            {
                int b = br.ReadByte();
                packet._LeapInducator = (byte)(b >> 6);
                packet._VersionNumber = (byte)((b & 0x38) >> 3);
                packet._ProtocolMode = (byte)((b & 0xC0) >> 6);
                packet._Stratum = (byte)br.ReadByte();
                packet._Poll = (byte)br.ReadByte();
                packet._Precision = (byte)br.ReadByte();
                packet._RootDelay = new NtpShortTimeFormat(br.ReadUInt16(), br.ReadUInt16());
                packet._RootDispersion = new NtpShortTimeFormat(br.ReadUInt16(), br.ReadUInt16());
                br.ReadBytes(4).CopyTo(packet._ReferenceIdentifier,0);
                packet._ReferenceTimestamp = new NtpTimeStamp(br.ReadUInt32(), br.ReadUInt32());
                packet._OriginateTimestamp= new NtpTimeStamp(br.ReadUInt32(), br.ReadUInt32());
                packet._ReceiveTimestamp= new NtpTimeStamp(br.ReadUInt32(), br.ReadUInt32());
                packet._TransmitTimestamp= new NtpTimeStamp(br.ReadUInt32(), br.ReadUInt32());
                return packet;
            }   
        }

        public static NtpPacket Parse(byte[] data)=>Parse(new MemoryStream(data));

        public byte[] ToBytes()
        {
            byte[] buffer=new byte[48];
            using (var bw=new IO.BinaryWriter(new MemoryStream(48)))
            {
                bw.IsBigEndian = true;
                bw.WriteByte((byte)((_LeapInducator << 6) | (_VersionNumber << 3) | _ProtocolMode));
                bw.WriteByte(_Stratum);
                bw.WriteByte(_Poll);
                bw.WriteByte(_Precision);
                bw.WriteUInt16(_RootDelay.Seconds);
                bw.WriteUInt16(_RootDelay.Fraction);
                bw.WriteUInt16(_RootDispersion.Seconds);
                bw.WriteUInt16(_RootDispersion.Fraction);
                bw.WriteBytes(_ReferenceIdentifier);
                bw.WriteUInt32(_ReferenceTimestamp.Seconds);
                bw.WriteUInt32(_ReferenceTimestamp.Fraction);
                bw.WriteUInt32(_OriginateTimestamp.Seconds);
                bw.WriteUInt32(_OriginateTimestamp.Fraction);
                bw.WriteUInt32(_ReceiveTimestamp.Seconds);
                bw.WriteUInt32(_ReceiveTimestamp.Fraction);
                bw.WriteUInt32(_TransmitTimestamp.Seconds);
                bw.WriteUInt32(_TransmitTimestamp.Fraction);
                bw.BaseStream.Seek(0, SeekOrigin.Begin);
                bw.BaseStream.Read(buffer);
            }
            return buffer;
        }
    }
}
