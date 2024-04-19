using System.Text;
using static HNIdesu.Net.Ntp.NtpPacket;

namespace HNIdesu.Net.Ntp
{
    public struct NtpTimeStamp(uint seconds, uint fraction = 0)
    {
        private static readonly long UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        public uint Seconds = seconds;
        public uint Fraction = fraction;

        public static NtpTimeStamp FromDateTime(DateTime dateTime)
        {
            double seconds = (dateTime.Ticks- UnixEpoch) / (double)TimeSpan.TicksPerSecond+2208988800;
            uint second = (uint)seconds;
            uint fraction =(uint)((seconds - second) * 0x100000000);
            return new NtpTimeStamp(second, fraction);
        }
        public readonly DateTime ToDateTime() {
            double x = Seconds;
            x -= 2208988800;
            x += Fraction / (double)0x100000000;
            return new DateTime(Convert.ToInt64(x * TimeSpan.TicksPerSecond + UnixEpoch));
        } 
    }

    public struct NtpShortTimeFormat(ushort seconds, ushort fraction = 0)
    {
        public ushort Seconds = seconds;
        public ushort Fraction = fraction;

        public readonly TimeSpan ToTimeSpan()
        {
            double second = Seconds;
            second += Fraction / (double)0x10000;
            return new TimeSpan(Convert.ToInt64(second * TimeSpan.TicksPerSecond));
        }
    }
    public class NtpPacket(
        byte leapInducator, 
        byte versionNumber,
        NtpProtocolMode protocolMode,
        byte stratum,
        byte pool,
        sbyte precision,
        NtpShortTimeFormat rootDelay,
        NtpShortTimeFormat rootDispersion,
        byte[] referenceIdentifier,
        NtpTimeStamp referenceTimestamp,
        NtpTimeStamp originateTimestamp,
        NtpTimeStamp receiveTimestamp,
        NtpTimeStamp transmitTimestamp
    )
    {
        public enum NtpProtocolMode
        { Reserved, SymmetricActive, SymmetricPassive, Client, Server, Broadcast, NtpControl, ReservedForPrivateUsage }

        private byte _LeapInducator = leapInducator;
        private byte _VersionNumber = versionNumber;
        private byte _ProtocolMode = (byte)protocolMode;
        private byte _Stratum = stratum;
        private byte _Poll = pool;
        private sbyte _Precision = precision;
        private NtpShortTimeFormat _RootDelay = rootDelay;
        private NtpShortTimeFormat _RootDispersion = rootDispersion;
        private readonly byte[] _ReferenceIdentifier = referenceIdentifier;
        private NtpTimeStamp _ReferenceTimestamp = referenceTimestamp;
        private NtpTimeStamp _OriginateTimestamp = originateTimestamp;
        private NtpTimeStamp _ReceiveTimestamp = receiveTimestamp;
        private NtpTimeStamp _TransmitTimestamp = transmitTimestamp;

        public static Builder NewBuilder()=> new();

        public class Builder
        {
            private byte _LeapInducator = 0;
            private readonly byte _VersionNumber = 4;
            private NtpProtocolMode _ProtocolMode = NtpProtocolMode.Client;
            private byte _Stratum = 1;
            private byte _Poll = 4;
            private sbyte _Precision= -6;
            private NtpShortTimeFormat _RootDelay;
            private NtpShortTimeFormat _RootDispersion;
            private readonly byte[] _ReferenceIdentifier = new byte[4];
            private NtpTimeStamp? _ReferenceTimestamp;
            private NtpTimeStamp? _OriginateTimestamp;
            private NtpTimeStamp? _ReceiveTimestamp;
            private NtpTimeStamp? _TransmitTimestamp;

            public Builder(){}
            public Builder SetReferenceTimestamp(NtpTimeStamp referenceTimestamp)
            {
                _ReferenceTimestamp = referenceTimestamp;
                return this;
            }
            public Builder SetOriginateTimestamp(NtpTimeStamp originateTimestamp)
            {
                _OriginateTimestamp = originateTimestamp;
                return this;
            }
            public Builder SetReceiveTimestamp(NtpTimeStamp receiveTimestamp)
            {
                _ReceiveTimestamp = receiveTimestamp;
                return this;
            }

            public Builder SetTransmitTimestamp(NtpTimeStamp transmitTimestamp)
            {
                _TransmitTimestamp = transmitTimestamp;
                return this;
            }
            public Builder SetLeapInducator(byte leapInducator)
            {
                _LeapInducator = leapInducator;
                return this;
            }

            public Builder SetReferenceIdentifier(byte[] referenceIdentifier)
            {
                int length = referenceIdentifier.Length;
                if (length > 4)
                    length = 4;
                referenceIdentifier.CopyTo(new Span<byte>(_ReferenceIdentifier, 0, length));
                return this;
            }

            public Builder SetRootDispersion(NtpShortTimeFormat rootDispersion)
            {
                _RootDispersion = rootDispersion;
                return this;
            }

            public Builder SetPool(byte pool)
            {
                _Poll = pool;
                return this;
            }

            public Builder SetPrecision(sbyte precision)
            {
                _Precision = precision;
                return this;
            }

            public Builder SetStratum(byte stratum)
            {
                _Stratum = stratum;
                return this;
            }

            public Builder SetProtocolMode(NtpProtocolMode protocolMode)
            {
                _ProtocolMode = protocolMode;
                return this;
            }

            public Builder SetRootDelay(NtpShortTimeFormat rootDelay)
            {
                _RootDelay = rootDelay;
                return this;
            }

            public NtpPacket Build()
            {
                var originateTimestamp = _OriginateTimestamp;
                if (originateTimestamp == null)
                {
                    if (_ProtocolMode == NtpProtocolMode.Client)
                        originateTimestamp = NtpTimeStamp.FromDateTime(DateTime.UtcNow);
                    else
                        throw new Exception("OriginateTimestamp must be set in serve mode");
                }
                var referenceTimestamp = _ReferenceTimestamp ?? new NtpTimeStamp();
                var receiveTimestamp = _ReceiveTimestamp;
                if (receiveTimestamp == null)
                {
                    if (_ProtocolMode == NtpProtocolMode.Client)
                        receiveTimestamp = new NtpTimeStamp();
                    else
                        throw new Exception("ReceiveTimestamp must be set in server mode");
                }
                var transmitTimestamp = _TransmitTimestamp;
                if (transmitTimestamp == null)
                {
                    if (_ProtocolMode == NtpProtocolMode.Client)
                        transmitTimestamp = new NtpTimeStamp();
                    else
                        transmitTimestamp = NtpTimeStamp.FromDateTime(DateTime.UtcNow);
                }
                return new NtpPacket(
                    _LeapInducator, 
                    _VersionNumber, 
                    _ProtocolMode, 
                    _Stratum, 
                    _Poll, 
                    _Precision, 
                    _RootDelay, 
                    _RootDispersion, 
                    _ReferenceIdentifier, 
                    referenceTimestamp,
                    originateTimestamp.Value, 
                    receiveTimestamp.Value, 
                    transmitTimestamp.Value);
            }
        }

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
        public sbyte Precision
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
            using var br = new IO.BinaryReader(stream) { IsBigEndian = true };
            int b = br.ReadByte();
            var leapInducator = (byte)(b >> 6);
            var versionNumber = (byte)((b & 0x38) >> 3);
            if (versionNumber != 4)
                throw new NotSupportedException("Version number not supported");
            var protocolMode = (byte)((b & 0xC0) >> 6);
            var stratum = (byte)br.ReadByte();
            var poll = (byte)br.ReadByte();
            return NewBuilder()
                .SetLeapInducator(leapInducator)
                .SetStratum(stratum)
                .SetProtocolMode((NtpProtocolMode)protocolMode)
                .SetPool(poll)
                .SetPrecision((sbyte)br.ReadByte())
                .SetRootDelay(new NtpShortTimeFormat(br.ReadUInt16(), br.ReadUInt16()))
                .SetRootDispersion(new NtpShortTimeFormat(br.ReadUInt16(), br.ReadUInt16()))
                .SetReferenceIdentifier(br.ReadBytes(4))
                .SetReferenceTimestamp(new NtpTimeStamp(br.ReadUInt32(), br.ReadUInt32()))
                .SetOriginateTimestamp(new NtpTimeStamp(br.ReadUInt32(), br.ReadUInt32()))
                .SetReceiveTimestamp(new NtpTimeStamp(br.ReadUInt32(), br.ReadUInt32()))
                .SetTransmitTimestamp(new NtpTimeStamp(br.ReadUInt32(), br.ReadUInt32()))
                .Build();
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
                bw.WriteByte((byte)_Precision);
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
