using System.Net;
using System.Net.Sockets;

namespace HNIdesu.Net.Ntp
{
    public class NtpServer:UdpClient
    {
        public bool IsWorking { get; private set; }
        public NtpServer(int port=123):base(port)
        {

        }

        public void Start()
        {
            BeginReceive(OnReceiveData, null);
            IsWorking = true;
        }

        public void Stop()
        {
            IsWorking = false;
        }
        public void OnReceiveData(IAsyncResult result){
            IPEndPoint? endPoint = IPEndPoint.Parse("127.0.0.1:123");
            try
            {
                byte[] buffer = EndReceive(result, ref endPoint);
                if (buffer.Length == 48)
                {
                    Console.WriteLine("Ntp packet received");
                    DateTime receivedTime = DateTime.UtcNow;
                    try
                    {
                        NtpPacket receivedPacket = NtpPacket.Parse(buffer);
                        NtpPacket packetToSend = new NtpPacket()
                        {
                            LeapInducator = 0,
                            VersionNumber = 4,
                            ProtocolMode = NtpPacket.NtpProtocolMode.Server,
                            Stratum = 1,
                            ReferenceTimestamp = NtpTimeStamp.FromDateTime(DateTime.UtcNow),
                            OriginateTimestamp = receivedPacket.TransmitTimestamp,
                            ReceiveTimestamp = NtpTimeStamp.FromDateTime(receivedTime),
                            TransmitTimestamp = NtpTimeStamp.FromDateTime(DateTime.UtcNow)
                        };
                        Send(packetToSend.ToBytes(), endPoint);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error caught:{e.Message}");

                    }
                }
            }
            catch (Exception)
            {

            }
            
            if (IsWorking)
                BeginReceive(OnReceiveData, null);
        }


    }
}