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
                        var packetToSend = NtpPacket.NewBuilder()
                            .SetLeapInducator(0)
                            .SetProtocolMode(NtpPacket.NtpProtocolMode.Server)
                            .SetStratum(1)
                            .SetReferenceTimestamp(NtpTimeStamp.FromDateTime(DateTime.UtcNow))
                            .SetOriginateTimestamp(receivedPacket.TransmitTimestamp)
                            .SetReceiveTimestamp(NtpTimeStamp.FromDateTime(receivedTime))
                            .SetTransmitTimestamp(NtpTimeStamp.FromDateTime(DateTime.UtcNow)).Build();
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